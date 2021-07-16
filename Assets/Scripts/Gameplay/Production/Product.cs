using UnityEngine;

namespace Gameplay.Productions
{
    [CreateAssetMenu(fileName = "Product", menuName = "Production/Product", order = 0)]
    public class Product : ScriptableObject
    {
        [Tooltip("What Product is it")]
        public EProduct m_EProduct;
        [Tooltip("Which Name does the Product have in the UI")]
        public string m_UI_Name;
        [Tooltip("The UI Icon")]
        public Sprite m_UI_Sprit;
        [Tooltip("What is the Price of the Product on the World Market")]
        public float m_PriceWorld;
        [Tooltip("What is the Price of the Product on the World Market")]
        public float m_PriceLocal;
        [Tooltip("What is the Impact on the Needed Living Demand")]
        public int m_ImpactOnLiving;

        [MyReadOnly]
        public bool IsSellingWorld = false;
    }
}
