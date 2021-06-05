using Gameplay.Tools;
using Grid;
using MyCustomCollsion;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.Tools
{
    public class CellAssignmentTool : Tool
    {
        public Material m_WohngebietMat;
        public Material m_GewerbegebietMat;
        public Material m_IndustriegebietMat;

        public EAssignment m_CurrendAssignment;

        public override void ToolStart()
        {
            Cursor.SetActiv(true);
            Cursor.SetColor(Color.white);

            SetMaterialAlpha(m_WohngebietMat, 1f);
            SetMaterialAlpha(m_GewerbegebietMat, 1f);
            SetMaterialAlpha(m_IndustriegebietMat, 1f);

            m_CurrendAssignment = EAssignment.LIVING;

            UIManager.Instance.SetActivAssignment();
        }

        public override void ToolEnd()
        {
            SetMaterialAlpha(m_WohngebietMat);
            SetMaterialAlpha(m_GewerbegebietMat);
            SetMaterialAlpha(m_IndustriegebietMat);
            Cursor.SetActiv(false);
        }

        public override void ToolUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                foreach (Cell c in GridManager.m_FirstGenCells)
                {
                    if (MyCollision.SphereSphere(new Vector2(m_hitPos.x, m_hitPos.z), 0.8f, c.m_PosCenter, c.m_Radius))
                    {
                        int materialIndex = c.Pos.y;
                        if (c.Pos.x < 0)
                            materialIndex += c.m_Street.m_RowAmount;

                        ChanageMaterial(m_CurrendAssignment, materialIndex, c.m_Street.m_GridRenderer);
                        c.m_Street.ChangeCellAssigtment(c.Pos, m_CurrendAssignment);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                m_CurrendAssignment = EAssignment.LIVING;
                UIManager.Instance.HighlightButton(UIManager.Instance.LivingButton);
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                m_CurrendAssignment = EAssignment.BUSINESS;
                UIManager.Instance.HighlightButton(UIManager.Instance.BusinessButton);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                m_CurrendAssignment = EAssignment.INDUSTRY;
                UIManager.Instance.HighlightButton(UIManager.Instance.IndustryButton);
            }
        }

        public void ChanageMaterial(EAssignment _assignment, int _index, MeshRenderer _mr)
        {
            Material mat = null;
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    mat = m_WohngebietMat;
                    break;
                case EAssignment.BUSINESS:
                    mat = m_GewerbegebietMat;
                    break;
                case EAssignment.INDUSTRY:
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
