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
                HashSet<Production> ProductionsToUpdateRatio = new HashSet<Production>();
                foreach (Production production in m_Productions)
                {
                    foreach (ProductionStat productionStat in production.m_Output)
                    {
                        Product p = productionStat.m_Product;
                        if (m_Inventory.ContainsKey(p) && m_CurrendProduction.ContainsKey(p) && m_NeededProduction.ContainsKey(p))
                        {
                            float balance = m_CurrendProduction[p] - m_NeededProduction[p];
                            float add = balance * (1f / 60f);
                            m_Inventory[p] += add;
                            if (m_Inventory[p] <= 0f)
                            {
                                m_Inventory[p] = 0f;
                            }
                            UI.UIManager.Instance.UpdateInventoryItem(p, m_Inventory[p], balance);
                        }
                        if (m_Inventory.ContainsKey(p))
                        {
                            ProductionsToUpdateRatio.Add(production);
                        }
                    }
                }
                foreach (Production p in ProductionsToUpdateRatio)
                {
                    p.m_Ratio = GetRatio(p);
                    //Debug.Log("Ratio for: " + p + " | Ratio set to: " + p.m_Ratio);
                }
                yield return new WaitForSeconds(1f);
            }
        }

        public void AddProductionBuilding(ProductionBuilding _pb)
        {
            if (!m_ProductionBuildingAmount.ContainsKey(_pb.m_Production))
                m_ProductionBuildingAmount.Add(_pb.m_Production, 0);
            m_ProductionBuildingAmount[_pb.m_Production]++;

            foreach (ProductionStat ps in _pb.m_Production.m_Input)
            {
                if (!m_CurrendProduction.ContainsKey(ps.m_Product))
                    m_CurrendProduction.Add(ps.m_Product, 0f);

                if (!m_NeededProduction.ContainsKey(ps.m_Product))
                    m_NeededProduction.Add(ps.m_Product, 0f);

                if (!m_Inventory.ContainsKey(ps.m_Product))
                    m_Inventory.Add(ps.m_Product, 0f);
            }

            foreach (ProductionStat ps in _pb.m_Production.m_Output)
            {
                if (!m_CurrendProduction.ContainsKey(ps.m_Product))
                    m_CurrendProduction.Add(ps.m_Product, 0f);

                if (!m_NeededProduction.ContainsKey(ps.m_Product))
                    m_NeededProduction.Add(ps.m_Product, 0f);

                if (!m_Inventory.ContainsKey(ps.m_Product))
                    m_Inventory.Add(ps.m_Product, 0f);
            }

            _pb.m_Production.m_Ratio = GetRatio(_pb.m_Production);
            CalculateCurrentProduction();
            CalculateNeededProduction();
        }

        public void RemoveProductionBuilding(ProductionBuilding _pb)
        {
            if (m_ProductionBuildingAmount.ContainsKey(_pb.m_Production))
                m_ProductionBuildingAmount[_pb.m_Production]--;

            _pb.m_Production.m_Ratio = GetRatio(_pb.m_Production);
        }

        private float GetRatio(Production _p)
        {
            float max = 0f;
            float min = 0f;
            float sum = 0f;

            if (_p.m_Input.Count == 0)
                return 1;
            else
                foreach (ProductionStat ps in _p.m_Input)
                {
                    float balance = m_CurrendProduction[ps.m_Product] - m_NeededProduction[ps.m_Product];
                    //if an input item is missing in the inventory AND
                    //if one balance is 0 or less, no production possible
                    if (balance < 0 && m_Inventory[ps.m_Product] <= 0) return 0;
                    
                    if(balance < 0 && balance > -ps.m_Amount)
                        sum += m_CurrendProduction[ps.m_Product];
                    else 
                        sum += m_NeededProduction[ps.m_Product];

                    max += m_NeededProduction[ps.m_Product];
                }
            if (max - min == 0)
            {
                Debug.Log("NaN");
                return 1f;
            }
            return Mathf.Clamp((sum - min) / (max - min), 0f, 1f);
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
