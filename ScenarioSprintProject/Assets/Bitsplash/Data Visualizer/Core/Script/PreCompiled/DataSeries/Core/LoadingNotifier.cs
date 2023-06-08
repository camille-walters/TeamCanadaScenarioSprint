using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data_Visualizer.Core.Script.PreCompiled.DataSeries.Core
{
    class LoadingNotifier
    {
        public interface ILoadingEventRaiser
        {
            void RaiseStartLoading();
            void RaiseDoneLoading();
            IEnumerable<IDataSeries> CurrentItems { get; }
        }
        HashSet<IDataSeries> mLoadingItems = new HashSet<IDataSeries>();
        ILoadingEventRaiser mEventRaiser;
        bool mStartedLoading = false;

        public LoadingNotifier(ILoadingEventRaiser raiser)
        {
            mEventRaiser = raiser;
        }

        void CleanUnusedItems()
        {
            mLoadingItems.IntersectWith(mEventRaiser.CurrentItems);
        }

        void StartLoading()
        {
            mEventRaiser.RaiseStartLoading();
            mStartedLoading = true;
        }

        void DoneLoading()
        {
            mEventRaiser.RaiseDoneLoading();
            mStartedLoading = false;
        }

        public void NotifyStart(IDataSeries series)
        {
            CleanUnusedItems();
            mLoadingItems.Add(series);
            if (mStartedLoading == false)
                StartLoading();
        }

        public void NotifyDone(IDataSeries series)
        {
            CleanUnusedItems();
            mLoadingItems.Remove(series);
            if(mLoadingItems.Count == 0)
            {
                if (mStartedLoading == true)
                    DoneLoading();
            }
        }
    }
}
