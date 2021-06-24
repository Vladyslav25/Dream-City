using Gameplay.Productions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ProductionInfoUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_ProductionInfoItem_Prefab;
        [SerializeField]
        private Transform m_InputTransform;
        [SerializeField]
        private Text m_RatioText;
        [SerializeField]
        private Transform m_OutputTransform;
        [SerializeField]
        private TextMeshProUGUI m_Title;
        [SerializeField]
        private TextMeshProUGUI m_OperatingCost;

        public void SetProductionInfo(ProductionBuilding _pb)
        {
            m_Title.text = _pb.m_UIName;
            m_OperatingCost.text = UIManager.ConvertFloatToStringPriceWithSign(_pb.m_OperatingCost, out Color c);
            m_OperatingCost.color = c;

            foreach (Transform child in m_InputTransform.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in m_OutputTransform.transform)
            {
                Destroy(child.gameObject);
            }

            Production p = _pb.m_Production;
            if (p.m_Input.Count == 0)
            {
                GameObject obj = Instantiate(m_ProductionInfoItem_Prefab, m_InputTransform);
                ProductionInfoItem pii = obj.GetComponent<ProductionInfoItem>();
                pii.SetAmount("∞");
            }
            else
                foreach (ProductionStat ps in p.m_Input)
                {
                    GameObject obj = Instantiate(m_ProductionInfoItem_Prefab, m_InputTransform);
                    ProductionInfoItem pii = obj.GetComponent<ProductionInfoItem>();
                    pii.SetAmount(ps.m_Amount);
                    pii.SetIcon(ps.m_Product.m_UI_Sprit);
                }

            if (p.m_Output.Count == 0)
            {
                GameObject obj = Instantiate(m_ProductionInfoItem_Prefab, m_InputTransform);
                ProductionInfoItem pii = obj.GetComponent<ProductionInfoItem>();
                pii.SetAmount("∞");
            }
            else
                foreach (ProductionStat ps in p.m_Output)
                {
                    GameObject obj = Instantiate(m_ProductionInfoItem_Prefab, m_OutputTransform);
                    ProductionInfoItem pii = obj.GetComponent<ProductionInfoItem>();
                    pii.SetAmount(ps.m_Amount);
                    pii.SetIcon(ps.m_Product.m_UI_Sprit);
                }

            m_RatioText.text = (_pb.m_Production.m_Ratio * 100) + " %";
        }
    }
}
