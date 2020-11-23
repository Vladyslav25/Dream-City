using MeshGeneration;
using Splines;
using Streets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadEnd : Street
{
    public Street m_StreetNeighbour;
    public bool isStart;

    /// <summary>
    /// Initialize the DeadEnd
    /// </summary>
    /// <param name="_shape">The Shape of the DeadEnd</param>
    /// <param name="_streetRef">The Ref of the Street it is connected to</param>
    /// <param name="_isStart">Is it the Start of the Street?</param>
    /// <returns>return the DeadEnd Class</returns>
    public DeadEnd Init(ExtrudeShapeBase _shape, Street _streetRef, bool _isStart)
    {
        m_Shape = _shape;
        m_StreetNeighbour = _streetRef;
        isStart = _isStart;
        if (isStart)
        {
            //Move the Start Point half the width of the Street | Street width is 2
            Vector3 start = _streetRef.m_Spline.StartPos + _streetRef.m_Spline.GetNormalAt(0) * 0.5f;
            //Move the Tangent 1 up above the start Point
            Vector3 tangent1 = start - _streetRef.m_Spline.GetTangentAt(0);
            //Move the End Point half the width of the Street | Street width is 2
            Vector3 end = _streetRef.m_Spline.StartPos - _streetRef.m_Spline.GetNormalAt(0) * 0.5f;
            //Move the Tangent 2 up above the end Point
            Vector3 tangent2 = end - _streetRef.m_Spline.GetTangentAt(0);
            m_Spline = new Spline(
                start, tangent1, tangent2, end, 10, gameObject
                );
        }
        else
        {
            Vector3 start = _streetRef.m_Spline.EndPos - _streetRef.m_Spline.GetNormalAt(1) * 0.5f;
            Vector3 tangent1 = start + _streetRef.m_Spline.GetTangentAt(1);
            Vector3 end = _streetRef.m_Spline.EndPos + _streetRef.m_Spline.GetNormalAt(1) * 0.5f;
            Vector3 tangent2 = end + _streetRef.m_Spline.GetTangentAt(1);
            m_Spline = new Spline(
                start, tangent1, tangent2, end, 10, gameObject
                );
        }
        m_MeshFilterRef = gameObject.AddComponent<MeshFilter>();
        m_MeshRendererRef = gameObject.AddComponent<MeshRenderer>();
        m_MeshRendererRef.material = StreetManager.Instance.DeadEndMat;
        transform.SetParent(m_StreetNeighbour.transform);
        MeshGenerator.Extrude(this);
        return this;
    }

}
