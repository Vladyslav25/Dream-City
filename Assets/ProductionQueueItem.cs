using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ProductionQueueItem : MonoBehaviour
    {
        public int Index
        {
            get
            {
                return transform.GetSiblingIndex();
            }
        }

        [HideInInspector]
        public ProductionQueueUI m_Manager;
        public Text m_Title;

        public void OnClickRemove()
        {
            m_Manager.RemoveItem(Index);
            Destroy(gameObject);
        }
    }
}
