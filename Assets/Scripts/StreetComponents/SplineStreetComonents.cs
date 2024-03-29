﻿using MeshGeneration;
using Splines;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    /// <summary>
    /// Class that can be Extruded along the Spline (DeadEnd and Street)
    /// </summary>
    public class SplineStreetComonents : StreetComponent
    {
        public Spline m_Spline;
        public MeshFilter m_MeshFilter;
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
