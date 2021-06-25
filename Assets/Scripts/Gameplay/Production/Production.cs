using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Productions
{
    [CreateAssetMenu(fileName = "Production", menuName = "Production/Production", order = 1)]
    public class Production : ScriptableObject
    {
        public EProduction m_EProduction;
        public string m_UIName;

        [Header("Input")]
        public List<ProductionStat> m_Input;

        [Header("Output")]
        public List<ProductionStat> m_Output;

        public Condition m_Condition;

        [HideInInspector]
        public float m_Ratio = 1f;
    }
}
