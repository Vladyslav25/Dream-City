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
        private int depth;
        [SerializeField]
        private int width;
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

        private Area m_area;

        public int Inflow { get { return inflow; } }

        public float[] Impacts
        {
            get
            {
                return new float[] { impactOnLiving * 0.001f, impactOnBusiness * 0.001f, impactOnIndustry * 0.001f };
            }
        }
        public Vector2Int Size { get { return new Vector2Int(depth, width); } }

    }

    public struct Area
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
    }
}
