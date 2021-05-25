using Gameplay.Tools;
using Grid;
using MyCustomCollsion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Tools
{
    public class CellAssignmentTool : Tool
    {
        public Material m_WohngebietMat;
        public Material m_GewerbegebietMat;
        public Material m_IndustriegebietMat;

        private CellAssignment currendAssignment;


        public override void ToolStart()
        {
            Cursor.SetActiv(true);
            Cursor.SetColor(Color.white);

            SetMaterialAlpha(m_WohngebietMat, 1f);
            SetMaterialAlpha(m_GewerbegebietMat, 1f);
            SetMaterialAlpha(m_IndustriegebietMat, 1f);

            currendAssignment = CellAssignment.LIVING;
        }

        public override void ToolEnd()
        {
            SetMaterialAlpha(m_WohngebietMat);
            SetMaterialAlpha(m_GewerbegebietMat);
            SetMaterialAlpha(m_IndustriegebietMat);
        }

        public override void ToolUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                foreach (Cell c in GridManager.m_FirstGenCells)
                {
                    if (MyCollision.SphereSphere(new Vector2(m_hitPos.x, m_hitPos.z), 0.8f, c.m_PosCenter, c.m_Radius))
                    {
                        int materialIndex = c.pos.y;
                        if (c.pos.x < 0)
                            materialIndex += c.m_Street.m_RowAmount + 1;

                        ChanageMaterial(currendAssignment, materialIndex, c.m_Street.m_GridRenderer);
                        c.m_Street.ChangeCellAssigtment(c.pos, currendAssignment);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
                currendAssignment = CellAssignment.LIVING;
            if (Input.GetKeyDown(KeyCode.G))
                currendAssignment = CellAssignment.BUSINESS;
            if (Input.GetKeyDown(KeyCode.I))
                currendAssignment = CellAssignment.INDUSTRY;
        }

        public void ChanageMaterial(CellAssignment _assignment, int _index, MeshRenderer _mr)
        {
            Material mat = null;
            switch (_assignment)
            {
                case CellAssignment.NONE:
                    break;
                case CellAssignment.LIVING:
                    mat = m_WohngebietMat;
                    break;
                case CellAssignment.BUSINESS:
                    mat = m_GewerbegebietMat;
                    break;
                case CellAssignment.INDUSTRY:
                    mat = m_IndustriegebietMat;
                    break;
                default:
                    break;
            }

            Material[] mats = _mr.sharedMaterials;
            mats[_index] = mat;
            _mr.materials = mats;
        }

        private void SetMaterialAlpha(Material _mat, float _a = 0.4f)
        {
            Color c = _mat.color;
            c.a = _a;
            _mat.color = c;
        }
    }
}
