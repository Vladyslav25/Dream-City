using MeshGeneration;
using Splines;
using Streets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Grid
{
    public enum CellAssignment
    {
        NONE,
        LIVING,
        BUSINESS,
        INDUSTRY
    }

    public class Cell : MonoBehaviour
    {
        public Vector3 m_WorldPosCenter { get; private set; }

        public Quaternion m_Orientation { get; private set; }

        public CellAssignment m_CellAssignment { get; private set; }

        public Street m_Street { get; private set; }

        public Vector3[] m_Corner { get; private set; }

        //  2 -- 3
        //  |    |
        //  0 -- 1
        //  ->->-> SplineDirection

        public Mesh m_Mesh;
        public Cell m_NextCell;     //vlt nicht benötig
        public Cell m_PreviousCell; //vlt nicht benötig
        [ReadOnly]
        public float CellSquareSize;

        private int m_generation;
        private float m_TStart;
        private float m_TEnd;

        public bool Init(Street _street, float _tStart, float _tEnd, int _generation, bool _isLeftSide)
        {
            m_CellAssignment = 0;
            m_Street = _street;
            m_TStart = _tStart;
            m_TEnd = _tEnd;
            m_generation = _generation;
            CalculateCornerPos(_isLeftSide);
            CalculateCellCenter();
            CalculateOrientation();
            return CheckValidSize();
        }

        private void CalculateOrientation()
        {
            m_Orientation = Quaternion.LookRotation(m_Street.m_Spline.GetNormalAt((m_TStart + m_TEnd) * 0.5f), m_Street.m_Spline.GetNormalUpAt((m_TStart + m_TEnd) * 0.5f));
        }

        public void CheckForCollision()
        {
            Collider[] coll = Physics.OverlapBox(
                m_WorldPosCenter,
                (m_WorldPosCenter - m_Corner[0]) * 0.5f,
                m_Orientation,
                0);

            Debug.Log(coll.Length);
        }

        private void CalculateCellCenter()
        {
            Vector3 AD = m_Corner[3] - m_Corner[0];
            Vector3 BC = m_Corner[2] - m_Corner[1];
            Vector3 AB = m_Corner[1] - m_Corner[0];

            Vector3 crossAD_BC = Vector3.Cross(AD, BC);
            Vector3 crossAB_BC = Vector3.Cross(AB, BC);

            float dot = Vector3.Dot(AB, crossAD_BC);

            if (Mathf.Approximately(dot, 0f) && !Mathf.Approximately(crossAD_BC.sqrMagnitude, 0f))
            {
                float tmp = Vector3.Dot(crossAB_BC, crossAD_BC) / crossAD_BC.sqrMagnitude;
                m_WorldPosCenter = m_Corner[0] + (AD * tmp);
                return;
            }
            m_WorldPosCenter = Vector3.zero;
            Debug.LogError("Not Valid Cell Center");
        }

        public bool CheckValidSize()
        {
            // 0 -> 2
            Vector3 AC = m_Corner[2] - m_Corner[0];
            // 0 -> 1
            Vector3 AB = m_Corner[1] - m_Corner[0];

            Vector3 cross = Vector3.Cross(AC, AB);
            float squareArea = cross.sqrMagnitude;

            CellSquareSize = squareArea;

            if (squareArea > GridManager.Instance.GridMaxSquareArea || squareArea < GridManager.Instance.GridMinSquareArea)
                return false;

            return true;
        }

        private void CalculateCornerPos(bool _isLeftSide)
        {
            Spline streetSpline = m_Street.m_Spline;
            int generationOffset = m_generation;

            float splineOffset = generationOffset * GridManager.Instance.CellSize - (GridManager.Instance.CellSize - 1);
            float splineOffsetAdd = (generationOffset + 1) * GridManager.Instance.CellSize - (GridManager.Instance.CellSize - 1);

            if (!_isLeftSide)
            {
                splineOffset *= -1;
                splineOffsetAdd *= -1;
            }

            m_Corner = new Vector3[4];
            m_Corner[0] = streetSpline.GetNormalAt(m_TStart) * splineOffset + streetSpline.GetPositionAt(m_TStart);
            m_Corner[1] = streetSpline.GetNormalAt(m_TEnd) * splineOffset + streetSpline.GetPositionAt(m_TEnd);
            m_Corner[2] = streetSpline.GetNormalAt(m_TStart) * splineOffsetAdd + streetSpline.GetPositionAt(m_TStart);
            m_Corner[3] = streetSpline.GetNormalAt(m_TEnd) * splineOffsetAdd + streetSpline.GetPositionAt(m_TEnd);
        }

        public void SetAssignment(CellAssignment _newAssigment)
        {
            m_CellAssignment = _newAssigment;
        }
    }
}
