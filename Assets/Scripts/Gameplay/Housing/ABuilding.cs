using Gameplay.StreetComponents;
using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Building
{
    public abstract class ABuilding : MonoBehaviour
    {
        public EAssignment m_Assigment;

        [SerializeField]
        private int depth;
        [SerializeField]
        private int width;
        [SerializeField]
        private int inflow; //Living: Residents, Business: Jobs, Industry: Jobs

        private Area m_area;

        public int Inflow { get { return inflow; } }
        public Vector2Int Size { get { return new Vector2Int(width, depth); } }

        public bool InverseRotation;
    }

    public struct Area
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
    }
}
