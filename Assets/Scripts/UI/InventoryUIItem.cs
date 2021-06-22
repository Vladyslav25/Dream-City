using Gameplay.Productions;
using System.Collections;
using System.Collections.Generic;
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

        public Product m_Product;

        public void Init(Product _p)
        {
            m_Product = _p;
            m_Image.sprite = _p.m_UI_Sprit;
        }

        public void UpdateItem(float _inventory, float _balance)
        {
            if (_inventory <= 0)
                m_InventoryAmount.color = Color.red;
            else
                m_InventoryAmount.color = Color.black;

            m_InventoryAmount.text = ConvertFloatToString(_inventory);

            string sign = "";
            if (_balance < 0)
            {
                m_Balance.color = Color.red;
            }
            else if (_balance == 0)
            {
                m_Balance.color = Color.yellow;
                sign = "";
            }
            else
            {
                m_Balance.color = Color.black;
                sign = "+";
            }

            m_Balance.text = sign + " " + ConvertFloatToString(_balance);
        }

        private string ConvertFloatToString(float _input)
        {
            return ((int)(_input * 10) * 0.1f).ToString();
        }
    }
}
