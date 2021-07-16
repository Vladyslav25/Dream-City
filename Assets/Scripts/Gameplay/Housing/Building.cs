using UnityEngine;

namespace Gameplay.Buildings
{
    public class Building : ABuilding
    {
        [Header("Settings")]
        public EDemand m_Density;
        public float Tax;

        public override void Destroy()
        {
            Productions.Inventory.Instance.m_TaxIncome -= Tax;
            base.Destroy();
        }
    }
}
