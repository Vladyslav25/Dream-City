﻿using Gameplay.Productions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ProductionQueueUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_Parent;
        [SerializeField]
        private GameObject m_ItemPrefab;

        private List<ProductionQueueItem> m_items = new List<ProductionQueueItem>();
        private Image m_background;

        private void Awake()
        {
            m_background = GetComponent<Image>();
            m_background.enabled = false;
        }

        public void RemoveItem(int _index)
        {
            Destroy(m_items[_index].gameObject);
            m_items.RemoveAt(_index);
            if (m_items.Count == 0)
                m_background.enabled = false;
        }

        public void AddToQueue(ProductionBuilding _pb)
        {
            m_background.enabled = true;
            GameObject obj = Instantiate(m_ItemPrefab, m_Parent.transform);
            ProductionQueueItem pqi = obj.GetComponent<ProductionQueueItem>();
            pqi.m_Manager = this;
            pqi.m_Title.text = _pb.m_UIName;
            m_items.Add(pqi);
        }

        public void ChangeColor(int _index, Color _color)
        {
            Image img = m_items[_index].GetComponent<Image>();
            img.color = _color;
        }
    }
}
