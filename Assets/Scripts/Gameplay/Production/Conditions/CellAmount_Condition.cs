using UnityEngine;

namespace Gameplay.Productions.Conditions
{
    [CreateAssetMenu(fileName = "Condition_CellAmount", menuName = "Production/Condition/CellAmount", order = 1)]
    public class CellAmount_Condition : ACondition
    {
        [Header("Type")]
        [SerializeField]
        private EValue_Cell m_CellType;

        public override string GetString()
        {
            UpdateValue();

            if (string.IsNullOrEmpty(m_valueCompareString))
                switch (m_CellType)
                {
                    case EValue_Cell.LIVING_CELLS:
                        m_valueCompareString = " m² Wohnfläche";
                        break;
                    case EValue_Cell.BUSINESS_CELLS:
                        m_valueCompareString = " m² Gewerbefläche";
                        break;
                    case EValue_Cell.INDUSTRY_CELLS:
                        m_valueCompareString = " m² Industryfläche";
                        break;
                    default:
                        break;
                }

            return base.GetString();
        }

        public override bool UpdateValue()
        {
            switch (m_CellType)
            {
                case EValue_Cell.LIVING_CELLS:
                    m_value = Grid.GridManager.m_AllLivingCells.Count;
                    break;
                case EValue_Cell.BUSINESS_CELLS:
                    m_value = Grid.GridManager.m_AllBusinessCells.Count;
                    break;
                case EValue_Cell.INDUSTRY_CELLS:
                    m_value = Grid.GridManager.m_AllIndustryCells.Count;
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}
