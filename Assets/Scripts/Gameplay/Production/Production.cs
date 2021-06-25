using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Productions
{
    [CreateAssetMenu(fileName = "Production", menuName = "Production/Production", order = 1)]
    public class Production : ScriptableObject
    {
        [Header("Input")]
        public List<ProductionStat> m_Input;

        [Header("Output")]
        public List<ProductionStat> m_Output;

        public Condition m_Condition;

        [HideInInspector]
        public float m_Ratio = 1f;
    }
}
