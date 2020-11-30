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

        public Street m_StreetConnect_Start;
        public bool m_StartIsConnectable
        {
            get
            {
                if (m_StreetConnect_Start == null || m_StreetConnect_Start.ID < 0)
                    return true;
                return false;
            }
        }
        public Street m_StreetConnect_End;
        public bool m_EndIsConnectable
        {
            get
            {
                if (m_StreetConnect_End == null || m_StreetConnect_End.ID < 0)
                    return true;
                return false;
            }
        }

        public bool m_EndIsPreview
        {
            get
            {
                if (m_StreetConnect_End != null && m_StreetConnect_End.ID == -1)
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

        /// <summary>
        /// Init the Street. Need to call befor use.
        /// </summary>
        /// <param name="_startPos">Start GameObject of the Spline</param>
        /// <param name="_tangent1">Tangent 1 GameObject of the Spline </param>
        /// <param name="_tangent2">Tangent 2 GameObject of the Spline</param>
        /// <param name="_endPos">End GameObject of the Spline</param>
        /// <param name="_segments">The amount of segments</param>
        /// <param name="_meshFilter">MeshFilter Ref</param>
        /// <param name="_meshRenderer">MeshRenderer Ref</param>
        /// <param name="_shape">The Shape of the Street</param>
        /// <param name="_updateMesh">Update the form of the Street? <strong>Heavy impact on Performance. Use only for the Preview Street</strong></param>
        /// <param name="_needID">Need the Street an id? <strong>If true: Create DeadEnds or Combines</strong></param>
        /// <param name="_connectionStart">Ref to the Street Connection on the Start of the Street. <strong>Is needed if <paramref name="_needID"/> is true</strong></param>
        /// <param name="_connectionStartIsOtherStart">Is it the start of the other Street in the Street start? <strong>Is needed if <paramref name="_needID"/> is true</strong></param>
        /// <param name="_connectionEnd">Ref to the Street connection on the End of the Street. <strong>Is needed if <paramref name="_needID"/> is true</strong></param>
        /// <param name="_connectionEndIsOtherStart">Is it the start of the other Street in the Street end? <strong>Is needed if <paramref name="_needID"/> is true</strong></param>
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

        /// <summary>
        /// Remove a DeadEnd and set the new Ref on the given StreetSide
        /// </summary>
        /// <param name="_isStart">Is it the Start of the Street?</param>
        /// <param name="_newStreetRef">The new Street Ref on the Street Side</param>
        public void RemoveDeadEnd(bool _isStart, Street _newStreetRef)
        {
            if (_isStart && m_StreetConnect_Start != null && m_StreetConnect_Start is DeadEnd)
            {
                Destroy(m_StreetConnect_Start.gameObject);
                m_StreetConnect_Start = _newStreetRef;
            }
            else
            if (!_isStart && m_StreetConnect_End != null && m_StreetConnect_End is DeadEnd)
            {
                Destroy(m_StreetConnect_End.gameObject);
                m_StreetConnect_End = _newStreetRef;
            }
        }

        /// <summary>
        /// Create a DeadEnd on the given Side of the Street
        /// </summary>
        /// <param name="isStart">Is it the Start of the Street?</param>
        public void CreateDeadEnd(bool isStart)
        {
            if (isStart && m_StartIsConnectable)
            {
                //Look for maybe already created DeadEnd
                Transform t = transform.Find("DeadEnd_Start");
                GameObject tmp;
                if (t == null) //if there is no DeadEnd GameObject -> Create
                {
                    tmp = new GameObject("DeadEnd_Start");
                    m_StreetConnect_Start = tmp.AddComponent<DeadEnd>();
                    DeadEnd de = (DeadEnd)m_StreetConnect_Start;
                    de.Init(new DeadEndShape(), this, true);
                }
                else //If a DeadEnd GameObjec exist -> Reset Ref
                {
                    tmp = t.gameObject;
                    m_StreetConnect_Start = tmp.GetComponent<DeadEnd>();
                }
            }
            else
            if (!isStart && m_EndIsConnectable)
            {
                //Look for maybe already created DeadEnd
                Transform t = transform.Find("DeadEnd_End");
                GameObject tmp;
                if (t == null)
                {
                    tmp = new GameObject("DeadEnd_End");
                    m_StreetConnect_End = tmp.AddComponent<DeadEnd>();
                    DeadEnd de = (DeadEnd)m_StreetConnect_End;
                    de.Init(new DeadEndShape(), this, false);
                }
                else
                {
                    tmp = t.gameObject;
                    m_StreetConnect_End = tmp.GetComponent<DeadEnd>();
                }
            }
        }

        public bool Combine(Street _otherStreet, bool _isMyStart, bool _otherStreetIsStart)
        {
            if (_otherStreet == null) return false;

            RemoveDeadEnd(_isMyStart, _otherStreet); //Remove my DeadEnd

            //Set Ref of other Street
            if (_otherStreetIsStart)
                _otherStreet.m_StreetConnect_Start = this;
            else
                _otherStreet.m_StreetConnect_End = this;

            //Set my Ref
            if (_isMyStart)
            {
                m_StreetConnect_Start = _otherStreet;
                return true;
            }
            else if (!_isMyStart)
            {
                m_StreetConnect_End = _otherStreet;
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