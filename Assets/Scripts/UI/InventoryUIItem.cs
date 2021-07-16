using Gameplay.Productions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventoryUIItem : MonoBehaviour
    {
        [SerializeField]
        private Image m_Image;
        [SerializeField]
        private Text m_InventoryAmount;
        [SerializeField]
        private Text m_Balance;
        [SerializeField]
        private Text m_Income;
        [SerializeField]
        private Image m_BG;
        [HideInInspector]
        public Product m_Product;

        public void Init(Product _p)
        {
            m_Product = _p;
            m_Image.sprite = _p.m_UI_Sprit;
            ChangeBGColor(new Color32(0xFF, 0xEE, 0x99, 0xFF));

            GetComponent<Button>()?.onClick.AddListener(
                delegate
            {
                UIManager.Instance.OpenInventorySellUI(this);
            }
            );
        }

        public void UpdateItem(Product _p)
        {
            float inventory = Inventory.Instance.m_Inventory[_p];
            float balance = Inventory.Instance.m_ProductionBalance[_p];

            if (inventory <= 0)
                m_InventoryAmount.color = Color.red;
            else
                m_InventoryAmount.color = Color.black;

            m_InventoryAmount.text = UIManager.ConvertFloatToStringDigit(inventory);
            m_Balance.text = UIManager.ConvertFloatToStringDigitWithSign(balance, out Color c);
            m_Balance.color = c;
            UpdateIncome();
        }

        public void UpdateIncome()
        {
            float price = 0f;
            if (m_Product.IsSellingWorld)
                price = m_Product.m_PriceWorld;
            else
                price = m_Product.m_PriceLocal;
            m_Income.text = UIManager.ConvertFloatToStringPrice(Inventory.Instance.m_SellingAmount[m_Product] * price);
        }

        public void ChangeBGColor(Color _color)
        {
            m_BG.color = _color;
        }
    }
}
