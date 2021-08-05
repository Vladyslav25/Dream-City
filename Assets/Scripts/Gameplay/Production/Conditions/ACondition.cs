using UnityEngine;

namespace Gameplay.Productions.Conditions
{
    public abstract class ACondition : ScriptableObject
    {
        [Header("Values")]
        [SerializeField]
        protected EComparisonSign m_CompareSign;
        [SerializeField]
        protected float m_GoalValue;

        protected string m_valueCompareString;
        protected string m_signString;
        protected float m_value;

        public virtual string GetString()
        {
            if (string.IsNullOrEmpty(m_signString))
                switch (m_CompareSign)
                {
                    case EComparisonSign.GREATER_AS:
                        m_signString = "mehr als ";
                        break;
                    case EComparisonSign.GREATER_EQUAL:
                        m_signString = "gleich oder mehr als ";
                        break;
                    case EComparisonSign.EQUAL:
                        m_signString = "gleich ";
                        break;
                    case EComparisonSign.SMALLER_AS:
                        m_signString = "weniger als ";
                        break;
                    case EComparisonSign.SMALLER_EQUAL:
                        m_signString = "gleich oder weniger als";
                        break;
                }

            return (m_signString + m_GoalValue.ToString() + m_valueCompareString + $" (aktuell: {m_value})").Trim();
        }

        public abstract bool UpdateValue();

        public bool CheckCondition()
        {
            if (UpdateValue())
            {
                switch (m_CompareSign)
                {
                    case EComparisonSign.GREATER_AS:
                       if (m_value > m_GoalValue)
                            return true;
                        else
                            return false;
                    case EComparisonSign.GREATER_EQUAL:
                        if (m_value >= m_GoalValue)
                            return true;
                        else
                            return false;
                    case EComparisonSign.EQUAL:
                        if (m_value == m_GoalValue)
                            return true;
                        else
                            return false;
                    case EComparisonSign.SMALLER_AS:
                        if (m_value < m_GoalValue)
                            return true;
                        else
                            return false;
                    case EComparisonSign.SMALLER_EQUAL:
                        if (m_value <= m_GoalValue)
                            return true;
                        else
                            return false;
                }
            }
            return false;
        }
    }

    public enum EValueType
    {
        NONE,
        INFLOW,
        CELL_AMOUNT,
        MONEY,
        INVENTORY_BALANCE,
        INVENTORY_AMOUNT,
        PRODUCTIONBUILDING_AMOUNT
    }

    public enum EValue_Inflow
    {
        POPULATION,
        JOBS,
        STALLS,
    }

    public enum EValue_Cell
    {
        LIVING_CELLS,
        BUSINESS_CELLS,
        INDUSTRY_CELLS,
    }

    public enum EValue_Money
    {
        MONEY_BALANCE,
        MONEY_AMOUNT,
    }

    public enum EValue_IBalance
    {
        //Inventory Balance
        //Tier 1
        COAL_ORE_BALANCE,
        IRON_ORE_BALANCE,
        COPPER_ORE_BALANCE,
        GOLD_ORE_BALANCE,
        SILVER_ORE_BALANCE,
        RUBBER_BALANCE,
        OIL_BALANCE,

        //Tier 2
        IRON_INGOT_BALANCE,
        COPPER_INGOT_BALANCE,
        GOLD_INGOT_BALANCE,
        SILVER_INGOT_BALANCE,
        PLASTIC_BALANCE,
        TIRE_BALANCE,

        //Tier 3
        CABLE_BALANCE,
        IRON_PLATE_BALANCE,

        //Tier 4
        CIRCUIT_BOARD_BALANCE,
        ENGINE_BALANCE,

        //Tier 5
        CARBODY_BALANCE,
        COMPUTER_BALANCE,

        //Tier 6
        CAR_BALANCE,
    }

    public enum EValue_IAmount
    {
        //Inventory Amount
        //Tier 1
        COAL_ORE_INVENTORY,
        IRON_ORE_INVENTORY,
        COPPER_ORE_INVENTORY,
        GOLD_ORE_INVENTORY,
        SILVER_ORE_INVENTORY,
        RUBBER_INVENTORY,
        OIL_INVENTORY,

        //Tier 2
        IRON_INGOT_INVENTORY,
        COPPER_INGOT_INVENTORY,
        GOLD_INGOT_INVENTORY,
        SILVER_INGOT_INVENTORY,
        PLASTIC_INVENTORY,
        TIRE_INVENTORY,

        //Tier 3
        CABLE_INVENTORY,
        IRON_PLATE_INVENTORY,

        //Tier 4
        CIRCUIT_BOARD_INVENTORY,
        ENGINE_INVENTORY,

        //Tier 5
        CARBODY_INVENTORY,
        COMPUTER_INVENTORY,

        //Tier 6
        CAR_INVENTORY,

    }

    public enum EValue_PAmount
    {
        //Production Amount
        //Tier 1
        COAL_ORE_PRODUCTION_AMOUNT,
        IRON_ORE_PRODUCTION_AMOUNT,
        COPPER_ORE_PRODUCTION_AMOUNT,
        GOLD_ORE_PRODUCTION_AMOUNT,
        SILVER_ORE_PRODUCTION_AMOUNT,
        RUBBER_PRODUCTION_AMOUNT,
        OIL_PRODUCTION_AMOUNT,

        //Tier 2
        IRON_INGOT_PRODUCTION_AMOUNT,
        COPPER_INGOT_PRODUCTION_AMOUNT,
        GOLD_INGOT_PRODUCTION_AMOUNT,
        SILVER_INGOT_PRODUCTION_AMOUNT,
        PLASTIC_PRODUCTION_AMOUNT,
        TIRE_PRODUCTION_AMOUNT,

        //Tier 3
        CABLE_PRODUCTION_AMOUNT,
        IRON_PLATE_PRODUCTION_AMOUNT,

        //Tier 4
        CIRCUIT_BOARD_PRODUCTION_AMOUNT,
        ENGINE_PRODUCTION_AMOUNT,

        //Tier 5
        CARBODY_PRODUCTION_AMOUNT,
        COMPUTER_PRODUCTION_AMOUNT,

        //Tier 6
        CAR_PRODUCTION_AMOUNT,
    }

    public enum EComparisonSign
    {
        GREATER_AS,
        GREATER_EQUAL,
        EQUAL,
        SMALLER_AS,
        SMALLER_EQUAL,
    }
}
