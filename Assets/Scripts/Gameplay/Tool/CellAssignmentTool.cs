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
    public class CellAssignmentTool : ATool
    {
        [SerializeField]
        private Material m_LivingMat;
        [SerializeField]
        private Material m_BusinessMat;
        [SerializeField]
        private Material m_IndustryMat;
        [SerializeField]
        private Material m_ClearMat;
        [SerializeField]
        private Material m_CollRed;

        [HideInInspector]
        public EAssignment m_CurrendAssignment;

        public override void ToolStart()
        {
            Cursor.SetActiv(true);
            Cursor.SetColor(Color.white);

            SetMaterialAlpha(m_LivingMat, 1f);
            SetMaterialAlpha(m_BusinessMat, 1f);
            SetMaterialAlpha(m_IndustryMat, 1f);

            m_CurrendAssignment = EAssignment.LIVING;
            UIManager.Instance.HighlightButton(UIManager.Instance.LivingButton);

            UIManager.Instance.SetActivAssignment();
        }

        public override void ToolEnd()
        {
            SetMaterialAlpha(m_LivingMat, 0f);
            SetMaterialAlpha(m_BusinessMat, 0f);
            SetMaterialAlpha(m_IndustryMat, 0f);
            UIManager.Instance.SetActivToolChoose();
            Cursor.SetActiv(false);
        }

        public override void ToolUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                foreach (Cell c in GridManager.m_FirstGenCells)
                {
                    if (c.IsBlocked) continue;
                    if (MyCollision.SphereSphere(new Vector2(m_hitPos.x, m_hitPos.z), 0.8f, c.m_PosCenter, c.m_Radius))
                    {
                        int materialIndex = c.Pos.y;
                        if (c.Pos.x < 0)
                            materialIndex += c.m_Street.m_RowAmount;

                        ChanageMaterial(m_CurrendAssignment, materialIndex, c.m_Street.m_GridRenderer, c.m_Street.m_Coll_GridRenderer);
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

        private void ChanageMaterial(EAssignment _assignment, int _index, MeshRenderer _mr, MeshRenderer _collMr)
        {
            Material mat = null;
            switch (_assignment)
            {
                case EAssignment.NONE:
                    mat = m_ClearMat;
                    break;
                case EAssignment.LIVING:
                    mat = m_LivingMat;
                    break;
                case EAssignment.BUSINESS:
                    mat = m_BusinessMat;
                    break;
                case EAssignment.INDUSTRY:
                    mat = m_IndustryMat;
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
