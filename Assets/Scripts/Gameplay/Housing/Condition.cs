using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Productions
{
    [Serializable]
    public class Condition
    {
        public EValueType m_EValueType;

        public EValue_Inflow m_EValue_Inflow;
        public EValue_Cell m_EValue_Cell;
        public EValue_Money m_EValue_Money;
        public EValue_IBalance m_EValue_IBalance;
        public EValue_IAmount m_EValue_IAmount;
        public EValue_PAmount m_EValue_PAmount;

        public EComparisonSign m_Compare;
        public float m_Value;

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
}
