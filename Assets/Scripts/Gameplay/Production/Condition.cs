using System;
using Gameplay.Buildings;
using Grid;

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
        private float m_ValueToCompare;
        Product product = null;
        Production production = null;
        string sValueCompare = "";

        public override string ToString()
        {
            UpdateValues();
            string sign = "";
            switch (m_Compare)
            {
                case EComparisonSign.GREATER_AS:
                    sign = "mehr als ";
                    break;
                case EComparisonSign.GREATER_EQUAL:
                    sign = "gleich oder mehr als  ";
                    break;
                case EComparisonSign.EQUAL:
                    sign = "gleich ";
                    break;
                case EComparisonSign.SMALLER_AS:
                    sign = "weniger als ";
                    break;
                case EComparisonSign.SMALLER_EQUAL:
                    sign = "gleich oder weniger als";
                    break;
            }
            return sign + m_Value.ToString() + sValueCompare;
        }

        public bool UpdateValues()
        {
            float value = 0;
            product = null;
            production = null;

            switch (m_EValueType)
            {
                case EValueType.NONE:
                    break;
                case EValueType.INFLOW: //Inflow

                    switch (m_EValue_Inflow)
                    {
                        case EValue_Inflow.POPULATION:
                            value = BuildingManager.Instance.Living_CurrAmount;
                            sValueCompare = " Einwohnern";
                            break;
                        case EValue_Inflow.JOBS:
                            value = BuildingManager.Instance.Industry_CurrAmount;
                            sValueCompare = " Industrie Jobs";
                            break;
                        case EValue_Inflow.STALLS:
                            value = BuildingManager.Instance.Business_CurrAmount;
                            sValueCompare = " Verkaufsständen";
                            break;
                    }
                    break;

                case EValueType.CELL_AMOUNT: //Cells

                    switch (m_EValue_Cell)
                    {
                        case EValue_Cell.LIVING_CELLS:
                            value = GridManager.m_AllIndustryCells.Count;
                            sValueCompare = "m² Wohngebiet";
                            break;
                        case EValue_Cell.BUSINESS_CELLS:
                            value = GridManager.m_AllBusinessCells.Count;
                            sValueCompare = "m² Gewerbegebiet";
                            break;
                        case EValue_Cell.INDUSTRY_CELLS:
                            value = GridManager.m_AllIndustryCells.Count;
                            sValueCompare = "m² Industriegebiet";
                            break;
                    }
                    break;

                case EValueType.MONEY: //Money

                    switch (m_EValue_Money)
                    {
                        case EValue_Money.MONEY_BALANCE:
                            value = Inventory.Instance.m_MoneyBalance;
                            sValueCompare = "€ Einkommen";
                            break;
                        case EValue_Money.MONEY_AMOUNT:
                            value = Inventory.Instance.m_MoneyAmount;
                            sValueCompare = "€ Vermögen";
                            break;
                    }
                    break;

                case EValueType.INVENTORY_BALANCE: //Inventory Balance

                    switch (m_EValue_IBalance)
                    {
                        case EValue_IBalance.COAL_ORE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COAL_ORE);
                            break;
                        case EValue_IBalance.IRON_ORE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.IRON_ORE);
                            break;
                        case EValue_IBalance.COPPER_ORE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COPPER_ORE);
                            break;
                        case EValue_IBalance.GOLD_ORE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.GOLD_ORE);
                            break;
                        case EValue_IBalance.SILVER_ORE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.SILVER_ORE);
                            break;
                        case EValue_IBalance.RUBBER_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.RUBBER);
                            break;
                        case EValue_IBalance.OIL_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.OIL);
                            break;
                        case EValue_IBalance.IRON_INGOT_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.IRON_INGOT);
                            break;
                        case EValue_IBalance.COPPER_INGOT_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COPPER_INGOT);
                            break;
                        case EValue_IBalance.GOLD_INGOT_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.GOLD_INGOT);
                            break;
                        case EValue_IBalance.SILVER_INGOT_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.SILVER_INGOT);
                            break;
                        case EValue_IBalance.PLASTIC_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.PLASTIC);
                            break;
                        case EValue_IBalance.TIRE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.TIRES);
                            break;
                        case EValue_IBalance.CABLE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CABLE);
                            break;
                        case EValue_IBalance.IRON_PLATE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.IRON_PLATES);
                            break;
                        case EValue_IBalance.CIRCUIT_BOARD_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CIRCUIT_BOARD);
                            break;
                        case EValue_IBalance.ENGINE_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.ENGINE);
                            break;
                        case EValue_IBalance.CARBODY_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CARBODY);
                            break;
                        case EValue_IBalance.COMPUTER_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COMPUTER);
                            break;
                        case EValue_IBalance.CAR_BALANCE:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CAR);
                            break;
                    }
                    if (product != null)
                    {
                        if (Inventory.Instance.m_ProductionBalance.ContainsKey(product))
                        {
                            sValueCompare = " " + product.m_UI_Name + " Förderung";
                            value = Inventory.Instance.m_ProductionBalance[product];
                            break;
                        }
                        else
                        {
                            sValueCompare = " ??? Förderung";
                            return false;
                        }
                    }
                    else return false;

                case EValueType.INVENTORY_AMOUNT: //Inventory Amount

                    switch (m_EValue_IAmount)
                    {
                        case EValue_IAmount.COAL_ORE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COAL_ORE);
                            break;
                        case EValue_IAmount.IRON_ORE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.IRON_ORE);
                            break;
                        case EValue_IAmount.COPPER_ORE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COPPER_ORE);
                            break;
                        case EValue_IAmount.GOLD_ORE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.GOLD_ORE);
                            break;
                        case EValue_IAmount.SILVER_ORE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.SILVER_ORE);
                            break;
                        case EValue_IAmount.RUBBER_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.RUBBER);
                            break;
                        case EValue_IAmount.OIL_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.OIL);
                            break;
                        case EValue_IAmount.IRON_INGOT_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.IRON_INGOT);
                            break;
                        case EValue_IAmount.COPPER_INGOT_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COPPER_INGOT);
                            break;
                        case EValue_IAmount.GOLD_INGOT_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.GOLD_INGOT);
                            break;
                        case EValue_IAmount.SILVER_INGOT_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.SILVER_INGOT);
                            break;
                        case EValue_IAmount.PLASTIC_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.PLASTIC);
                            break;
                        case EValue_IAmount.TIRE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.TIRES);
                            break;
                        case EValue_IAmount.CABLE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CABLE);
                            break;
                        case EValue_IAmount.IRON_PLATE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.IRON_PLATES);
                            break;
                        case EValue_IAmount.CIRCUIT_BOARD_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CIRCUIT_BOARD);
                            break;
                        case EValue_IAmount.ENGINE_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.ENGINE);
                            break;
                        case EValue_IAmount.CARBODY_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CARBODY);
                            break;
                        case EValue_IAmount.COMPUTER_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.COMPUTER);
                            break;
                        case EValue_IAmount.CAR_INVENTORY:
                            product = Inventory.Instance.GetProductByEnum(EProduct.CAR);
                            break;
                    }
                    if (product != null)
                    {
                        if (Inventory.Instance.m_Inventory.ContainsKey(product))
                        {
                            sValueCompare = " " + product.m_UI_Name + " im Besitz";
                            value = Inventory.Instance.m_Inventory[product];
                            break;
                        }
                        else
                        {
                            sValueCompare = " ??? im Besitz";
                            return false;
                        }
                    }
                    else return false;

                case EValueType.PRODUCTIONBUILDING_AMOUNT: //Production Building Amount

                    switch (m_EValue_PAmount)
                    {
                        case EValue_PAmount.COAL_ORE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.COAL_MINE);
                            break;
                        case EValue_PAmount.IRON_ORE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.IRON_MINE);
                            break;
                        case EValue_PAmount.COPPER_ORE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.COPPER_MINE);
                            break;
                        case EValue_PAmount.GOLD_ORE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.GOLD_MINE);
                            break;
                        case EValue_PAmount.SILVER_ORE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.SILVER_MINE);
                            break;
                        case EValue_PAmount.RUBBER_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.RUBBER_MANUFACTURE);
                            break;
                        case EValue_PAmount.OIL_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.OIL_PUMP);
                            break;
                        case EValue_PAmount.IRON_INGOT_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.IRON_MELTER);
                            break;
                        case EValue_PAmount.COPPER_INGOT_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.COPPER_MELTER);
                            break;
                        case EValue_PAmount.GOLD_INGOT_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.GOLD_MELTER);
                            break;
                        case EValue_PAmount.SILVER_INGOT_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.SILVER_MELTER);
                            break;
                        case EValue_PAmount.PLASTIC_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.PLASTIC_MANUFACTURE);
                            break;
                        case EValue_PAmount.TIRE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.TIRES_MANUFACTURE);
                            break;
                        case EValue_PAmount.CABLE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.CABLE_MANUFACTURE);
                            break;
                        case EValue_PAmount.IRON_PLATE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.IRON_PRESS);
                            break;
                        case EValue_PAmount.CIRCUIT_BOARD_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.MICRO_ELECTRONICS_MANUFACTURER);
                            break;
                        case EValue_PAmount.ENGINE_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.ENGINE_MANUFACTURE);
                            break;
                        case EValue_PAmount.CARBODY_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.CARBODY_MANUFACTURE);
                            break;
                        case EValue_PAmount.COMPUTER_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.ELECTRONICS_MANUFACTURER);
                            break;
                        case EValue_PAmount.CAR_PRODUCTION_AMOUNT:
                            production = Inventory.Instance.GetProductionByEnmun(EProduction.CAR_MANUFACTURE);
                            break;
                    }
                    if (production != null)
                    {
                        if (Inventory.Instance.m_ProductionBuildingAmount.ContainsKey(production))
                        {
                            sValueCompare = BuildingManager.Instance.productionBuilding_Dic[production].m_UIName + " erbaut";
                            value = Inventory.Instance.m_ProductionBuildingAmount[production];
                            break;
                        }
                        else
                        {
                            sValueCompare = " ??? erbaut";
                            return false;
                        }
                    }
                    else return false;
            }

            m_ValueToCompare = value;
            return true;
        }

        public bool CheckCondition()
        {
            if (UpdateValues())
            {
                switch (m_Compare)
                {
                    case EComparisonSign.GREATER_AS:
                        if (m_Value < m_ValueToCompare)
                            return true;
                        else
                            return false;
                    case EComparisonSign.GREATER_EQUAL:
                        if (m_Value <= m_ValueToCompare)
                            return true;
                        else
                            return false;
                    case EComparisonSign.EQUAL:
                        if (m_Value == m_ValueToCompare)
                            return true;
                        else
                            return false;
                    case EComparisonSign.SMALLER_AS:
                        if (m_Value > m_ValueToCompare)
                            return true;
                        else
                            return false;
                    case EComparisonSign.SMALLER_EQUAL:
                        if (m_Value >= m_ValueToCompare)
                            return true;
                        else
                            return false;
                }
            }
            return false;
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
}
