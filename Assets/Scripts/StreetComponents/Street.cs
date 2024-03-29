using Gameplay.Buildings;
using Grid;
using MeshGeneration;
using MyCustomCollsion;
using Splines;
using System.Collections;
using System.Collections.Generic;
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
        private bool drawGridNormalsUp = false;
        [SerializeField]
        private bool drawGridNormals = false;


        //Debug Settings
        private bool lastDrawMeshSetting;
        private int lastSegmentCount;
        #endregion

        [HideInInspector]
        public Dictionary<Vector2Int, Cell> m_StreetCells = new Dictionary<Vector2Int, Cell>(); //Dictionaray of the Cells as Value and there Position in the Grid as Key

        public GameObject m_GridObj; //The Grid Parent Gameobject

        public MeshRenderer m_GridRenderer;
        public MeshRenderer m_Coll_GridRenderer;

        private Street m_collisionStreet; //Ref to the Collision Street GameObject

        public int m_RowAmount;

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

        public List<Area> m_LivingAreas = new List<Area>();
        public List<Area> m_BusinessAreas = new List<Area>();
        public List<Area> m_IndustryAreas = new List<Area>();
        public List<Area> m_ProductionAreas = new List<Area>();

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
                if (_startConnection?.m_OtherConnection != null) //if the preview Street had a connection to something
                    Connection.Combine(GetStartConnection(), _startConnection.m_OtherConnection); //combine the new Street with the preview other connection
                else
                    StreetComponentManager.CreateDeadEnd(this, true); //If there is no Connections to Combine, Create a DeadEnd

                if (_endConnection?.m_OtherConnection != null)
                    Connection.Combine(m_EndConnection, _endConnection.m_OtherConnection);
                else
                    StreetComponentManager.CreateDeadEnd(this, false);
            }

            CreateSegments();

            if (_needID) //if its isnt a Collision Street
                StartCoroutine(PlaceBuildings());

            return this;
        }

        private IEnumerator PlaceBuildings()
        {
            GameObject obj;
            bool lastLeft = false;
            EAssignment lastAssignemnt = EAssignment.NONE;
            while (true)
            {
                if (lastAssignemnt != EAssignment.PRODUCTION && BuildingManager.Instance.m_ProductionBuildWaitingList.Count > 0)
                {
                    if (!lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceProductionBuilding(
                            BuildingManager.Instance.m_ProductionBuildWaitingList[0],
                            this, true);
                        lastLeft = true;
                        if (obj != null)
                        {
                            yield return new WaitForSeconds(BuildingManager.Instance.IndustryWaitTime);
                            continue;
                        }
                    }
                    if (lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceProductionBuilding(
                            BuildingManager.Instance.m_ProductionBuildWaitingList[0],
                            this, false);
                        lastLeft = false;
                        if (obj != null)
                        {
                            yield return new WaitForSeconds(BuildingManager.Instance.LivingWaitTime);
                            continue;
                        }
                    }
                }

                if (lastAssignemnt != EAssignment.LIVING)
                {
                    //LIVING
                    if (!lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceBuilding(EAssignment.LIVING, this, true);
                        lastLeft = true;
                        if (obj != null)
                        {
                            lastLeft = true;
                            yield return new WaitForSeconds(BuildingManager.Instance.BusinessWaitTime);
                            continue;
                        }
                    }
                    if (lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceBuilding(EAssignment.LIVING, this, false);
                        lastLeft = false;
                        if (obj != null)
                        {
                            lastLeft = false;
                            yield return new WaitForSeconds(BuildingManager.Instance.IndustryWaitTime);
                            continue;
                        }
                    }
                }

                if (lastAssignemnt != EAssignment.BUSINESS)
                {

                    //BUSINESS
                    if (!lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceBuilding(EAssignment.BUSINESS, this, true);
                        lastLeft = true;
                        if (obj != null)
                        {
                            lastLeft = true;
                            yield return new WaitForSeconds(1f);
                            continue;
                        }
                    }
                    if (lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceBuilding(EAssignment.BUSINESS, this, false);
                        lastLeft = false;
                        if (obj != null)
                        {
                            yield return new WaitForSeconds(1f);
                            continue;
                        }
                    }
                }

                if (lastAssignemnt != EAssignment.INDUSTRY)
                {

                    //INDUSTRY
                    if (!lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceBuilding(EAssignment.INDUSTRY, this, true);
                        lastLeft = true;
                        if (obj != null)
                        {
                            yield return new WaitForSeconds(1f);
                            continue;
                        }
                    }
                    if (lastLeft)
                    {
                        obj = BuildingManager.Instance.PlaceBuilding(EAssignment.INDUSTRY, this, false);
                        lastLeft = false;
                        if (obj != null)
                        {
                            yield return new WaitForSeconds(1f);
                            continue;
                        }
                    }
                }

                lastAssignemnt = EAssignment.NONE;
                yield return new WaitForEndOfFrame();
            }
        }

        #region AreaHandling/-Creation
        /// <summary>
        /// Find the next biggest Area on the left Side of the Road
        /// </summary>
        /// <param name="a">The new Area</param>
        /// <returns>Can there be a new Area</returns>
        public bool FindAreaLeftSide(out Area a)
        {
            Vector2Int p = new Vector2Int(1, 0); //Pointer
            int height = 0;
            int width = 0;
            a = null;

            if (m_StreetCells.Count == 0)
            {
                return false;
            }

            for (int y = 0; y < m_RowAmount; y++) //Find StartPoint
            {
                if (m_StreetCells.ContainsKey(p) && !m_StreetCells[p].IsInArea && m_StreetCells[p].m_CellAssignment != EAssignment.NONE)
                    break;
                else
                    p.y++;

            }
            if (!m_StreetCells.ContainsKey(p)) return false;
            EAssignment assi = m_StreetCells[p].m_CellAssignment;
            if (assi == EAssignment.NONE) return false;
            Vector2Int pStart = p;

            while (m_StreetCells.ContainsKey(p) && !m_StreetCells[p].IsBlocked && m_StreetCells[p].m_CellAssignment == assi && !m_StreetCells[p].IsInArea) //Go up till up end
            {
                p.x++;
                height++;
            }
            if (height == 0) return false;
            p.x--;
            while (m_StreetCells.ContainsKey(p) && !m_StreetCells[p].IsBlocked && m_StreetCells[p].m_CellAssignment == assi && !m_StreetCells[p].IsInArea) //Go right till end
            {
                p.y++;
                width++;
                if (m_StreetCells.ContainsKey(new Vector2Int(p.x + 1, p.y)))
                {
                    break; // if up exist break because fin
                }
            }
            if (width == 0) return false;

            List<Cell> areaCells = SetCellsInArea(pStart, new Vector2Int(height, width), true);
            if (areaCells == null) return false;
            Debug.Log("Size: X: " + height + " Y: " + width + " Assigmnet: " + assi);
            a = new Area(new Vector2Int(width, height), areaCells, this, null);
            return true;
        }

        /// <summary>
        /// Find the next biggest Area on the right Side of the Road
        /// </summary>
        /// <param name="a">The new Area</param>
        /// <returns>Can there be a new Area</returns>
        public bool FindAreaRightSide(out Area a)
        {
            Vector2Int p = new Vector2Int(-1, 0); //Pointer
            int height = 0;
            int width = 0;
            a = null;

            if (m_StreetCells.Count == 0)
            {
                return false;
            }

            for (int y = 0; y < m_RowAmount; y++) //Find valid StartPoint
            {
                if (m_StreetCells.ContainsKey(p) && !m_StreetCells[p].IsInArea && m_StreetCells[p].m_CellAssignment != EAssignment.NONE)
                    break;
                else
                    p.y++;

            }
            if (!m_StreetCells.ContainsKey(p)) return false;
            EAssignment assi = m_StreetCells[p].m_CellAssignment;
            if (assi == EAssignment.NONE) return false;
            Vector2Int pStart = p;

            while (m_StreetCells.ContainsKey(p) && !m_StreetCells[p].IsBlocked && m_StreetCells[p].m_CellAssignment == assi && !m_StreetCells[p].IsInArea) //Go up till up end
            {
                p.x--;
                height++;
            }
            if (height == 0) return false;
            p.x++;
            while (m_StreetCells.ContainsKey(p) && !m_StreetCells[p].IsBlocked && m_StreetCells[p].m_CellAssignment == assi && !m_StreetCells[p].IsInArea) //Go right till end
            {
                p.y++;
                width++;
                if (m_StreetCells.ContainsKey(new Vector2Int(p.x - 1, p.y)))
                {
                    break; // if up exist break because fin
                }
            }
            if (width == 0) return false;

            List<Cell> areaCells = SetCellsInArea(pStart, new Vector2Int(height, width), false);
            if (areaCells == null) return false;
            Debug.Log("Size: X: " + height + " Y: " + width + " Assigmnet: " + assi);
            a = new Area(new Vector2Int(width, height), areaCells, this, null);
            return true;
        }

        /// <summary>
        /// Find a Area with the given settings on the left side of the Road
        /// </summary>
        /// <param name="_size">The size of the Area</param>
        /// <param name="_assignment">The Assignment of the Cells</param>
        /// <param name="a">The new Area</param>
        /// <returns>Can there be such an Area</returns>
        public bool FindAreaLeftSide(Vector2Int _size, EAssignment _assignment, out Area a)
        {
            //Look Left Side
            Vector2Int p = new Vector2Int(1, 0); //Pointer
            a = null;

            if (m_StreetCells.Count == 0)
            {
                return false;
            }

            Vector2Int pStart = p;

            //move the StartPointer along the Grid to the Start if the grid
            for (int i = 0; i < m_RowAmount; i++)
            {
                if (m_StreetCells.ContainsKey(pStart) && m_StreetCells[pStart].m_CellAssignment == _assignment)
                    break;
                else
                    pStart.y++;
            }

            while (m_StreetCells.ContainsKey(pStart))
            {
                //move right as long as this generation dont have the needed depth
                while ((m_StreetCells.ContainsKey(pStart) && m_StreetCells[pStart].IsInArea) || !m_StreetCells.ContainsKey(new Vector2Int(pStart.x + _size.x - 1, pStart.y)))
                {
                    if (pStart.y > m_RowAmount) return false;
                    pStart.y++; //if this geneartion dont reached this depth, move to the right
                }

                p.x = _size.x; //move p to the needed depth
                p.y = pStart.y;

                //move right as long as the size.y
                if (MoveCellPointerRight(_size.y, ref pStart, ref p, _assignment, true)) //if to the right all cells are valid + reset pStart if invalid
                {
                    List<Cell> cells = SetCellsInArea(pStart, _size, true);
                    if (cells == null)
                    {
                        pStart = new Vector2Int(1, p.y + 1); //set pStart to right generation and move it back down
                        p = pStart;
                        continue;
                    }
                    a = new Area(_size, cells, this, GetOrientenPointFromCells(m_StreetCells[pStart], m_StreetCells[p]));
                    switch (_assignment)
                    {
                        case EAssignment.NONE:
                            break;
                        case EAssignment.LIVING:
                            m_LivingAreas.Add(a);
                            break;
                        case EAssignment.BUSINESS:
                            m_BusinessAreas.Add(a);
                            break;
                        case EAssignment.INDUSTRY:
                            m_IndustryAreas.Add(a);
                            break;
                        case EAssignment.PRODUCTION:
                            m_ProductionAreas.Add(a);
                            break;
                    }
                    return true;
                }
            }
            return false; //if the while ends because the pointer is on an invalid pos, then return false
        }

        /// <summary>
        /// Find a Area with the given settings on the right side of the Road
        /// </summary>
        /// <param name="_size">The size of the Area</param>
        /// <param name="_assignment">The Assignment of the Cells</param>
        /// <param name="a">The new Area</param>
        /// <returns>Can there be such an Area</returns>
        public bool FindAreaRightSide(Vector2Int _size, EAssignment _assignment, out Area a)
        {
            //Look Left Side
            Vector2Int p = new Vector2Int(-1, 0); //Pointer
            a = null;

            if (m_StreetCells.Count == 0)
            {
                return false;
            }

            Vector2Int pStart = p;

            //move the StartPointer along the Grid to the Start if the grid
            for (int i = 0; i < m_RowAmount; i++)
            {
                if (m_StreetCells.ContainsKey(pStart) && m_StreetCells[pStart].m_CellAssignment == _assignment)
                    break;
                else
                    pStart.y++;
            }

            while (m_StreetCells.ContainsKey(pStart))
            {
                //move right as long as this generation dont have the need depht
                while ((m_StreetCells.ContainsKey(pStart) && m_StreetCells[pStart].IsInArea) || !m_StreetCells.ContainsKey(new Vector2Int(pStart.x - _size.x + 1, pStart.y)))
                {
                    if (pStart.y > m_RowAmount) return false;
                    pStart.y++; //if this geneartion dont reached this depth move to the right
                }

                p.x = -_size.x; //move p to the needed depth
                p.y = pStart.y;

                //move right as long as the size.y
                if (MoveCellPointerRight(_size.y, ref pStart, ref p, _assignment, false)) //if to the right all cells are valid
                {
                    List<Cell> cells = SetCellsInArea(pStart, _size, false);
                    if (cells == null)
                    {
                        pStart = new Vector2Int(-1, p.y + 1); //set pStart to right generation and move it back down
                        p = pStart;
                        return false;
                    }
                    a = new Area(_size, cells, this, GetOrientenPointFromCells(m_StreetCells[pStart], m_StreetCells[p]));
                    switch (_assignment)
                    {
                        case EAssignment.NONE:
                            break;
                        case EAssignment.LIVING:
                            m_LivingAreas.Add(a);
                            break;
                        case EAssignment.BUSINESS:
                            m_BusinessAreas.Add(a);
                            break;
                        case EAssignment.INDUSTRY:
                            m_IndustryAreas.Add(a);
                            break;
                        case EAssignment.PRODUCTION:
                            m_ProductionAreas.Add(a);
                            break;
                    }
                    return true;
                }
            }
            return false; //if the while ends because the pointer is on an invalid pos, then return false
        }

        /// <summary>
        /// Moves the pointer a specified amount along the street
        /// </summary>
        /// <param name="_sizeY">The amount by which the pointer should be moved</param>
        /// <param name="pStart">The start pointer, to reset this if the move fails</param>
        /// <param name="p">The Pointer to move</param>
        /// <param name="_assignment">The needed assignment to check</param>
        /// <returns>Can the pointer move by this amount along the street</returns>
        private bool MoveCellPointerRight(int _sizeY, ref Vector2Int pStart, ref Vector2Int p, EAssignment _assignment, bool _isLeft)
        {
            for (int i = 0; i < _sizeY - 1; i++) //move to the right and look if the cell is valid
            {
                p.y++;
                if (!m_StreetCells.ContainsKey(p) || m_StreetCells[p].m_CellAssignment != _assignment || m_StreetCells[p].IsInArea) //if the cell exist and the assigment is valid
                {
                    pStart = new Vector2Int(1, p.y + 1); //set pStart to right generation and move it back down
                    if (!_isLeft)
                        pStart.x *= -1;
                    p = pStart;
                    return false;
                }
            }
            return true;
        }

        private List<Cell> SetCellsInArea(Vector2Int _start, Vector2Int _size, bool _isLeft)
        {
            List<Cell> output = new List<Cell>();
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    Vector2Int v;

                    if (_isLeft)
                        v = _start + new Vector2Int(x, y);
                    else
                        v = _start + new Vector2Int(-x, y);

                    if (m_StreetCells.ContainsKey(v) && !m_StreetCells[v].IsInArea)
                    {
                        output.Add(m_StreetCells[v]);
                        m_StreetCells[v].IsInArea = true;
                    }
                    else
                    {
                        //Reset Cells
                        foreach (Cell c in output)
                        {
                            c.IsInArea = false;
                        }
                        return null;
                    }
                }
            }
            return output;
        }

        private OrientedPoint GetOrientenPointFromCells(Cell cDownLeft, Cell cTopRight)
        {
            Vector3 center = (cTopRight.m_WorldPosCenter - cDownLeft.m_WorldPosCenter) * .5f + cDownLeft.m_WorldPosCenter;
            Quaternion rotation = Quaternion.Lerp(cTopRight.m_Orientation, cDownLeft.m_Orientation, .5f);
            return new OrientedPoint(center, rotation);
        }

        private IEnumerator PlaceBuilding()
        {
            yield return null;
        }
        #endregion

        #region Collision
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
                foreach (int id in segment.CheckCollision(GridManager.m_AllCells))
                {
                    //Saves the IDs of Streets which the segment collide with
                    StreetsToRecreate.Add(id);
                }
            }

            //Recreate the Streets Grid Mesh
            foreach (int id in StreetsToRecreate)
            {
                Street s = StreetComponentManager.GetStreetByID(id);
                GridManager.RemoveGridMesh(s);
                MeshGenerator.CreateGridMesh(s, s.m_GridObj.GetComponent<MeshFilter>(), s.m_GridRenderer);
            }
        }

        public void ClearSegmentsCorner()
        {
            m_segmentsCorner.Clear();
        }
        #endregion

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

            if (Input.GetKeyDown(KeyCode.F) && m_StreetCells.Count > 1)
            {
                BuildingManager.Instance.PlaceBuilding(EAssignment.LIVING, this, true);
                BuildingManager.Instance.PlaceBuilding(EAssignment.LIVING, this, false);
            }
        }

        public void ChangeCellAssigtment(Vector2Int _cellPosStart, EAssignment _assignment)
        {
            Vector2Int pos = _cellPosStart;
            while (m_StreetCells.ContainsKey(pos))
            {
                m_StreetCells[pos].SetAssignment(_assignment);
                pos.x += _cellPosStart.x;       //pos.x = -1 change to pos.x = -2 and pos.x = 1 change to pos.x = 2
            }
        }

        public void SetCellBlocked(Cell c)
        {
            if (c.IsBlocked)
                Debug.LogError("Cell at: " + c.Pos + " is already blocked");
            c.IsBlocked = true;
        }

        private void OnDrawGizmosSelected()
        {
            foreach (Cell c in m_StreetCells.Values)
            {
                if (!c.IsInArea) continue;
                switch (c.m_CellAssignment)
                {
                    case EAssignment.NONE:
                        Gizmos.color = Color.grey;
                        break;
                    case EAssignment.LIVING:
                        Gizmos.color = Color.green;
                        break;
                    case EAssignment.BUSINESS:
                        Gizmos.color = Color.blue;
                        break;
                    case EAssignment.INDUSTRY:
                        Gizmos.color = Color.yellow;
                        break;
                }
                Gizmos.DrawWireSphere(c.m_WorldPosCenter, c.m_Radius);
            }

            foreach (StreetSegment segment in m_Segments)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(segment.m_Center.x, 0, segment.m_Center.y), segment.m_CollisionRadius);
            }

            foreach (Area a in m_LivingAreas)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(a.m_OP.Position, a.m_OP.Position + Vector3.up * 3f);
                Gizmos.DrawLine(a.m_OP.Position, a.m_OP.Rotation * (Vector3.forward * 3f) + a.m_OP.Position);
            }

            foreach (Area a in m_BusinessAreas)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(a.m_OP.Position, a.m_OP.Position + Vector3.up * 3f);
                Gizmos.DrawLine(a.m_OP.Position, a.m_OP.Rotation * (Vector3.forward * 3f) + a.m_OP.Position);
            }

            foreach (Area a in m_IndustryAreas)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(a.m_OP.Position, a.m_OP.Position + Vector3.up * 3f);
                Gizmos.DrawLine(a.m_OP.Position, a.m_OP.Rotation * (Vector3.forward * 3f) + a.m_OP.Position);
            }

            if (drawGridNormalsUp)
            {
                for (int i = 0; i < m_Spline.GridOPs.Length; i++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(m_Spline.GridOPs[i].Position, m_Spline.GridOPs[i].Position + m_Spline.GetNormalUpAt(m_Spline.GridOPs[i].t) * normalLenghtUp);
                }
            }

            if (drawGridNormals)
            {
                for (int i = 0; i < m_Spline.GridOPs.Length; i++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(m_Spline.GridOPs[i].Position, m_Spline.GridOPs[i].Position + m_Spline.GetNormalAt(m_Spline.GridOPs[i].t) * normalLenght);
                    Gizmos.DrawLine(m_Spline.GridOPs[i].Position, m_Spline.GridOPs[i].Position - m_Spline.GetNormalAt(m_Spline.GridOPs[i].t) * normalLenght);
                }
                    Gizmos.DrawLine(m_Spline.GetLastOrientedPoint().Position, m_Spline.GetLastOrientedPoint().Position - m_Spline.GetNormalAt(m_Spline.GetLastOrientedPoint().t) * normalLenght);
                    Gizmos.DrawLine(m_Spline.GetLastOrientedPoint().Position, m_Spline.GetLastOrientedPoint().Position + m_Spline.GetNormalAt(m_Spline.GetLastOrientedPoint().t) * normalLenght);
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
            // 0 -- 2
            // |    |
            // 1 -- 3
            // -> Street Direction ->

            m_Street = _steet;
            m_CornerPos = new Vector2[4];
            for (int i = 0; i < m_CornerPos.Length; i++)
            {
                m_CornerPos[i] = new Vector2(_corner[i].x, _corner[i].z);
            }

            Vector3 worldCenter = (_corner[3] - _corner[0]) * 0.5f + _corner[0];
            m_Center = new Vector2(worldCenter.x, worldCenter.z);

            float c1 = Vector3.Distance(worldCenter, _corner[1]);
            float c2 = Vector3.Distance(worldCenter, _corner[2]);

            if (c1 > c2)
                m_CollisionRadius = c1;
            else
                m_CollisionRadius = c2;

        }

        /// <summary>
        /// Check the Segment Collision and Remove colliding Cells with there later Generations
        /// </summary>
        /// <param name="_cellsToCheck">The List of Cells to Check the Collision with</param>
        /// <returns></returns>
        public HashSet<int> CheckCollision(List<Cell> _cellsToCheck)
        {
            List<Cell> PolyPolyCheckList = new List<Cell>();
            HashSet<int> StreetIDsToRecreate = new HashSet<int>();

            //Sphere Sphere Collsion | fast but not precies | Save the Cells that can possible collide for a more precies check 
            for (int i = 0; i < _cellsToCheck.Count; i++)
            {
                if (!_cellsToCheck[i].IsBlocked
                    && MyCollision.SphereSphere(m_Center, m_CollisionRadius, _cellsToCheck[i].m_PosCenter, _cellsToCheck[i].m_Radius))
                {
                    PolyPolyCheckList.Add(_cellsToCheck[i]);
                }
            }

            //Poly Poly Collision | slow but more precies 
            for (int i = 0; i < PolyPolyCheckList.Count; i++)
            {
                if (!PolyPolyCheckList[i].IsBlocked && MyCollision.PolyPoly(PolyPolyCheckList[i].m_Corner, m_CornerPos))
                {
                    PolyPolyCheckList[i].Delete(); //Delete the Colliding Cell and the later Generations
                    int id = PolyPolyCheckList[i].m_Street.ID;
                    StreetIDsToRecreate.Add(id); //Saves the Street ID to recreate the Grid Mesh
                }
            }

            return StreetIDsToRecreate;
        }
    }
}