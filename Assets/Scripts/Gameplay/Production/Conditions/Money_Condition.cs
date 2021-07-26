using UnityEngine;

namespace Gameplay.Productions.Conditions
{
    [CreateAssetMenu(fileName = "Condition_Money", menuName = "Production/Condition/Money", order = 2)]
    public class Money_Condition : ACondition
    {
        [Header("Type")]
        [SerializeField]
        private EValue_Money m_MoneyType;

        private bool multiValues = true;

        public override string GetString()
        {
            UpdateValue();

            if (string.IsNullOrEmpty(m_valueCompareString))
                switch (m_MoneyType)
                {
                    case EValue_Money.MONEY_BALANCE:
                        m_valueCompareString = "€ Einkommen";
                        break;
                    case EValue_Money.MONEY_AMOUNT:
                        m_valueCompareString = "€ Vermögen";
                        break;
                    default:
                        break;
                }

            return base.GetString();
        }

        public override bool UpdateValue()
        {
            if (multiValues)
            {
                m_GoalValue *= 100f;
                multiValues = false;
            }

            switch (m_MoneyType)
            {
                case EValue_Money.MONEY_BALANCE:
                    m_value = Inventory.Instance.m_MoneyBalance;
                    break;
                case EValue_Money.MONEY_AMOUNT:
                    m_value = Inventory.Instance.m_MoneyAmount;
                    break;
                default:
                    break;
            }
            m_value *= 100f;
            return true;
        }
    }
}
