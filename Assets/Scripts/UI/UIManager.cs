using Gameplay.Streets;
using Gameplay.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject toolChoose;
        [SerializeField]
        GameObject streetType;
        [SerializeField]
        GameObject assignment;

        private StreetTool streetTool;
        private CellAssignmentTool cellTool;
        private Outline lastOutline;

        #region -SingeltonPattern-
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("MeshGenerator");
                        _instance = container.AddComponent<UIManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private void Start()
        {
            streetTool = ToolManager.Instance.GetStreetTool();
            cellTool = ToolManager.Instance.GetCellAssignmentTool();
            SetActivToolChoose();
            ToolManager.Instance.ChangeTool(TOOLTYPE.NONE);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetOutline(lastOutline);
                SetActivToolChoose();
                ToolManager.Instance.ChangeTool(TOOLTYPE.NONE);
            }
        }

        public void SetActivToolChoose()
        {
            toolChoose.SetActive(true);
            streetType.SetActive(false);
            assignment.SetActive(false);
        }

        public void SetActivStreetType()
        {
            toolChoose.SetActive(false);
            streetType.SetActive(true);
            assignment.SetActive(false);
        }

        public void SetActivAssignment()
        {
            toolChoose.SetActive(false);
            streetType.SetActive(false);
            assignment.SetActive(true);
        }

        public void DeactivateUI()
        {
            toolChoose.SetActive(false);
            streetType.SetActive(false);
            assignment.SetActive(false);
        }

        public void OnClickStreetTool()
        {
            SetActivStreetType();
            ToolManager.Instance.ChangeTool(TOOLTYPE.STREET);
        }

        public void OnClickCrossTool()
        {
            DeactivateUI();
            ToolManager.Instance.ChangeTool(TOOLTYPE.CROSS);
        }

        public void OnClickAssignmentTool()
        {
            SetActivAssignment();
            ToolManager.Instance.ChangeTool(TOOLTYPE.ASSIGNMENT);
        }

        public void OnClickLine(Outline _o)
        {
            streetTool.SetCurveLineTool(true);
            IncreaseOutline(_o);
        }

        public void OnClickCurve(Outline _o)
        {
            streetTool.SetCurveLineTool(false);
            IncreaseOutline(_o);
        }

        public void OnClickLiving(Outline _o)
        {
            cellTool.m_CurrendAssignment = Grid.CellAssignment.LIVING;
            IncreaseOutline(_o);
        }

        public void OnClickBusiness(Outline _o)
        {
            cellTool.m_CurrendAssignment = Grid.CellAssignment.BUSINESS;
            IncreaseOutline(_o);
        }

        public void OnClickIndustry(Outline _o)
        {
            cellTool.m_CurrendAssignment = Grid.CellAssignment.INDUSTRY;
            IncreaseOutline(_o);
        }

        private void IncreaseOutline(Outline _o)
        {
            ResetOutline(lastOutline);
            _o.effectDistance = new Vector2(3, 3);
            lastOutline = _o;
        }

        private void ResetOutline(Outline _o)
        {
            if (_o != null)
                _o.effectDistance = new Vector2(1, 1);
        }
    }
}
