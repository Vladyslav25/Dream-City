using Gameplay.Productions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SellMenuUI : MonoBehaviour
    {
        [SerializeField]
        private Text m_CurrPrice;
        [SerializeField]
        private Text m_CurrProduction;

        private Product m_currProduct;

        public void UpdateInfo(Product _product)
        {
            m_currProduct = _product;
            if (_product.IsSellingWorld)
                SetCurrPrice(m_currProduct.m_PriceWorld);
            else
                SetCurrPrice(m_currProduct.m_PriceLocal);
            SetCurrProduction(Inventory.Instance.m_Bilance[_product]);
        }

        public void SetSellingWorld()
        {
            if (m_currProduct == null) return;
            m_currProduct.IsSellingWorld = true;
            SetCurrPrice(m_currProduct.m_PriceWorld);
        }

        public void SetSellingLocal()
        {
            if (m_currProduct == null) return;
            m_currProduct.IsSellingWorld = false;
            SetCurrPrice(m_currProduct.m_PriceLocal);
        }

        private void SetCurrPrice(float _amount)
        {
            m_CurrPrice.text = ((int)(_amount * 10f) * 0.01f) + " €";
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

            m_CurrProduction.text = sign + " " + ConvertFloatToString(_amount);
        }

        private string ConvertFloatToString(float _input)
        {
            return ((int)(_input * 10) * 0.1f).ToString();
        }
    }
}
