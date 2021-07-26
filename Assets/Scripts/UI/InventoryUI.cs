using Gameplay.Productions;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{

    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_InventoryItem_Prefab;
        [SerializeField]
        private Transform m_Row01;
        [SerializeField]
        private Transform m_Row02;

        public Dictionary<Product, InventoryUIItem> m_ItemProduct_Dic = new Dictionary<Product, InventoryUIItem>();

        public void CreateItem(Product _p)
        {
            if (m_ItemProduct_Dic.ContainsKey(_p)) return;

            GameObject obj = null;
            if (m_Row01.transform.childCount <= 9)
                obj = Instantiate(m_InventoryItem_Prefab, m_Row01);
            else if (m_Row02.transform.childCount <= 9)
                obj = Instantiate(m_InventoryItem_Prefab, m_Row02);
            else
                Debug.LogError("To much Items");

            if (obj == null) return;

            m_ItemProduct_Dic.Add(_p, obj.GetComponent<InventoryUIItem>());
            m_ItemProduct_Dic[_p].Init(_p);
        }
    }
}
