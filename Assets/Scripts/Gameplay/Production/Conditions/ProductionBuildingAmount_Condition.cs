using Gameplay.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Productions.Conditions
{
    [CreateAssetMenu(fileName = "Condition_ProductionBuildingAmount", menuName = "Production/Condition/ProductionBuildingAmount", order = 5)]
    public class ProductionBuildingAmount_Condition : ACondition
    {
        [Header("Type")]
        [SerializeField]
        private EProduction m_ProductionType;

        private Production m_production;
        public override string GetString()
        {
            if (UpdateValue())
            {
                if (string.IsNullOrEmpty(m_valueCompareString) && m_production != null)
                    m_valueCompareString = BuildingManager.Instance.productionBuilding_Dic[m_production].m_UIName + " erbaut";
            }
            else
            {
                m_valueCompareString = " ??? erbaut";
            }

            return base.GetString();
        }
        public override bool UpdateValue()
        {
            if (m_production == null)
                m_production = Inventory.Instance.GetProductionByEnmun(m_ProductionType);
            if (m_production != null && Inventory.Instance.m_ProductionBuildingAmount.ContainsKey(m_production))
            {
                m_value = Inventory.Instance.m_ProductionBuildingAmount[m_production];
                return true;
            }
            else
                return false;
        }
    }
}
