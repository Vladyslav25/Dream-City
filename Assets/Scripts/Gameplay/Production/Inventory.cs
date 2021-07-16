using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UI;
using Gameplay.Productions.Conditions;

namespace Gameplay.Productions
{
    public class Inventory : MonoBehaviour
    {
        [Header("For Testing")]
        [SerializeField]
        private bool unlockAll = false;

        public Dictionary<Product, float> m_Inventory = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_CurrendProduction = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_NeededProduction = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_ProductionBalance = new Dictionary<Product, float>();
        public Dictionary<Product, float> m_SellingAmount = new Dictionary<Product, float>();

        [HideInInspector]
        public List<Production> m_AllProductions = new List<Production>();
        public Dictionary<Production, int> m_ProductionBuildingAmount = new Dictionary<Production, int>();

        [HideInInspector]
        public float m_MoneyAmount = 0f;
        [HideInInspector]
        public float m_MoneyBalance = 0f;
        [HideInInspector]
        public float m_OperatingCostSum = 0f;
        [HideInInspector]
        public float m_TaxIncome = 0f;

        private int m_sellingWorldAmount = 0;
        [SerializeField]
        private int m_maxSellingWorldAmount;

        public bool CanSellInWorldMarket
        {
            get
            {
                if (m_sellingWorldAmount >= m_maxSellingWorldAmount) return false;
                return true;
            }
        }

        public List<Product> m_AllProducts = new List<Product>();

        private int m_currProductionIndex;
        private Production m_currProductionToCheck;

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
            m_currProductionIndex = 0;
            m_currProductionToCheck = m_AllProductions[m_currProductionIndex];
            StartCoroutine(CalculateInventory());
            m_MoneyAmount = 2500f;
        }

