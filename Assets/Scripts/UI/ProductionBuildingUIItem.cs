using Gameplay.Buildings;
using Gameplay.Productions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private Image m_BtnBG;
        [SerializeField]
        private TextMeshProUGUI m_ConditionText;

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
            m_BtnBG.color = Color.red;
            m_ConditionText.text = _pb.m_Production.m_Condition.ToString();

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
            m_BtnBG.color = Color.green;
        }
    }
}
