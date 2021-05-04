using Grid;
using MeshGeneration;
using MyCustomCollsion;
using Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<Vector2, Cell> m_StreetCells = new Dictionary<Vector2, Cell>();
        public GameObject m_GridObj;
        private Street m_collisionStreet;

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

        private bool m_inValidForm = true;
        public bool m_HasValidForm
        {
            get
            {
                return m_inValidForm;
            }
            set
            {
                m_inValidForm = value;
                if (value)
                {
                    StreetManager.SetStreetColor(this, Color.green);
                }
                else
                {
                    StreetManager.SetStreetColor(this, Color.red);
                }
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

        private List<Vector3> m_segmentsCorner = new List<Vector3>();

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
            m_Spline = new Spline(_startPos, _tangent1, _tangent2, _endPos, _segments, this);
            m_MeshFilterRef = _meshFilter;
            m_MeshRendererRef = _meshRenderer;
            m_Shape = _shape;
            m_Spline.UpdateOPs(this);
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

        public void AddSegmentsCorner(Vector3 _input)
        {
            if (!m_segmentsCorner.Contains(_input))
            {
                m_segmentsCorner.Add(_input);
            }
        }

        public void SetCollisionStreet(Street _collStreet)
        {
            m_collisionStreet = _collStreet;
        }

        public Street GetCollisionStreet()
        {
            return m_collisionStreet;
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
            if (gameObject.layer == 8) return;
            if (isStart && m_StartIsConnectable)
            {
                //Look for maybe already created DeadEnd
                Transform t = transform.Find("DeadEnd_Start");
                GameObject tmp;
                if (t == null) //if there is no DeadEnd GameObject -> Create
                {
                    tmp = new GameObject("DeadEnd_Start");
                    tmp.tag = "Street";
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
                    tmp.tag = "Street";
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

        public void CheckCollision()
        {
            if (ID > 0)
                m_Spline.UpdateOPs(this);
            List<Cell> cellsToCheck = new List<Cell>();
            List<StreetSegment> segments = new List<StreetSegment>();
            int segmentsAmount = (m_segmentsCorner.Count - 2) / 2;
            int offset = 0;
            for (int i = 0; i < segmentsAmount; i++)
            {
                Vector3[] segmentCorner = new Vector3[4];
                segmentCorner[0] = m_segmentsCorner[offset];
                segmentCorner[1] = m_segmentsCorner[offset + 1];
                segmentCorner[2] = m_segmentsCorner[offset + 2];
                segmentCorner[3] = m_segmentsCorner[offset + 3];
                offset += 2;
                StreetSegment newSegment = new StreetSegment(this, segmentCorner);
                segments.Add(newSegment);
            }

            HashSet<int> StreetsToRecreate = new HashSet<int>();
            foreach (StreetSegment segment in segments)
            {
                foreach (int i in segment.CheckCollision(GridManager.m_AllCells))
                    StreetsToRecreate.Add(i);
            }

            foreach (int i in StreetsToRecreate)
            {
                Street s = StreetManager.GetStreetByID(i);
                GridManager.RemoveGrid(s);
                MeshGenerator.CreateGridMesh(s.m_StreetCells.Values.ToList(), s.m_GridObj.GetComponent<MeshFilter>());
            }
        }

        public void ClrearSegmentsCorner()
        {
            m_segmentsCorner.Clear();
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
                    m_Spline.UpdateOPs(this);

                if (drawMesh)
                    MeshGenerator.Extrude(this);
                else
                    m_MeshFilterRef.mesh.Clear();
            }

            lastDrawMeshSetting = drawMesh;

            if (lastSegmentCount != segments)
            {
                m_Spline.segments = segments;
                m_Spline.UpdateOPs(this);
                MeshGenerator.Extrude(this);
            }

            lastSegmentCount = segments;

        }

        private void OnDrawGizmosSelected()
        {
            if (m_segmentsCorner != null && m_segmentsCorner.Count > 0)
            {
                for (int i = 0; i < m_segmentsCorner.Count; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(m_segmentsCorner[i], 0.2f);
                }

                for (int i = 0; i < m_Spline.OPs.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Vector3 PPos = m_Spline.OPs[i].Position + m_Spline.GetNormalAt(m_Spline.OPs[i].t);
                    Vector3 NPos = m_Spline.OPs[i].Position - m_Spline.GetNormalAt(m_Spline.OPs[i].t);
                    Gizmos.DrawWireCube(PPos, new Vector3(0.2f, 0.2f, 0.2f));
                    Gizmos.DrawWireCube(NPos, new Vector3(0.2f, 0.2f, 0.2f));
                }
            }

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

    public struct StreetSegment
    {
        public Street m_Street { get; private set; }
        public int ID { get { return m_Street.ID; } }

        public Vector2[] m_CornerPos;

        public Vector2 m_Center;

        public float m_CollisionRadius;

        public StreetSegment(Street _steet, Vector3[] _corner)
        {
            m_Street = _steet;
            m_CornerPos = new Vector2[4];
            for (int i = 0; i < m_CornerPos.Length; i++)
            {
                m_CornerPos[i] = new Vector2(_corner[i].x, _corner[i].z);
            }
            float dis02 = Vector3.Distance(_corner[0], _corner[2]);
            float dis01 = Vector3.Distance(_corner[0], _corner[1]);
            if (dis02 > dis01)
            {
                m_CollisionRadius = dis02 * 0.5f;
                Vector3 v = _corner[0] + (_corner[2] - _corner[0]) * 0.5f;
                m_Center = new Vector2(v.x, v.z);
            }
            else
            {
                m_CollisionRadius = dis01 * 0.5f;
                Vector3 v = _corner[0] + (_corner[1] - _corner[0]) * 0.5f;
                m_Center = new Vector2(v.x, v.z);
            }
        }

        public List<int> CheckCollision(List<Cell> _cellsToCheck)
        {
            List<Cell> PolyPolyCheckList = new List<Cell>();
            List<int> StreetIDsToRecreate = new List<int>();
            for (int i = 0; i < _cellsToCheck.Count; i++)
            {
                if (MyCollision.SphereSphere(m_Center, m_CollisionRadius, _cellsToCheck[i].m_PosCenter, _cellsToCheck[i].CellSquareSize))
                {
                    PolyPolyCheckList.Add(GridManager.m_AllCells[i]);
                }
            }

            for (int i = 0; i < PolyPolyCheckList.Count; i++)
            {
                if (MyCollision.PolyPoly(PolyPolyCheckList[i].m_Corner, m_CornerPos))
                {
                    PolyPolyCheckList[i].Delete();
                    int id = PolyPolyCheckList[i].m_Street.ID;
                    if (!StreetIDsToRecreate.Contains(id))
                        StreetIDsToRecreate.Add(id);
                }
            }

            return StreetIDsToRecreate;
        }
    }
}