using Gameplay.Buildings;
using Gameplay.Productions.Conditions;
using Gameplay.Productions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace UI
{
    public class ProductionBuildingUIItem : MonoBehaviour
    {
        [HideInInspector]
        public ProductionBuilding m_PB;

        [SerializeField]
        private GameObject Output_Prefab;
        [SerializeField]
        private GameObject Input_Prefab;
        [SerializeField]
        private TextMeshProUGUI m_Title;
        [SerializeField]
        private RectTransform m_Rect;
        [SerializeField]
        private TextMeshProUGUI m_Cost;
        [SerializeField]
        private TextMeshProUGUI m_Balance;
        [SerializeField]
        private Button m_Btn;
        [SerializeField]
        private Image m_UnlockBG;
        [SerializeField]
        private TextMeshProUGUI m_UnlockText;

        private string m_conditionText;
        private bool m_mouseOnButton;

        private List<ACondition> m_Conditions;

        public void OnClick()
        {
            BuildingManager.Instance.AddProductionBuildingToList(m_PB);
        }

        public void Init(ProductionBuilding _pb)
        {
            m_PB = _pb;
            m_Title.text = m_PB.m_UIName;
            m_Cost.text = UIManager.ConvertFloatToStringPrice(_pb.m_Cost);
            m_Balance.text = UIManager.ConvertFloatToStringPriceWithSign(_pb.m_OperatingCost, out Color c);
            m_Balance.color = c;
            m_Btn.interactable = false;
            m_UnlockBG.color = Color.red;
            m_Conditions = _pb.m_Production.m_Conditions;

            foreach (ProductionStat ps in m_PB.m_Production.m_Output)
            {
                GameObject obj = Instantiate(Output_Prefab, transform);
                obj.transform.SetSiblingIndex(1);
                m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_Rect.rect.height + 50f);
                TextMeshProUGUI t = null;
                foreach (Transform child in obj.transform)
                {
                    if (child.gameObject.tag == "EditText")
                    {
                        t = child.GetComponent<TextMeshProUGUI>();
                        break;
                    }
                }
                if (t == null)
                {
                    Debug.LogError("No EditText Tag found in: " + obj);
                    return;
                }
                t.text = ps.m_Amount.ToString() + " " + ps.m_Product.m_UI_Name;
                if (ps.m_Product.m_UI_Sprit != null)
                {
                    Image i = obj.GetComponent<Image>();
                    i.sprite = ps.m_Product.m_UI_Sprit;
                }
            }

            foreach (ProductionStat ps in m_PB.m_Production.m_Input)
            {
                GameObject obj = Instantiate(Input_Prefab, transform);
                obj.transform.SetSiblingIndex(1);
                m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_Rect.rect.height + 50f);
                TextMeshProUGUI t = null;
                foreach (Transform child in obj.transform)
                {
                    if (child.gameObject.tag == "EditText")
                    {
                        t = child.GetComponent<TextMeshProUGUI>();
                        break;
                    }
                }
                if (t == null)
                {
                    Debug.LogError("No EditText Tag found in: " + obj);
                    return;
                }
                t.text = ps.m_Amount.ToString() + " " + ps.m_Product.m_UI_Name;
                if (ps.m_Product.m_UI_Sprit != null)
                {
                    Image i = obj.GetComponent<Image>();
                    i.sprite = ps.m_Product.m_UI_Sprit;
                }
            }
        }

        public void UnlockProduction()
        {
            m_Btn.interactable = true;
            m_UnlockText.text = "Freigeschaltet";
            m_UnlockBG.color = Color.green;
        }

        public void UpdateConditionsText()
        {
            m_conditionText = "";

            foreach (ACondition condition in m_Conditions)
            {
                if(condition == null)
                {
                    Debug.LogError("Condition is null");
                    return;
                }
                m_conditionText += condition.GetString() + "\n";
            }

            if (m_mouseOnButton && !m_Btn.interactable) //Update Text in ToolTip
                ToolTip.ShowToolTip(m_conditionText);
        }

        public void OnHowerEnter()
        {
            if (m_Btn.interactable) return; //No ToolTip if Production is Unlocked
            ToolTip.ShowToolTip(m_conditionText);
            m_mouseOnButton = true;
        }

        public void OnHowerEnd()
        {
            ToolTip.HideToolTip();
            m_mouseOnButton = false;
        }
    }
}
