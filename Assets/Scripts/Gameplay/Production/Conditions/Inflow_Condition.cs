using Gameplay.Buildings;
using UnityEngine;

namespace Gameplay.Productions.Conditions
{
    [CreateAssetMenu(fileName = "Condition_Inflow", menuName = "Production/Condition/Inflow", order = 0)]
    public class Inflow_Condition : ACondition
    {
        [Header("Type")]
        [SerializeField]
        private EValue_Inflow m_InflowType;

        public override string GetString()
        {
            UpdateValue();


            if (string.IsNullOrEmpty(m_valueCompareString))
                switch (m_InflowType)
                {
                    case EValue_Inflow.POPULATION:
                        m_valueCompareString = " Einwohner";
                        break;
                    case EValue_Inflow.JOBS:
                        m_valueCompareString = " Arbeitsplätze";
                        break;
                    case EValue_Inflow.STALLS:
                        m_valueCompareString = " Verkaufsstände";
                        break;
                    default:
                        break;
                }


            return base.GetString();
        }

        public override bool UpdateValue()
        {
            switch (m_InflowType)
            {
                case EValue_Inflow.POPULATION:
                    m_value = BuildingManager.Instance.Living_CurrAmount;
                    break;
                case EValue_Inflow.JOBS:
                    m_value = BuildingManager.Instance.Industry_CurrAmount;
                    break;
                case EValue_Inflow.STALLS:
                    m_value = BuildingManager.Instance.Business_CurrAmount;
                    break;
            }
            return true;
        }
    }
}
