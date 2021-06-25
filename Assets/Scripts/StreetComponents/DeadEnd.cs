using MeshGeneration;
using Splines;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class DeadEnd : SplineStreetComonents
    {
        /// <summary>
        /// Initialize the DeadEnd
        /// </summary>
        /// <param name="_comp">The Street the Dead End is connected to</param>
        /// <param name="_isStart">Is it the Start of the Street?</param>
        /// <param name="_op">The OrientedPoint of the DeadEnd</param>
        /// <returns>The new Dead End</returns>
        public DeadEnd Init(Street _comp, bool _isStart, OrientedPoint _op)
        {
            base.Init(false);
            ID = -2;
            m_Shape = new DeadEndShape();
            SetStartConnection(new Connection(null, this, true));
            if (_isStart)
            {
                //Move the Start Point half the width of the Street | Street width is 2
                Vector3 start = _op.Position + (_op.Rotation * -Vector3.right) * 0.5f;
                //Move the Tangent 1 up above the start Point
                Vector3 tangent1 = start - _op.Rotation * Vector3.forward * 0.8f;
                //Move the End Point half the width of the Street | Street width is 2
                Vector3 end = _op.Position + (_op.Rotation * Vector3.right) * 0.5f;
                //Move the Tangent 2 up above the end Point
                Vector3 tangent2 = end - _op.Rotation * Vector3.forward * 0.8f;
                m_Spline = new Spline(
                    start, tangent1, tangent2, end, 10, this
                    );
                Connection.Combine(GetStartConnection(), _comp.GetStartConnection());
            }
            else
            {
                //Move the Start Point half the width of the Street | Street width is 2
                Vector3 start = _op.Position + (_op.Rotation * Vector3.right) * 0.5f;
                //Move the Tangent 1 up above the start Point
                Vector3 tangent1 = start + _op.Rotation * Vector3.forward * 0.8f;
                //Move the End Point half the width of the Street | Street width is 2
                Vector3 end = _op.Position + (_op.Rotation * -Vector3.right) * 0.5f;
                //Move the Tangent 2 up above the end Point
                Vector3 tangent2 = end + _op.Rotation * Vector3.forward * 0.8f;
                m_Spline = new Spline(
                    start, tangent1, tangent2, end, 10, this
                    );
                Connection.Combine(GetStartConnection(), _comp.m_EndConnection);
            }

            m_MeshFilter = gameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_MeshRenderer.material = StreetComponentManager.Instance.DeadEndMat;
            transform.SetParent(_comp.transform);
            MeshGenerator.Extrude(this);
            return this;
        }

        public DeadEnd Init(Cross _cross, int _index, OrientedPoint _op)
        {
            base.Init(false);
            ID = -2;
            m_Shape = new DeadEndShape();
            SetStartConnection(new Connection(null, this, true));

            Vector3 start = _op.Position + (_op.Rotation * Vector3.right) * 0.5f;
            Vector3 tangent1 = start + _op.Rotation * Vector3.forward * 0.8f;
            Vector3 end = _op.Position - (_op.Rotation * Vector3.right) * 0.5f;
            Vector3 tangent2 = end + _op.Rotation * Vector3.forward * 0.8f;

            m_Spline = new Spline(
                start, tangent1, tangent2, end, 10, this
                );

            m_MeshFilter = gameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
            m_MeshRenderer.material = StreetComponentManager.Instance.DeadEndMat;
            MeshGenerator.Extrude(this);
            Connection.Combine(GetStartConnection(), _cross.m_Connections[_index]);

            return this;
        }
    }
}
