using MeshGeneration;
using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Streets
{

    public class Street : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField]
        [Tooltip("The Amount of Segments to Draw")]
        [Range(1, 256)]
        private int segments = 15;
        [SerializeField]
        private bool drawMesh = true;
        [SerializeField]
        private bool updateSpline = false;
        [SerializeField]
        [Tooltip("Draw the Curve?")]
        private bool drawLine = false;
        [SerializeField]
        [Tooltip("Draw the Tangent?")]
        private bool drawTangent = false;
        [SerializeField]
        [Tooltip("Draw the Normal Up?")]
        private bool drawNormalUp = false;
        [SerializeField]
        [Tooltip("Draw the Normal?")]
        private bool drawNormal = false;
        [SerializeField]
        [Tooltip("Tangent Lenght")]
        private float tangentLenght = 1;
        [SerializeField]
        [Tooltip("Normal Up Lenght")]
        private float normalLenghtUp = 1;
        [SerializeField]
        [Tooltip("Normal Lenght")]
        private float normalLenght = 1;

        [SerializeField]
        private bool drawGridNormals = false;

        public Spline m_Spline;
        public MeshFilter m_MeshFilterRef;
        public MeshRenderer m_MeshRendererRef;
        public ExtrudeShapeBase m_Shape;

        public Street m_SplineConnect_Start;
        public bool m_StartIsConnectable
        {
            get
            {
                if (m_SplineConnect_Start == null || m_SplineConnect_Start.ID < 0)
                    return true;
                return false;
            }
        }
        public Street m_SplineConnect_End;
        public bool m_EndIsConnectable
        {
            get
            {
                if (m_SplineConnect_End == null || m_SplineConnect_End.ID < 0)
                    return true;
                return false;
            }
        }

        public Vector3 m_MeshOffset
        {
            get
            {
                return transform.position;
            }
        }

        [MyReadOnly]
        [SerializeField]
        protected int id = -1;

        public int ID
        {
            get
            {
                return id;
            }
        }

        private bool lastDrawMeshSetting;
        private int lastSegmentCount;

        public Street Init(GameObject _startPos, GameObject _tangent1, GameObject _tangent2, GameObject _endPos, int _segments, MeshFilter _meshFilter, MeshRenderer _meshRenderer, ExtrudeShapeBase _shape, bool _updateMesh = false, bool _needID = true, Street _connectionStart = null, bool _connectionStartIsOtherStart = true, Street _connectionEnd = null, bool _connectionEndIsOtherStart = true)
        {
            m_Spline = new Spline(_startPos, _tangent1, _tangent2, _endPos, _segments);
            m_MeshFilterRef = _meshFilter;
            m_MeshRendererRef = _meshRenderer;
            m_Shape = _shape;
            MeshGenerator.Extrude(this);
            updateSpline = _updateMesh;
            if (_needID)
            {
                id = StreetManager.GetNewSplineID();

                if (_connectionStart == null)
                    CreateDeadEnd(true);
                else
                    Combine(_connectionStart, true, _connectionStartIsOtherStart);

                if (_connectionEnd == null)
                    CreateDeadEnd(false);
                else
                    Combine(_connectionEnd, false, _connectionEndIsOtherStart);
            }
            return this;
        }

        public void RemoveDeadEnd(bool _isStart, Street _newStreetRef)
        {
            if (_isStart && m_SplineConnect_Start != null && m_SplineConnect_Start is DeadEnd)
            {
                Destroy(m_SplineConnect_Start.gameObject);
                m_SplineConnect_Start = _newStreetRef;
            }
            else
            if (!_isStart && m_SplineConnect_End != null && m_SplineConnect_End is DeadEnd)
            {
                Destroy(m_SplineConnect_End.gameObject);
                m_SplineConnect_End = _newStreetRef;
            }
        }

        public void CreateDeadEnd(bool isStart)
        {
            if (isStart && m_StartIsConnectable)
            {
                GameObject tmp = new GameObject("DeadEnd_Start");
                m_SplineConnect_Start = tmp.AddComponent<DeadEnd>();
                DeadEnd de = (DeadEnd)m_SplineConnect_Start;
                de.Init(new DeadEndShape(), this, true);
            }
            else
            if (!isStart && m_EndIsConnectable)
            {
                GameObject tmp = new GameObject("DeadEnd_End");
                m_SplineConnect_End = tmp.AddComponent<DeadEnd>();
                DeadEnd de = (DeadEnd)m_SplineConnect_End;
                de.Init(new DeadEndShape(), this, false);
            }
        }

        public bool Combine(Street _otherStreet, bool _isMyStart, bool _otherStreetIsStart)
        {
            if (_otherStreet == null) return false;

            RemoveDeadEnd(_isMyStart, _otherStreet); //Remove my DeadEnd

            //Set Ref of other Street
            if (_otherStreetIsStart)
                _otherStreet.m_SplineConnect_Start = this;
            else
                _otherStreet.m_SplineConnect_End = this;

            //Set my Ref
            if (_isMyStart)
            {
                m_SplineConnect_Start = _otherStreet;
                return true;
            }
            else if (!_isMyStart)
            {
                m_SplineConnect_End = _otherStreet;
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (lastDrawMeshSetting != drawMesh || updateSpline)
            {
                if (updateSpline)
                    m_Spline.UpdateOPs();

                if (drawMesh)
                    MeshGenerator.Extrude(this);
                else
                    m_MeshFilterRef.mesh.Clear();
            }

            lastDrawMeshSetting = drawMesh;

            if (lastSegmentCount != segments)
            {
                m_Spline.segments = segments;
                m_Spline.UpdateOPs();
                MeshGenerator.Extrude(this);
            }

            lastSegmentCount = segments;
        }

        private void OnDrawGizmos()
        {
            if (drawGridNormals)
            {
                for (int i = 0; i < m_Spline.GridOPs.Length; i++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(m_Spline.GridOPs[i].Position, m_Spline.GridOPs[i].Position + m_Spline.GetNormalUpAt(m_Spline.GridOPs[i].t) * normalLenghtUp);
                }
            }
            for (int i = 0; i < m_Spline.OPs.Length; i++)
            {
                float t = 1.0f / segments * i;
                float tnext = 1.0f / segments * (i + 1);

                if (!(i + 1 >= m_Spline.OPs.Length))
                {
                    if (drawLine)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i + 1].Position);
                    }
                }
                if (drawTangent)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i].Position + m_Spline.GetTangentAt(t) * tangentLenght);
                }
                if (drawNormalUp)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i].Position + m_Spline.GetNormalUpAt(t) * normalLenghtUp);
                }
                if (drawNormal)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i].Position + m_Spline.GetNormalAt(t) * normalLenght);
                }
            }
        }
    }
}