using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spinner : MaskableGraphic
{

    public float MinRadius;
    public float MaxRadius;
    public float StartAngle;
    public float AngleSpan;
    public float spinSpeed;
    public Color Color;
    const float SegmentCount = 20f;
    Mesh mMesh;
    // Start is called before the first frame update

    void CreateMesh()
    {
        
        List<Vector3> pos = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Color> color = new List<Color>();
        float radAngle = StartAngle * Mathf.Deg2Rad;
        float radSpan = AngleSpan * Mathf.Deg2Rad;
        for (int i = 0; i < SegmentCount; i++)
        {
            float angle = (((float)i) / (SegmentCount-1)) * radSpan + radAngle;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 v1 = new Vector3(cos * MinRadius, sin * MinRadius, 0f);
            Vector3 v2 = new Vector3(cos * MaxRadius, sin * MaxRadius, 0f);
            pos.Add(v1);
            pos.Add(v2);
            uv.Add(new Vector2((v1.x + MaxRadius) / (2 * MaxRadius), (v1.y + MaxRadius) / (2 * MaxRadius)));
            uv.Add(new Vector2((v2.x + MaxRadius) / (2 * MaxRadius), (v2.y + MaxRadius) / (2 * MaxRadius)));
            color.Add(Color);
            color.Add(Color);
        }
        List<int> tringles = new List<int>();
        for(int i=0; i<SegmentCount-1; i++)
        {
            tringles.Add(i * 2);
            tringles.Add(i * 2 + 1);
            tringles.Add(i * 2 + 2);
            
            tringles.Add(i * 2 + 1);
            tringles.Add(i * 2 + 2);
            tringles.Add(i * 2 + 3);
        }
        mMesh = new Mesh();
        mMesh.SetVertices(pos);
        mMesh.SetUVs(0, uv);
        mMesh.SetColors(color);
        mMesh.SetTriangles(tringles, 0);

    }

    protected override void UpdateGeometry()
    {
        if (mMesh == null)
            CreateMesh();
        var rend = GetComponent<CanvasRenderer>();
        rend.SetMesh(mMesh);
       // rend.SetMaterial(materialForRendering,0);
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }
}
