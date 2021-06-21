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
        public Dictionary<Product, float> m_Bilance = new Dictionary<Product, float>();

        public HashSet<Production> m_Productions = new HashSet<Production>();
        public Dictionary<Production, int> m_ProductionBuildingAmount = new Dictionary<Production, int>();

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
                CalculateCurrentProduction();
                CalculateNeededProduction();
                for (int i = 0; i < m_Inventory.Keys.Count; i++)
                {
                    Product p = m_Inventory.ElementAt(i).Key;
                    if (m_CurrendProduction.ContainsKey(p) && m_NeededProduction.ContainsKey(p))
                    {
                        float add = (m_CurrendProduction[p] - m_NeededProduction[p]) * (1f / 60f);
                        m_Inventory[p] += add;
                        if (m_Inventory[p] <= 0f)
                        {
                            m_Inventory[p] = 0f;
                            SetRatioInOutputProducts(p);
                        }
                        Debug.Log("Product: " + p.m_UI_Name + " | In Inventory: " + m_Inventory[p] + " | Add: " + add);
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        public void AddProductionBuilding(ProductionBuilding _pb)
        {
            if (!m_ProductionBuildingAmount.ContainsKey(_pb.m_Production))
                m_ProductionBuildingAmount.Add(_pb.m_Production, 0);
            m_ProductionBuildingAmount[_pb.m_Production]++;

            foreach (ProductionStat ps in _pb.m_Production.m_Output)
            {
                if (!m_CurrendProduction.ContainsKey(ps.m_Product))
                    m_CurrendProduction.Add(ps.m_Product, 0f);

                if (!m_NeededProduction.ContainsKey(ps.m_Product))
                    m_NeededProduction.Add(ps.m_Product, 0f);

                if (!m_Inventory.ContainsKey(ps.m_Product))
                    m_Inventory.Add(ps.m_Product, 0f);

                SetRatioInOutputProducts(ps.m_Product);
            }

            foreach (ProductionStat ps in _pb.m_Production.m_Input)
            {
                if (!m_CurrendProduction.ContainsKey(ps.m_Product))
                    m_CurrendProduction.Add(ps.m_Product, 0f);

                if (!m_NeededProduction.ContainsKey(ps.m_Product))
                    m_NeededProduction.Add(ps.m_Product, 0f);

                if (!m_Inventory.ContainsKey(ps.m_Product))
                    m_Inventory.Add(ps.m_Product, 0f);

                SetRatioInOutputProducts(ps.m_Product);
            }

            CalculateCurrentProduction();
            CalculateNeededProduction();
        }

        public void RemoveProductionBuilding(ProductionBuilding _pb)
        {
            if (m_ProductionBuildingAmount.ContainsKey(_pb.m_Production))
                m_ProductionBuildingAmount[_pb.m_Production]--;

            foreach (ProductionStat ps in _pb.m_Production.m_Output)
            {
                SetRatioInOutputProducts(ps.m_Product);
            }

            foreach (ProductionStat ps in _pb.m_Production.m_Input)
            {
                SetRatioInOutputProducts(ps.m_Product);
            }
        }

        private float GetRatio(Product _p)
        {
            if (!m_NeededProduction.ContainsKey(_p))
                m_NeededProduction.Add(_p, 0f);

            float max = m_NeededProduction[_p];
            float min = 0; //max * 0.75f;
            return Mathf.Clamp((m_CurrendProduction[_p] - min) / (max - min), 0f, 1f);
        }

        private void SetRatioInOutputProducts(Product _p)
        {
            if (m_Inventory[_p] > 0) return;

            float ratio = GetRatio(_p);

            foreach (Production production in m_Productions)
            {
                foreach (ProductionStat ps in production.m_Input)
                {
                    if (ps.m_Product == _p)
                    {
                        if (production.m_Ratio > ratio)
                        {
                            production.m_Ratio = ratio;
                            Debug.Log("Set Ratio to " + ratio + " in " + production);
                        }
                    }
                }
            }
        }

        private void CalculateCurrentProduction()
        {
            if (m_CurrendProduction.Count == 0) return;

            for (int i = 0; i < m_CurrendProduction.Count; i++)
            {
                Product p = m_Inventory.ElementAt(i).Key;
                m_CurrendProduction[p] = 0;
            }

            for (int i = 0; i < m_Productions.Count; i++)
            {
                Production production = m_Productions.ElementAt(i);
                if (m_ProductionBuildingAmount.ContainsKey(production))
                    for (int ii = 0; ii < production.m_Output.Count; ii++)
                    {
                        ProductionStat ps = production.m_Output[ii];
                        m_CurrendProduction[ps.m_Product] += ps.m_Amount * production.m_Ratio * m_ProductionBuildingAmount[production];
                    }
            }
        }

        private void CalculateNeededProduction()
        {
            if (m_NeededProduction.Count == 0) return;

            for (int i = 0; i < m_NeededProduction.Count; i++)
            {
                Product p = m_Inventory.ElementAt(i).Key;
                m_NeededProduction[p] = 0;
            }

            for (int i = 0; i < m_Productions.Count; i++)
            {
                Production production = m_Productions.ElementAt(i);
                if (m_ProductionBuildingAmount.ContainsKey(production))
                    for (int ii = 0; ii < production.m_Input.Count; ii++)
                    {
                        ProductionStat ps = production.m_Input[ii];
                        m_NeededProduction[ps.m_Product] += ps.m_Amount * m_ProductionBuildingAmount[production];
                    }
            }
        }
    }
}
