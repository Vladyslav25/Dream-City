using MeshGeneration;
using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class DeadEnd : SplineStreetComonents
    {
        public bool isStart;

        /// <summary>
        /// Initialize the DeadEnd
        /// </summary>
        /// <param name="_shape">The Shape of the DeadEnd</param>
        /// <param name="_streetRef">The Ref of the Street it is connected to</param>
        /// <param name="_isStart">Is it the Start of the Street?</param>
        /// <returns>return the DeadEnd Class</returns>
        public DeadEnd Init(Street _streetRef, bool _isStart, OrientedPoint _op)
        {
            base.Init(false);
            ID = -2;
            m_Shape = new DeadEndShape();
            isStart = _isStart;
            if (isStart)
            {
                //Move the Start Point half the width of the Street | Street width is 2
                Vector3 start = _op.Position + (_op.Rotation * Vector3.right) * 0.5f;
                //Move the Tangent 1 up above the start Point
                Vector3 tangent1 = start - _op.Rotation * Vector3.forward;
                //Move the End Point half the width of the Street | Street width is 2
                Vector3 end = _op.Position + (_op.Rotation * -Vector3.right) * 0.5f;
                //Move the Tangent 2 up above the end Point
                Vector3 tangent2 = end - _op.Rotation * -Vector3.forward;
                m_Spline = new Spline(
                    start, tangent1, tangent2, end, 10, _streetRef
                    );
            }
            m_MeshFilter = gameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_MeshRenderer.material = StreetComponentManager.Instance.DeadEndMat;
            transform.SetParent(_streetRef.transform);
            MeshGenerator.Extrude(this);
            return this;
        }
    }
}
