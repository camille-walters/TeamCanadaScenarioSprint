using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataVisualizer{
    /// <summary>
    /// the string formatter formats string accroding to the following:
    /// constString<?varname|format>constString
    /// int the above example format can be any format from : https://msdn.microsoft.com/en-us/library/system.string.format(v=vs.110).aspx
    /// for example"
    /// info:<?number,5:N0> or info:<?number:N0> the var name is sperated by the first , or : if it exists
    /// </summary>
    public class StringFormatter
    {
        public static readonly string ParameterSize = "size";
        public static readonly string ParameterXValue = "x";
        public static readonly string ParameterYValue = "y";
        public static readonly string ParameterZValue = "z";
        public static readonly string ParameterEndXValue = "endx";
        public static readonly string ParameterEndYValue = "endy";
        public static readonly string ParameterEndZValue = "endz";
        public static readonly string ParameterHighValue = "high";
        public static readonly string ParameterLowValue = "low";
        public static readonly string ParameterStartValue = "start";
        public static readonly string ParameterEndValue = "end";
        public static readonly string ParameterMaxErrorValue = "maxerror";
        public static readonly string ParameterMinErrorValue = "minerror";
        public static readonly string ParameterUserData = "userdata";
        public static readonly string ParameterCategory = "category";
        public static readonly string ParameterStack = "stack";
        public static readonly string ParameterNewLine = "newline";



        [ThreadStatic]
        static Regex mInnerSeperator;

        public const String DatePrefix = "dateTime-";
        static Regex mSeperator
        {
            get
            {
                if (mInnerSeperator == null)
                    mInnerSeperator = new Regex(@"(?=[,:])");
                return mInnerSeperator;
            }
        }

        /// <summary>
        /// sperator regex pattern used per thread
        /// </summary>
        [ThreadStatic]
        static Regex mInnerVariable;
        /// </summary>
        static Regex mVariable
        {
            get
            {
                if(mInnerVariable == null)
                    mInnerVariable = new Regex(@"\<\?([^<>]*?)\>");
                return mInnerVariable;
            }
        }

        /// <summary>s
        /// holds a list of all complied formats for memory and speed efficiency. when a new format is created , this dictionary is searched to see if it already exist
        /// </summary>
        private static Dictionary<string, StringFormatter> mFormats;

        /// <summary>
        /// the format of this stringFormatter object
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// this is a builder used for building a string from the format. one per thread to save memory
        /// </summary>s
        static StringBuilder mInnerBuild = new StringBuilder();

        static StringBuilder mBuild
        {
            get
            {
                if (mInnerBuild == null)
                    return mInnerBuild;
                return mInnerBuild;
            }
        }

        /// <summary>
        /// A list of actions that formats a string from the values in mArgumentValues
        /// </summary>
        List<Action<IPrivateDataSeriesChart>> mCompiledFormat = new List<Action<IPrivateDataSeriesChart>>();

        /// <summary>
        /// holds the argument values , this value is passed to the method FormatValues and assigned to this memeber. It can then be used by the rest of the class
        /// </summary>
        Dictionary<string, object> mArgumentValues;

        /// <summary>
        /// 
        /// </summary>
        int mIndex;
        public bool IsConstant { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argumentValues"></param>
        /// <returns></returns>
        public string FormatValues(Dictionary<string, object> argumentValues,DataSeriesChart chart)
        {
            var privateChart = (IPrivateDataSeriesChart)chart;
            mArgumentValues = argumentValues;   // set the argument values
            mBuild.Length = 0;  // clear the string
            try
            {
                for (int i = 0; i < mCompiledFormat.Count; i++)   // append all compiled actions
                    mCompiledFormat[i](privateChart);
            }
            catch(Exception)
            {
                mBuild.Clear();
                mBuild.Append("<Invalid Format>");
            }
            mArgumentValues = null;
            return mBuild.ToString();
        }
 
        /// <summary>
        /// Creates a new format object or returns an existing one. formats are cached according to the format string for memeory and performance reasons
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static StringFormatter GetFormat(string format)
        {
            StringFormatter res;
            if (mFormats != null)
            {
                if (mFormats.TryGetValue(format, out res))   // if the format already exists return it
                {
                    ChartCommon.DevLog("add format", "found");
                    return res;
                }
            }
            else
                mFormats = new Dictionary<string, StringFormatter>();
            res = new StringFormatter(format);  // otherwise create a new format
            if (res.IsConstant == false) // we don't store constant formats
            {
                ChartCommon.DevLog("add format", "added " + format);
                mFormats[format] = res; // add the new format to the format cache
            }
            return res; // return the new format
        }
        void AppendDefault(string method,IPrivateDataSeriesChart chart)
        {
            object value;
            if (mArgumentValues.TryGetValue(method, out value))
            {
                if (chart.IsParameterDateType(method))
                {
                    DateTime date = DateUtility.ValueToDate((double)value);
                    mBuild.AppendFormat(chart.DefaultDateFormat.Replace("\\n","\n"), date);
                }
                else
                {
                    mBuild.AppendFormat(chart.DefaultNumberFormat, value);
                }
            }
            else
            {
                mBuild.Append("<NULL>");
            }
        }
        void AppendDateTime(string method, string formats)
        {
            object value;

            if (mArgumentValues.TryGetValue(method, out value))
            {
                DateTime date = DateUtility.ValueToDate((double)value);
                mBuild.AppendFormat(formats, date);
            }
            else
                mBuild.Append("<NULL>");
        }
        /// <summary>
        /// this action appends a variable value to the string builder mBuild. This is meant to be used as part of the array mCompiledFormat
        /// </summary>
        /// <param name="method"></param>
        /// <param name="formats"></param>
        void AppendVariable(string method,string formats)
        {
            object value;

            if (mArgumentValues.TryGetValue(method, out value))
                mBuild.AppendFormat(formats, value);
            else
                mBuild.Append("<NULL>");           
        }

        /// <summary>
        /// this action appends a constant value to the string builder mBuild. This is meant to be used as part of the array mCompiledFormat
        /// </summary>
        /// <param name="method"></param>
        /// <param name="formats"></param>
        void AppendConstant(string constant)
        {
            mBuild.Append(constant);
        }

        /// <summary>
        /// adds a compled action to the mCompiledFormat array
        /// </summary>
        /// <param name="action"></param>
        void AddCompiledAction(Action<IPrivateDataSeriesChart> action)
        {
            mCompiledFormat.Add(action);
        }
        
        /// <summary>
        /// convert a format to a list of actions using regex. 
        /// </summary>
        /// <param name="format"></param>
        void Complie(string format)
        {
            int position = 0;
            mCompiledFormat.Clear();
            IsConstant = true;
          //  ChartCommon.DevLog("add format", "compile");
            foreach (Match m in mVariable.Matches(format)) // find all variables (matching <?var,0:N0> for example see class desc for more)
            {
                IsConstant = false;
                if (position < m.Index)
                {
                    string constStr = format.Substring(position, m.Index - position);   
                    AddCompiledAction((chart) => AppendConstant(constStr)); // append all data before the var as constant string
                }
                position = m.Index + m.Length;   // endPosition]
                int group = 1;
                if (m.Groups.Count <= 1)
                    group = 0;
          //      ChartCommon.DevLog("data", m.Groups[1].Value);
                string item = m.Groups[group].Value;                
                string[] items = mSeperator.Split(item, 2); // split the inside of the barackts to variable name and format
                string arg = null;
                if (items.Length == 0)
                    continue;
                string method = items[0];
                if (items.Length == 1)
                {
                    AddCompiledAction((chart) => AppendDefault(method,chart));
                }
                else
                {
                    arg = "{0" + items[1] + "}";
                    if (method.StartsWith(DatePrefix))
                    {
                        method = method.Substring(DatePrefix.Length);
                        AddCompiledAction((chart) => AppendDateTime(method, arg));
                    }
                    else
                        AddCompiledAction((chart) => AppendVariable(method, arg));
                }
            }
            if(position<format.Length)  // append the rest of the string
            {
                string constStr = format.Substring(position);
                AddCompiledAction((chart) => AppendConstant(constStr));
            }
        }

        /// <summary>
        /// creates a new string formatter
        /// </summary>
        /// <param name="format"></param>
        private StringFormatter(string format)
        {
            Format = format;
            Complie(format);
        }
       
    }
}
