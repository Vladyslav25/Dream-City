using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Productions
{
    [CreateAssetMenu(fileName = "Product", menuName = "Production/Product", order = 0)]
    public class Product : ScriptableObject
    {
        [Tooltip("What Product is it")]
        public EProduct m_Product;
        [Tooltip("Which Name does the Product have in the UI")]
        public string m_UI_Name;
        [Tooltip("What is the Price of the Product on the World Market")]
        public float m_Price;
        [Tooltip("What is the Impact on the Needed Living Demand")]
        public int m_ImpactOnLiving;
    }
}
