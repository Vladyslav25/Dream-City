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
        public string m_UIName;

        public override void Destroy()
        {
            base.Destroy();

            foreach (ProductionStat ps in m_Production.m_Output)
            {
                Inventory.Instance.AddCurrendProduction(ps.m_Product, -ps.m_Amount);
            }

            foreach (ProductionStat ps in m_Production.m_Input)
            {
                Inventory.Instance.AddNeededProduction(ps.m_Product, -ps.m_Amount);
            }
        }
    }
}
