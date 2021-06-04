using Gameplay.Streets;
using Gameplay.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

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
        private Image lastImage;
        private Button lastButton;

        //public Outline LineOutline, CurveOutline, LivingOutline, BusinessOutline, IndustryOutline;
        public Button LineButton, CurveButton, LivingButton, BusinessButton, IndustryButton;

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
                ResetHighlightButton();
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

        public void OnClickLine(Button sender)
        {
            streetTool.SetCurveLineTool(true);
            HighlightButton(sender);
        }

        public void OnClickCurve(Button sender)
        {
            streetTool.SetCurveLineTool(false);
            HighlightButton(sender);
        }

        public void OnClickLiving(Button sender)
        {
            cellTool.m_CurrendAssignment = Grid.EAssignment.LIVING;
            HighlightButton(sender);
        }

        public void OnClickBusiness(Button sender)
        {
            cellTool.m_CurrendAssignment = Grid.EAssignment.BUSINESS;
            HighlightButton(sender);
        }

        public void OnClickIndustry(Button sender)
        {
            cellTool.m_CurrendAssignment = Grid.EAssignment.INDUSTRY;
            HighlightButton(sender);
        }

        public void IncreaseOutline(Outline _o)
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

        /// <summary>
        /// Highlight a Button with Color and Outline. Also remove the Highlight of the last Button
        /// </summary>
        /// <param name="_b">The Button to Highlight</param>
        public void HighlightButton(Button _b)
        {
            if (_b == null) return;

            ResetHighlightButton();
            Image i = _b.GetComponent<Image>();
            i.color = Color.white;

            lastImage = i;

            Outline o = _b.GetComponent<Outline>();
            if (o != null)
                IncreaseOutline(o);
        }

        public void ResetHighlightButton()
        {
            if (lastImage != null)
                lastImage.color = new Color(0.77f, 0.77f, 0.77f, 1);
        }
    }
}
