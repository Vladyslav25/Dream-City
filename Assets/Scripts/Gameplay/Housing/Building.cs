using Gameplay.StreetComponents;
using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Building
{
    public class Building : MonoBehaviour
    {
        [Header("Settings")]
        public EAssignment m_Assigment;
        public EDemand m_Density;
        [SerializeField]
        private int width;

        [SerializeField]
        private int depth;
        public bool InverseRotation;

        [Header("Impacts")]
        [SerializeField]
        [Tooltip("The Amount of Impact of the Currend Demand")]
        private int inflow; //Living: Residents, Business: Jobs, Industry: Jobs
        [Space]
        [Tooltip("The Amount of Impact of the Needed Demand")]
        [SerializeField]
        private int impactOnLiving;
        [Tooltip("The Amount of Impact of the Needed Demand")]
        [SerializeField]
        private int impactOnBusiness;
        [Tooltip("The Amount of Impact of the Needed Demand")]
        [SerializeField]
        private int impactOnIndustry;

        public Area m_Area;
        private GameObject m_ColliderPlane;

        public int Inflow { get { return inflow; } }

        public float[] Impacts
        {
            get
            {
                return new float[] { impactOnLiving, impactOnBusiness, impactOnIndustry };
            }
        }
        public Vector2Int Size { get { return new Vector2Int(depth, width); } }

        public GameObject GetColliderPlane()
        {
            if (m_ColliderPlane != null)
                return m_ColliderPlane;
            else
            {
                foreach (Transform child in transform)
                {
                    if (child.gameObject.layer == 8)
                    {
                        m_ColliderPlane = child.gameObject;
                    }
                }
                if (m_ColliderPlane == null) Debug.LogError("Building Obj: " + gameObject + " couldnt find a Collider Plane");
                return m_ColliderPlane;
            }
        }

        public void Destroy()
        {
            m_Area.Destroy();

            switch (m_Assigment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    HousingManager.Instance.m_Living_CurrAmount -= Inflow;
                    break;
                case EAssignment.BUSINESS:
                    HousingManager.Instance.m_Business_CurrAmount -= Inflow;
                    break;
                case EAssignment.INDUSTRY:
                    HousingManager.Instance.m_Industry_CurrAmount -= Inflow;
                    break;
                default:
                    break;
            }

            HousingManager.Instance.m_Living_NeedAmount -= Impacts[0];
            HousingManager.Instance.m_Business_NeedAmount -= Impacts[1];
            HousingManager.Instance.m_Industry_NeedAmount -= Impacts[2];

            Destroy(gameObject); //byby House
        }

        public void OnDrawGizmosSelected()
        {
            if (m_Area == null) return;

            switch (m_Area.m_Assignment)
            {
                case EAssignment.NONE:
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

            foreach (Cell c in m_Area.m_Cells)
            {
                Gizmos.DrawWireSphere(c.m_WorldPosCenter, c.m_Radius * 0.8f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(m_Area.m_OP.Position, m_Area.m_OP.Position + Vector3.up * 3f);
            Gizmos.DrawLine(m_Area.m_OP.Position + Vector3.up * 3f, m_Area.m_OP.Rotation * (Vector3.forward * 2f) + m_Area.m_OP.Position + Vector3.up * 3f);
        }
    }

    public class Area
    {
        public Area(Vector2Int _size, List<Cell> _cells, Street _s, OrientedPoint _center)
        {
            m_Size = _size;
            m_Cells = _cells;
            m_Street = _s;
            m_OP = _center;
            m_Assignment = _cells[0].m_CellAssignment;
        }

        public Vector2Int m_Size;
        public List<Cell> m_Cells;
        public Street m_Street;
        public OrientedPoint m_OP;
        public EAssignment m_Assignment;
        public Vector3[] m_Corners = new Vector3[4];
        public Building m_Building;
        public float m_Radius;

        public void Init(Building _b)
        {
            m_Building = _b;
            m_Building.m_Area = this;
            SetCorners(m_Building.GetColliderPlane().GetComponent<MeshFilter>().sharedMesh.vertices, m_Building.GetColliderPlane().transform);
            CalculateRadius();
        }

        private void SetCorners(Vector3[] _planeVerticies, Transform _planeObj)
        {
            m_Corners[0] = _planeObj.TransformPoint(_planeVerticies[0]);
            m_Corners[1] = _planeObj.TransformPoint(_planeVerticies[10]);
            m_Corners[2] = _planeObj.TransformPoint(_planeVerticies[110]);
            m_Corners[3] = _planeObj.TransformPoint(_planeVerticies[120]);
        }

        private void CalculateRadius()
        {
            float rSquar = 0;

            for (int i = 0; i < 4; i++)
            {
                float tmp = Vector3.Distance(m_Corners[i], m_OP.Position);
                if (tmp > rSquar)
                    rSquar = tmp;
            }

            m_Radius = rSquar;
        }

        public void Destroy()
        {
            HousingManager.m_AllAreas.Remove(this);
            switch (m_Assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    m_Street.m_LivingAreas.Remove(this);
                    break;
                case EAssignment.BUSINESS:
                    m_Street.m_BusinessAreas.Remove(this);
                    break;
                case EAssignment.INDUSTRY:
                    m_Street.m_IndustryAreas.Remove(this);
                    break;
            }

            foreach (Cell c in m_Cells)
            {
                c.IsBlocked = false;
                c.IsInArea = false;
            }
        }
    }
}
