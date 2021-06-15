using Gameplay.Buildings;
using Gameplay.Productions;
using System.Collections;
using System.Collections.Generic;
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
        private GameObject Text_Parent;
        [SerializeField]
        private Text m_Title;

        public void OnClick()
        {
            HousingManager.Instance.AddProductionBuildingToList(m_PB);
        }

        public void Init(ProductionBuilding _pb)
        {
            m_PB = _pb;
            m_Title.text = m_PB.m_UIName;

            foreach (ProductionStat ps in m_PB.m_Production.m_Output)
            {
                GameObject obj = Instantiate(Output_Prefab, Text_Parent.transform);
                Text t = null;
                foreach (Transform child in obj.transform)
                {
                    if (child.gameObject.tag == "EditText")
                    {
                        t = child.GetComponent<Text>();
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
                GameObject obj = Instantiate(Input_Prefab, Text_Parent.transform);
                Text t = null;
                foreach (Transform child in obj.transform)
                {
                    if (child.gameObject.tag == "EditText")
                    {
                        t = child.GetComponent<Text>();
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
    }
}
