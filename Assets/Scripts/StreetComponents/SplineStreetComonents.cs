using MeshGeneration;
using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class SplineStreetComonents : StreetComponent
    {
        public Spline m_Spline;
        public MeshFilter m_MeshFilter;
        public MeshRenderer m_MeshRenderer;
        public ExtrudeShapeBase m_Shape;
        public Vector3 m_MeshOffset
        {
            get
            {
                return transform.position;
            }
        }
    }
}
