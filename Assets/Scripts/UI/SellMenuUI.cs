using Gameplay.Productions;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SellMenuUI : MonoBehaviour
    {
        [SerializeField]
        private Text m_Title;
        [SerializeField]
        private Text m_CurrPrice;
        [SerializeField]
        private Text m_CurrProduction;
        [SerializeField]
        private InputField m_InputField;
        [HideInInspector]
        public Product m_currProduct;

        [SerializeField]
        private Button m_SellLocal_Btn;
        [SerializeField]
        private Button m_SellWorld_Btn;

        private void Start()
        {
            m_InputField.onValidateInput += delegate (string _input, int _charIndex, char _addedChar)
            {
                return ValidateInput(_addedChar);
            };

            m_InputField.onValueChanged.AddListener(delegate { SetSellAmount(); });

            UIManager.Instance.HighlightButton(m_SellLocal_Btn, true);
            m_SellLocal_Btn.interactable = false;
        }

        public void UpdateInfo(Product _product)
        {
            m_currProduct = _product;
            if (_product.IsSellingWorld)
                SetCurrPrice(m_currProduct.m_PriceWorld);
            else
                SetCurrPrice(m_currProduct.m_PriceLocal);
            SetCurrProduction(Inventory.Instance.m_ProductionBalance[_product]);
            m_Title.text = _product.m_UI_Name;

            if (_product.IsSellingWorld)
            {
                UIManager.Instance.HighlightButton(m_SellWorld_Btn, true);
                m_SellWorld_Btn.interactable = false;
                m_SellLocal_Btn.interactable = true;
            }
            else
            {
                UIManager.Instance.HighlightButton(m_SellLocal_Btn, true);
                m_SellLocal_Btn.interactable = false;
                m_SellWorld_Btn.interactable = true;
            }

            m_InputField.text = Inventory.Instance.m_SellingAmount[_product].ToString();
        }

        public void SetSellingWorld()
        {
            if (m_currProduct == null) return;
            m_currProduct.IsSellingWorld = true;
            UIManager.Instance.HighlightButton(m_SellWorld_Btn, true);
            m_SellWorld_Btn.interactable = false;
            m_SellLocal_Btn.interactable = true;
            SetSellAmount();
            SetCurrPrice(m_currProduct.m_PriceWorld);
        }

        public void SetSellingLocal()
        {
            if (m_currProduct == null) return;
            m_currProduct.IsSellingWorld = false;
            UIManager.Instance.HighlightButton(m_SellLocal_Btn, true);
            m_SellWorld_Btn.interactable = true;
            m_SellLocal_Btn.interactable = false;
            SetSellAmount();
            SetCurrPrice(m_currProduct.m_PriceLocal);
        }

        public char ValidateInput(char _addedChar)
        {
            if (!int.TryParse(_addedChar.ToString(), out int value))
            {
                return '\0';
            }
            return _addedChar;
        }

        private void SetSellAmount()
        {
            //remove Living Impact if selling on world
            if (m_currProduct.IsSellingWorld)
                Gameplay.Buildings.BuildingManager.Instance.Living_NeedAmount -= m_currProduct.m_ImpactOnLiving * Inventory.Instance.m_SellingAmount[m_currProduct];

            //after remove set new Amount
            Inventory.Instance.SetSellingAmount(m_currProduct, UIManager.ConvertStringToFloat(m_InputField.text));
            UIManager.Instance.UpdateInventoryItem(m_currProduct);

            //add living impact if selling on local
            if (!m_currProduct.IsSellingWorld)
                Gameplay.Buildings.BuildingManager.Instance.Living_NeedAmount += m_currProduct.m_ImpactOnLiving * Inventory.Instance.m_SellingAmount[m_currProduct];
        }

        private void SetCurrPrice(float _amount)
        {
            m_CurrPrice.text = UIManager.ConvertFloatToStringPrice(_amount);
        }

        private void SetCurrProduction(float _amount)
        {
            string sign = "";
            if (_amount < 0)
            {
                m_CurrProduction.color = Color.red;
            }
            else if (_amount == 0)
            {
                m_CurrProduction.color = Color.yellow;
                sign = "";
            }
            else
            {
                m_CurrProduction.color = Color.black;
                sign = "+";
            }

            m_CurrProduction.text = sign + " " + UIManager.ConvertFloatToStringDigit(_amount);
        }
    }
}
