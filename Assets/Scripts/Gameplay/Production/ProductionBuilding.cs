using Gameplay.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Productions
{
    public class ProductionBuilding : ABuilding
    {
        [Header("Production")]
        public Production m_Production;
        public float m_Cost;
        public float m_OperatingCost;
        [Space()]
        public string m_UIName;

        public override void Destroy()
        {
            Inventory.Instance.RemoveProductionBuilding(this);
            
            base.Destroy();
        }
    }
}
