using Gameplay.StreetComponents;
using MeshGeneration;
using MyCustomCollsion;
using Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Grid
{
    public enum EAssignment
    {
        NONE,
        LIVING,
        BUSINESS,
        INDUSTRY
    }

    public class Cell
    {
        public Vector3 m_WorldPosCenter { get; private set; }

        public Vector2 m_PosCenter { get; private set; }

        public Quaternion m_Orientation { get; private set; }

        public bool m_isLeft { get; private set; }

        public EAssignment m_CellAssignment { get; private set; }

        public Street m_Street { get; private set; }

        public Spline m_Spline { get; private set; }

        public int ID { get; private set; }

        public Vector3[] m_WorldCorner { get; private set; }

        public Vector2[] m_Corner { get; private set; }

        //  2 -- 3
        //  |    |
        //  0 -- 1
        //  ->->-> SplineDirection

        [ReadOnly]
        public float CellSquareSize;

        private int m_generation;
        private float m_TStart;
        private float m_TEnd;
        public float m_Radius;
        public bool IsValid;
        public Vector2Int Pos;
        public bool IsBlocked; //if Builing is build on it -> true
        public bool IsInArea; //is this Cell assigned to a Area?

        public bool Init(Street _street, float _tStart, float _tEnd, int _generation, bool _isLeftSide, Vector2Int _pos)
        {
            m_CellAssignment = 0;
            m_Street = _street;
            m_Spline = _street.m_Spline;
            m_TStart = _tStart;
            m_TEnd = _tEnd;
            m_generation = _generation;
            m_isLeft = _isLeftSide;
            ID = m_Street.ID;
            if (!_isLeftSide)
                _pos.x = -_pos.x;
            Pos = _pos;
            CalculateCornerPos(_isLeftSide);
            CalculateCellCenter();
            CalculateOrientation();
            CalculateRadius();
            IsValid = !CheckForCollision() && CheckValidSize();
            return IsValid;
        }

        private void CalculateRadius()
        {
            float rSquar = 0;

            for (int i = 0; i < 4; i++)
            {
                float tmp = Vector3.Distance(m_WorldCorner[i], m_WorldPosCenter);
                if (tmp > rSquar)
                    rSquar = tmp;
            }

            m_Radius = rSquar;
        }

        private void CalculateOrientation()
        {
            if (m_isLeft)
                m_Orientation = Quaternion.LookRotation(m_Spline.GetNormalAt((m_TStart + m_TEnd) * 0.5f), m_Spline.GetNormalUpAt((m_TStart + m_TEnd) * 0.5f)) * Quaternion.Euler(0, 180, 0);
            else
                m_Orientation = Quaternion.LookRotation(m_Spline.GetNormalAt((m_TStart + m_TEnd) * 0.5f), m_Spline.GetNormalUpAt((m_TStart + m_TEnd) * 0.5f));
        }

        /// <summary>
        /// Check für Collider with a OverlapSphere
        /// </summary>
        /// <returns>return true if another Cell from a diffrent Street ID collid</returns>
        public bool CheckForCollision()
        {
            List<Cell> cellToCheck = new List<Cell>();
            List<StreetSegment> segToCheck = new List<StreetSegment>();
            List<Cross> crossToCheck = new List<Cross>();

            //Sphere Sphere
            //Cell Cell
            foreach (Cell c in GridManager.m_AllCells)
                if (c.ID != this.ID && MyCollision.SphereSphere(this.m_PosCenter, this.m_Radius, c.m_PosCenter, c.m_Radius))
                    cellToCheck.Add(c);

            //Cell Segment
            foreach (StreetComponent comp in StreetComponentManager.GetAllStreetComponents())
            {
                if (comp.ID == this.ID) continue;
                if (comp is Street)
                {
                    Street s = (Street)comp;
                    foreach (StreetSegment seg in s.m_Segments)
                        if (MyCollision.SphereSphere(this.m_PosCenter, this.m_Radius, seg.m_Center, seg.m_CollisionRadius))
                            segToCheck.Add(seg);
                }
                else if (comp is Cross)
                {
                    Cross c = (Cross)comp;
                    if (MyCollision.SphereSphere(this.m_PosCenter, this.m_Radius, c.m_center, 1.7f)) ;
                    crossToCheck.Add(c);
                }
            }

            //Poly Poly
            //Cell Cross
            foreach (Cross c in crossToCheck)
                if (MyCollision.PolyPoly(this.m_Corner, c.m_corners))
                    return true;

            //Cell Segment
            foreach (StreetSegment seg in segToCheck)
                if (MyCollision.PolyPoly(this.m_Corner, seg.m_CornerPos))
                    return true;

            //Cell Cell
            foreach (Cell c in cellToCheck)
                if (MyCollision.PolyPoly(this.m_Corner, c.m_Corner))
                    return true;

            return false;
        }

        private void CalculateCellCenter()
        {
            Vector3 AD = m_WorldCorner[3] - m_WorldCorner[0];
            Vector3 BC = m_WorldCorner[2] - m_WorldCorner[1];
            Vector3 AB = m_WorldCorner[1] - m_WorldCorner[0];

            Vector3 crossAD_BC = Vector3.Cross(AD, BC);
            Vector3 crossAB_BC = Vector3.Cross(AB, BC);

            float dot = Vector3.Dot(AB, crossAD_BC);

            if (Mathf.Approximately(dot, 0f) && !Mathf.Approximately(crossAD_BC.sqrMagnitude, 0f))
            {
                float tmp = Vector3.Dot(crossAB_BC, crossAD_BC) / crossAD_BC.sqrMagnitude;
                m_WorldPosCenter = m_WorldCorner[0] + (AD * tmp);
                m_PosCenter = new Vector2(m_WorldPosCenter.x, m_WorldPosCenter.z);
                return;
            }
            m_WorldPosCenter = Vector3.zero;
            Debug.LogError("Not Valid Cell Center");
        }

        public bool CheckValidSize()
        {
            // 0 -> 2
            Vector3 AC = m_WorldCorner[2] - m_WorldCorner[0];
            // 0 -> 1
            Vector3 AB = m_WorldCorner[1] - m_WorldCorner[0];

            Vector3 cross = Vector3.Cross(AC, AB);
            float squareArea = cross.sqrMagnitude;

            CellSquareSize = squareArea;

            if (squareArea > GridManager.Instance.GridMaxSquareArea || squareArea < GridManager.Instance.GridMinSquareArea)
            {
                if (squareArea > GridManager.Instance.GridMaxSquareArea)
                {
                    //Debug.Log("Fail Size: Too Big | Size: " + squareArea + " Max: " + GridManager.Instance.GridMaxSquareArea + " AC: " + AC.magnitude + " AB: " + AB.magnitude);
                }
                return false;
            }

            return true;
        }

        private void CalculateCornerPos(bool _isLeftSide)
        {
            Spline streetSpline = m_Spline;
            int generationOffset = m_generation;

            float splineOffset = generationOffset * GridManager.Instance.CellSize - (GridManager.Instance.CellSize - 1);
            float splineOffsetAdd = (generationOffset + 1) * GridManager.Instance.CellSize - (GridManager.Instance.CellSize - 1);

            if (!_isLeftSide)
            {
                splineOffset *= -1;
                splineOffsetAdd *= -1;
            }

            m_WorldCorner = new Vector3[4];
            m_WorldCorner[0] = streetSpline.GetNormalAt(m_TStart) * splineOffset + streetSpline.GetPositionAt(m_TStart);
            m_WorldCorner[1] = streetSpline.GetNormalAt(m_TEnd) * splineOffset + streetSpline.GetPositionAt(m_TEnd);
            m_WorldCorner[2] = streetSpline.GetNormalAt(m_TStart) * splineOffsetAdd + streetSpline.GetPositionAt(m_TStart);
            m_WorldCorner[3] = streetSpline.GetNormalAt(m_TEnd) * splineOffsetAdd + streetSpline.GetPositionAt(m_TEnd);

            m_Corner = new Vector2[4];
            for (int i = 0; i < m_Corner.Length; i++)
            {
                m_Corner[i] = new Vector2(m_WorldCorner[i].x, m_WorldCorner[i].z);
            }
        }

        public void SetAssignment(EAssignment _newAssigment)
        {
            switch (m_CellAssignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    GridManager.m_AllLivingCells.Remove(this);
                    break;
                case EAssignment.BUSINESS:
                    GridManager.m_AllBuisnessCells.Remove(this);
                    break;
                case EAssignment.INDUSTRY:
                    GridManager.m_AllIndustryCells.Remove(this);
                    break;
            } //Remove from old assignment

            m_CellAssignment = _newAssigment;

            switch (_newAssigment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    GridManager.m_AllLivingCells.Add(this);
                    break;
                case EAssignment.BUSINESS:
                    GridManager.m_AllBuisnessCells.Add(this);
                    break;
                case EAssignment.INDUSTRY:
                    GridManager.m_AllIndustryCells.Add(this);
                    break;
            } //Add to new assignment
        }

        public void Delete()
        {
            if (m_Street.m_StreetCells.ContainsKey(Pos))
            {
                Vector2Int nextPos = Pos;
                while (m_Street.m_StreetCells.ContainsKey(nextPos))
                {
                    GridManager.m_AllCells.Remove(m_Street.m_StreetCells[nextPos]);
                    m_Street.m_StreetCells.Remove(nextPos);

                    if (m_isLeft)
                    {
                        nextPos.x++;
                    }
                    else
                    {
                        nextPos.x--;
                    }
                }
            }
            else
            {
                Debug.LogError("Cant Delete Cell At: " + Pos);
            }
        }
    }
}

