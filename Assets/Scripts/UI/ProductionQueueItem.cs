using Gameplay.Buildings;
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
            BuildingManager.Instance.RemoveProductionBuilingInList(Index);
            Destroy(gameObject);
        }
    }
}
