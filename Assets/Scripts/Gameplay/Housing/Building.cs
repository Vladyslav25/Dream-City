using Gameplay.StreetComponents;
using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Building
{
    public class Building : MonoBehaviour
    {
        [Header("Settings")]
        public EAssignment m_Assigment;
        public EDensity m_Density;
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

        public int Inflow { get { return inflow; } }

        public float[] Impacts
        {
            get
            {
                return new float[] { impactOnLiving * 0.001f, impactOnBusiness * 0.001f, impactOnIndustry * 0.001f };
            }
        }
        public Vector2Int Size { get { return new Vector2Int(depth, width); } }

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
        public Area() { }
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
    }
}