        private IEnumerator CalculateInventory()
        {
            while (true)
            {
                //Production
                CalculateCurrentProduction();
                CalculateNeededProduction();
                HashSet<Production> ProductionsToUpdateRatio = new HashSet<Production>();
                m_MoneyBalance = 0f;
                foreach (Production production in m_AllProductions)
                {
                    foreach (ProductionStat productionStat in production.m_Output)
                    {
                        Product p = productionStat.m_Product;
                        if (m_Inventory.ContainsKey(p) && m_CurrendProduction.ContainsKey(p) && m_NeededProduction.ContainsKey(p))
                        {
                            float balance = m_CurrendProduction[p] - m_NeededProduction[p] - m_SellingAmount[p];
                            float add = balance * (1f / 60f);

                            if (p.IsSellingWorld && m_Inventory[p] > 0)
                                m_MoneyBalance += p.m_PriceWorld * m_SellingAmount[p];
                            else
                                m_MoneyBalance += p.m_PriceLocal * m_SellingAmount[p];

                            m_Inventory[p] += add;
                            if (m_Inventory[p] <= 0f)
                            {
                                m_Inventory[p] = 0f;
                            }
                            m_ProductionBalance[p] = balance;
                            UIManager.Instance.UpdateInventoryItem(p);
                            UIManager.Instance.UpdateCurrendProductSellUI(p);
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
                }

                //Money
                m_MoneyBalance += m_TaxIncome;
                m_MoneyBalance += m_OperatingCostSum;
                m_MoneyAmount += m_MoneyBalance * (1f / 60f);
                UIManager.Instance.UpdateMoneyUI();

                //Condition
                if (m_currProductionToCheck != null && (m_currProductionToCheck.CheckConditions() || unlockAll))
                {
                    UIManager.Instance.UnlockProduction(m_currProductionToCheck);
                    m_currProductionIndex++;
                    if (m_currProductionIndex < m_AllProductions.Count)
                        m_currProductionToCheck = m_AllProductions[m_currProductionIndex];
                    else //if all Productions are Unlocked
                        m_currProductionToCheck = null;
                }

                yield return new WaitForSeconds(1f);
            }
        }

        public void AddSellInWorldMarket() => m_sellingWorldAmount++;

        public void RemoveSellInWorldMarket() => m_sellingWorldAmount--;

        public Product GetProductByEnum(EProduct _eProduct)
        {
            foreach (Product product in m_AllProducts)
            {
                if (product.m_EProduct == _eProduct)
                    return product;
            }
            Debug.LogError("No Product: " + _eProduct.ToString() + " found");
            return null;
        }

        public Production GetProductionByEnmun(EProduction _eProduction)
        {
            foreach (Production production in m_AllProductions)
            {
                if (production.m_EProduction == _eProduction)
                    return production;
            }
            Debug.LogError("No Production: " + _eProduction.ToString() + " found");
            return null;
        }

        public void AddProductionBuilding(ProductionBuilding _pb)
        {
            if (!m_ProductionBuildingAmount.ContainsKey(_pb.m_Production))
                m_ProductionBuildingAmount.Add(_pb.m_Production, 0);
            m_ProductionBuildingAmount[_pb.m_Production]++;

            foreach (ProductionStat ps in _pb.m_Production.m_Input)
            {
                LookForAddProduct(m_CurrendProduction, ps.m_Product);
                LookForAddProduct(m_NeededProduction, ps.m_Product);
                LookForAddProduct(m_Inventory, ps.m_Product);
                LookForAddProduct(m_ProductionBalance, ps.m_Product);
                LookForAddProduct(m_SellingAmount, ps.m_Product);
            }

            foreach (ProductionStat ps in _pb.m_Production.m_Output)
            {
                LookForAddProduct(m_CurrendProduction, ps.m_Product);
                LookForAddProduct(m_NeededProduction, ps.m_Product);
                LookForAddProduct(m_Inventory, ps.m_Product);
                LookForAddProduct(m_ProductionBalance, ps.m_Product);
                LookForAddProduct(m_SellingAmount, ps.m_Product);
            }

            _pb.m_Production.m_Ratio = GetRatio(_pb.m_Production);
            CalculateCurrentProduction();
            CalculateNeededProduction();
            m_OperatingCostSum += _pb.m_OperatingCost;
            m_MoneyAmount -= _pb.m_Cost;
        }

        public void RemoveProductionBuilding(ProductionBuilding _pb)
        {
            if (m_ProductionBuildingAmount.ContainsKey(_pb.m_Production))
                m_ProductionBuildingAmount[_pb.m_Production]--;

            _pb.m_Production.m_Ratio = GetRatio(_pb.m_Production);
            m_OperatingCostSum -= _pb.m_OperatingCost;
        }

        public void SetSellingAmount(Product _p, float _amount)
        {
            m_SellingAmount[_p] = _amount;
            UIManager.Instance.UpdateInventoryItem(_p);
            UIManager.Instance.UpdateInventorySellUI(_p);
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
                    if (!m_CurrendProduction.ContainsKey(ps.m_Product))
                        return 0;
                    float balance = m_CurrendProduction[ps.m_Product] - m_NeededProduction[ps.m_Product];
                    //if an input item is missing in the inventory AND
                    //if one balance is 0 or less, no production possible
                    if (balance < 0 && m_Inventory[ps.m_Product] <= 0) return 0;

                    if (balance < 0 && balance > -ps.m_Amount)
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

            for (int i = 0; i < m_AllProductions.Count; i++)
            {
                Production production = m_AllProductions.ElementAt(i);
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

            for (int i = 0; i < m_AllProductions.Count; i++)
            {
                Production production = m_AllProductions.ElementAt(i);
                if (m_ProductionBuildingAmount.ContainsKey(production))
                    for (int ii = 0; ii < production.m_Input.Count; ii++)
                    {
                        ProductionStat ps = production.m_Input[ii];
                        m_NeededProduction[ps.m_Product] += ps.m_Amount * m_ProductionBuildingAmount[production];
                    }
            }
        }

        private void LookForAddProduct(Dictionary<Product, float> _dic, Product _p, float _startVaslue = 0f)
        {
            if (!_dic.ContainsKey(_p))
                _dic.Add(_p, _startVaslue);
        }
    }
}
