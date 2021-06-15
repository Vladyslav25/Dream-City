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
    }
}
