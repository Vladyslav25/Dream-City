using UnityEngine;

namespace Gameplay.Productions.Conditions
{
    [CreateAssetMenu(fileName = "Condition_InventoryAmount", menuName = "Production/Condition/InventoryAmount", order = 4)]
    public class InventoryAmount_Condition : ACondition
    {
        [Header("Type")]
        [SerializeField]
        private EProduct m_ProductType;

        private Product m_product;

        public override string GetString()
        {
            if (UpdateValue()) //if Product is found
            {
                if (!string.IsNullOrEmpty(m_valueCompareString) && m_product != null)
                    m_valueCompareString = $" {m_product.m_UI_Name} im Besitz";
            }
            else
            {
                m_valueCompareString = " ??? im Besitz";
            }
            return base.GetString();
        }

        public override bool UpdateValue()
        {
            if (m_product == null)
                m_product = Inventory.Instance.GetProductByEnum(m_ProductType);
            if (m_product != null && Inventory.Instance.m_Inventory.ContainsKey(m_product))
            {
                m_value = Inventory.Instance.m_Inventory[m_product];
                return true;
            }
            else
                return false;
        }
    }
}
