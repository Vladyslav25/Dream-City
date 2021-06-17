using Gameplay.StreetComponents;
using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Buildings
{
    public class Building : ABuilding
    {
        [Header("Settings")]
        public EDemand m_Density;

        public int Inflow { get { return inflow; } }

        public int[] Impacts
        {
            get
            {
                return new int[] { impactOnLiving, impactOnBusiness, impactOnIndustry };
            }
        }

        public override void Destroy()
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
    }

    public class Area
    {
        public Area(Vector2Int _size, List<Cell> _cells, Street _s, OrientedPoint _center)
        {
            m_Size = _size;
            m_Cells = _cells;
            m_Street = _s;
            m_OP = _center;
        }

        public Vector2Int m_Size;
        public List<Cell> m_Cells;
        public Street m_Street;
        public OrientedPoint m_OP;
        public EAssignment m_Assignment
        {
            get
            {
                if (m_Building != null)
                    return m_Building.m_Assigment;
                else
                    return m_Cells[0].m_CellAssignment;
            }
        }
        public Vector3[] m_Corners = new Vector3[4];
        public ABuilding m_Building;
        public float m_Radius;

        public void Init(ABuilding _b)
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
