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
        #region -Debug Settings-
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

        //Debug Settings
        private bool lastDrawMeshSetting;
        private int lastSegmentCount;
        #endregion

        [HideInInspector]
        public Dictionary<Vector2, Cell> m_StreetCells = new Dictionary<Vector2, Cell>(); //Dictionaray of the Cells as Value and there Position in the Grid as Key

        public GameObject m_GridObj; //The Grid Parent Gameobject

        private Street m_collisionStreet; //Ref to the Collision Street GameObject

        //Check if the Start can be connected to a new Street
        public bool m_StartIsConnectable
        {
            get
            {
                if ((GetStartConnection().m_OtherConnection == null        //If the Start is not connected 
                    || GetStartConnection().m_OtherComponent.ID <= 0))     // OR If the Connected Street Component have an ID below Zero
                    return true;                                        // Default = 0 | DeadEnds ID = -2 | Finished Streets ID > 0 
                return false;
            }
        }

        //The End Connection of the Street
        public Connection m_EndConnection;

        //Check if the End can be connected to a new Street
        public bool m_EndIsConnectable
        {
            get
            {
                if (m_EndConnection.m_OtherConnection == null           //If the End is not connected 
                    || m_EndConnection.m_OtherComponent.ID <= 0)        // OR If the Connected Street Component have an ID below Zero
                    return true;                                        // Default = 0 | DeadEnds ID = -2 | Finished Streets ID > 0 
                return false;
            }
        }

        //List of segment Corners (Street - Cell Collision Test)
        private List<Vector3> m_segmentsCorner = new List<Vector3>();
        public List<StreetSegment> m_Segments = new List<StreetSegment>();

        /// <summary>
        /// Initialize the Street
        /// </summary>
        /// <param name="_splinePos">The 4 Locations for the Spline (Start, Tangent 1, Tangent 2, End)</param>
        /// <param name="_startConnection">The Start Connection of the Street Component</param>
        /// <param name="_endConnection">The End Connection of the Street</param>
        /// <param name="_segmentAmount">The Amount of Segments the Street needs</param>
        /// <param name="_needID">Need the Street an ID?</param>
        /// <param name="_updateSpline">Should the Spline be Updated?</param>
        /// <returns>Returns the new Street</returns>
        public Street Init(Vector3[] _splinePos, Connection _startConnection, Connection _endConnection, int _segmentAmount = 10, bool _needID = true, bool _updateSpline = false)
        {
            ID = 0;
            if (_needID)
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

            //Create Start and End Connections
            SetStartConnection(new Connection(null, this, true));
            m_EndConnection = new Connection(null, this, false);

            //If its an finished Street Combine the Connections from the Preview Street
            if (_needID)
            {

                if (_startConnection.m_OtherConnection != null) //if the preview Street had a connection to something
                    Connection.Combine(GetStartConnection(), _startConnection.m_OtherConnection); //combine the new Street with the preview other connection
                else
                    StreetComponentManager.CreateDeadEnd(this, true); //If there is no Connections to Combine, Create a DeadEnd

                if (_endConnection.m_OtherConnection != null)
                    Connection.Combine(m_EndConnection, _endConnection.m_OtherConnection);
                else
                    StreetComponentManager.CreateDeadEnd(this, false);
            }

            CreateSegments();

            return this;
        }

        private void CreateSegments()
        {
            m_Spline.UpdateOPs(this);
            int segmentsAmount = (m_segmentsCorner.Count - 2) / 2;
            int offset = 0;
            for (int i = 0; i < segmentsAmount; i++)
            {
                Vector3[] segmentCorner = new Vector3[4];
                segmentCorner[0] = m_segmentsCorner[offset];
                segmentCorner[1] = m_segmentsCorner[offset + 1];
                segmentCorner[2] = m_segmentsCorner[offset + 2];
                segmentCorner[3] = m_segmentsCorner[offset + 3];
                offset += 2; //Add 2 to the offset to get the last 2 Points of the last Segment as first two in the new Segment
                StreetSegment newSegment = new StreetSegment(this, segmentCorner);
                m_Segments.Add(newSegment);
            }
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
        /// Check the Street - Cell Collision and Update the Grid of other Streets if needed
        /// </summary>
        public void CheckCollision()
        {
            HashSet<int> StreetsToRecreate = new HashSet<int>(); //Create an HashSet of int (StreetComponent IDs) to save the Streets Grid thats needs to be recreated
            foreach (StreetSegment segment in m_Segments)
            {
                List<int> IDs = segment.CheckCollision(GridManager.m_AllCells); //Saves the IDs of Streets which the segment collide with
                foreach (int i in IDs)
                    StreetsToRecreate.Add(i); //Saves the Id in the HashSet (no duplicates) 
            }

            //Recreate the Streets Grid Mesh
            foreach (int i in StreetsToRecreate)
            {
                Street s = StreetComponentManager.GetStreetByID(i);
                GridManager.RemoveGridMesh(s);
                //MeshGenerator.CreateGridMesh(s.m_StreetCells.Values.ToList(), s.m_GridObj.GetComponent<MeshFilter>());
                MeshGenerator.CreateGrid(s, s.m_GridObj.GetComponent<MeshFilter>());
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
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(m_Spline.Tangent1Pos, 0.5f);
            Gizmos.DrawWireSphere(m_Spline.Tangent2Pos, 0.5f);

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

            //Calculate Collision Radius
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

        /// <summary>
        /// Check the Segment Collision and Remove colliding Cells with there later Generations
        /// </summary>
        /// <param name="_cellsToCheck">The List of Cells to Check the Collision with</param>
        /// <returns></returns>
        public List<int> CheckCollision(List<Cell> _cellsToCheck)
        {
            List<Cell> PolyPolyCheckList = new List<Cell>();
            List<int> StreetIDsToRecreate = new List<int>();

            //Sphere Sphere Collsion | fast but not precies | Save the Cells that can possible collide for a more precies check 
            for (int i = 0; i < _cellsToCheck.Count; i++)
            {
                if (MyCollision.SphereSphere(m_Center, m_CollisionRadius, _cellsToCheck[i].m_PosCenter, _cellsToCheck[i].CellSquareSize))
                {
                    PolyPolyCheckList.Add(GridManager.m_AllCells[i]);
                }
            }

            //Poly Poly Collision | slow but more precies 
            for (int i = 0; i < PolyPolyCheckList.Count; i++)
            {
                if (MyCollision.PolyPoly(PolyPolyCheckList[i].m_Corner, m_CornerPos))
                {
                    PolyPolyCheckList[i].Delete(); //Delete the Colliding Cell and the later Generations
                    int id = PolyPolyCheckList[i].m_Street.ID;
                    if (!StreetIDsToRecreate.Contains(id))
                        StreetIDsToRecreate.Add(id); //Saves the Street ID to recreate the Grid Mesh
                }
            }

            return StreetIDsToRecreate;
        }
    }
}