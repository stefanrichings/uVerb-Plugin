using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Triangle : Object {
    public MeshData.Points pts;
    public MeshData.UVs uvs;
    public Transform trans;

    public Triangle (MeshData.Points pts, MeshData.UVs uvs, Transform trans)
    {
        this.pts = pts;
        this.uvs = uvs;
        this.trans = trans;
        UpdateVerts();
    }

    public void UpdateVerts ()
    {
        pts.pt0 = trans.TransformPoint(pts.pt0);
        pts.pt1 = trans.TransformPoint(pts.pt1);
        pts.pt2 = trans.TransformPoint(pts.pt2);
    }
}
