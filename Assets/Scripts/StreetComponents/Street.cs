using Grid;
using MeshGeneration;
using MyCustomCollsion;
using Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class Street : SplineStreetComonents
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

        public Dictionary<Vector2, Cell> m_StreetCells = new Dictionary<Vector2, Cell>();
        public GameObject m_GridObj;
        private Street m_collisionStreet;

        public bool m_StartIsConnectable
        {
            get
            {
                if ((m_StartConnection.m_OtherConnection == null || m_StartConnection.m_OtherComponent.ID <= 0))
                    return true;
                return false;
            }
        }
        public Connection m_EndConnection;
        public bool m_EndIsConnectable
        {
            get
            {
                if (m_EndConnection.m_OtherConnection == null || m_EndConnection.m_OtherComponent.ID <= 0)
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
                    StreetComponentManager.SetStreetColor(this, Color.green);
                }
                else
                {
                    StreetComponentManager.SetStreetColor(this, Color.red);
                }
            }
        }

        private bool lastDrawMeshSetting;
        private int lastSegmentCount;

        private List<Vector3> m_segmentsCorner = new List<Vector3>();

        public Street Init(Vector3[] _splinePos, Connection _startConnection, Connection _endConnection, int _segmentAmount = 10, bool _needID = true, bool _updateSpline = false)
        {
            ID = 0;
            if (_needID)
                Debug.Log("");
            base.Init(_needID);
            m_Spline = new Spline(_splinePos[0], _splinePos[1], _splinePos[2], _splinePos[3], _segmentAmount, this);
            m_MeshFilter = GetComponent<MeshFilter>();
            if (m_MeshFilter == null) Debug.LogError("No MeshFilter found in: " + ID);
            m_MeshRenderer = GetComponent<MeshRenderer>();
            if (m_MeshRenderer == null) Debug.LogError("No MeshRenderer found in: " + ID);
            m_Shape = new StreetShape();
            m_Spline.UpdateOPs();
            MeshGenerator.Extrude(this);
            updateSpline = _updateSpline;
            m_StartConnection = new Connection(null, this, true);
            m_EndConnection = new Connection(null, this, false);
            if (_needID)
            {

                if (_startConnection.m_OtherConnection != null) //if the preview Street had a connection to something
                    Connection.Combine(m_StartConnection, _startConnection.m_OtherConnection); //combine the new Street with the preview othe connection
                else
                    StreetComponentManager.CreateDeadEnd(this, true);

                if (_endConnection.m_OtherConnection != null)
                    Connection.Combine(m_EndConnection, _endConnection.m_OtherConnection);
                else
                    StreetComponentManager.CreateDeadEnd(this, false);
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
                Street s = StreetComponentManager.GetStreetByID(i) as Street;
                GridManager.RemoveGrid(s);
                MeshGenerator.CreateGridMesh(s.m_StreetCells.Values.ToList(), s.m_GridObj.GetComponent<MeshFilter>());
            }
        }

        public void ClearSegmentsCorner()
        {
            m_segmentsCorner.Clear();
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
                    m_MeshFilter.mesh.Clear();
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