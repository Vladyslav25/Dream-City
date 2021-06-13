using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Production
{
    [CreateAssetMenu(fileName = "Product", menuName = "Production/Product", order = 0)]
    public class Product : ScriptableObject
    {
        public EProduct m_Product;
        public string m_Name;
        public float m_Price;
        public int m_ImpactOnLiving;
    }
}
