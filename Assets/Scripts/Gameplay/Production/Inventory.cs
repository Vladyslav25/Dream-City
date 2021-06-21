using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Productions
{

    public class Inventory : MonoBehaviour
    {
        public Dictionary<Product, float> m_Inventory = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_CurrendProduction = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_NeededProduction = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_Ratio = new Dictionary<Product, float>();

        #region -SingeltonPattern-
        private static Inventory _instance;
        public static Inventory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<Inventory>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("SplineManager");
                        _instance = container.AddComponent<Inventory>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private void Start()
        {
            StartCoroutine(CalculateInventory());
        }

        private IEnumerator CalculateInventory()
        {
            while (true)
            {
                for (int i = 0; i < m_Inventory.Keys.Count; i++)
                {

                    Product p = m_Inventory.ElementAt(i).Key;
                    float add = ((m_CurrendProduction[p] - m_NeededProduction[p]) * m_Ratio[p]) * (1f / 60f);
                    m_Inventory[p] += add;
                    float InInventory = ((int)(m_Inventory[p] * 10f)) * 0.1f;
                    Debug.Log("Product: " + p.m_UI_Name + "| In Inventory: " + InInventory + " | Added: " + add);
                }
                yield return new WaitForSeconds(1f);
            }
        }

        public void AddCurrendProduction(Product _p, float _amount)
        {
            if (!m_Inventory.ContainsKey(_p))
                m_Inventory.Add(_p, 0f);

            if (!m_CurrendProduction.ContainsKey(_p))
                m_CurrendProduction.Add(_p, 0f);
            m_CurrendProduction[_p] += _amount;

            if (!m_Ratio.ContainsKey(_p))
                m_Ratio.Add(_p, 1f);
            m_Ratio[_p] = GetRatio(_p);
        }

        public void AddNeededProduction(Product _p, float _amount)
        {
            if (!m_Inventory.ContainsKey(_p))
                m_Inventory.Add(_p, 0f);

            if (!m_NeededProduction.ContainsKey(_p))
                m_NeededProduction.Add(_p, 0f);
            m_NeededProduction[_p] += _amount;

            if (!m_Ratio.ContainsKey(_p))
                m_Ratio.Add(_p, 1f);
            m_Ratio[_p] = GetRatio(_p);
        }

        public float GetRatio(Product _p)
        {
            if (!m_NeededProduction.ContainsKey(_p))
                m_NeededProduction.Add(_p, 0f);

            float max = m_NeededProduction[_p];
            float min = 0; //max * 0.75f;
            return Mathf.Clamp((m_CurrendProduction[_p] - min) / (max - min), 0f, 1f);
        }
    }
}
