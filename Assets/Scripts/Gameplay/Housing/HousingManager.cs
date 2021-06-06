using Grid;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Gameplay.Building
{
    public class HousingManager : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> m_LivingPrefabs;
        [SerializeField]
        private List<GameObject> m_BusinessPrefabs;
        [SerializeField]
        private List<GameObject> m_IndustryPrefabs;

        private Dictionary<Vector2Int, List<GameObject>> livingPrefabs_Dic = new Dictionary<Vector2Int, List<GameObject>>();
        private Dictionary<Vector2Int, List<GameObject>> buisnessPrefabs_Dic = new Dictionary<Vector2Int, List<GameObject>>();
        private Dictionary<Vector2Int, List<GameObject>> industryPrefabs_Dic = new Dictionary<Vector2Int, List<GameObject>>();

        // between 0 - 1
        // 0 no Demand
        // 1 max Demand

        private float livingDemandAmount;
        private float businessDemandAmount;
        private float industryDemandAmount;

        public float LivingDemandAmount
        {
            get
            {
                return livingDemandAmount;
            }
            set
            {
                m_LivingDemand = SetDemand(ref livingDemandAmount, value);
                UIManager.Instance.SetDemand(EAssignment.LIVING, livingDemandAmount);
            }
        }

        public float BusinessDemandAmount
        {
            get
            {
                return businessDemandAmount;
            }
            set
            {
                m_BusinnesDemand = SetDemand(ref businessDemandAmount, value);
                UIManager.Instance.SetDemand(EAssignment.BUSINESS, businessDemandAmount);
            }
        }

        public float IndustryDemandAmount
        {
            get
            {
                return industryDemandAmount;
            }
            set
            {
                m_IndustryDemand = SetDemand(ref industryDemandAmount, value);
                UIManager.Instance.SetDemand(EAssignment.INDUSTRY, industryDemandAmount);
            }
        }

        public EDemand m_LivingDemand = 0;
        public EDemand m_BusinnesDemand = 0;
        public EDemand m_IndustryDemand = 0;

        #region -SingeltonPattern-
        private static HousingManager _instance;
        public static HousingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<HousingManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("SplineManager");
                        _instance = container.AddComponent<HousingManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private void Awake()
        {
            InitDictionary(m_LivingPrefabs, livingPrefabs_Dic);
            InitDictionary(m_BusinessPrefabs, buisnessPrefabs_Dic);
            InitDictionary(m_IndustryPrefabs, industryPrefabs_Dic);

            LivingDemandAmount = 0.2f;
            BusinessDemandAmount = 0;
            IndustryDemandAmount = 0;
        }

        public GameObject PlaceBuilding(Area _a)
        {
            GameObject prefab = GetRandomPrefab(_a.m_Assignment, _a.m_Size);
            if (prefab == null) return null;

            foreach (Cell c in _a.m_Cells)
            {
                c.IsBlocked = true;
            }

            float[] impacts = prefab.GetComponent<ABuilding>().Impacts;

            LivingDemandAmount += impacts[0];
            BusinessDemandAmount += impacts[1];
            IndustryDemandAmount += impacts[2];

            if (prefab.GetComponent<ABuilding>().InverseRotation)
            {
                return Instantiate(prefab, _a.m_OP.Position, _a.m_OP.Rotation * Quaternion.Euler(0, 180, 0), _a.m_Street.transform);
            }
            else
            {
                return Instantiate(prefab, _a.m_OP.Position, _a.m_OP.Rotation * Quaternion.Euler(0, 180, 0), _a.m_Street.transform);
            }
        }

        private GameObject GetRandomPrefab(EAssignment _assignment, Vector2Int _size)
        {
            List<GameObject> validPrefabs = new List<GameObject>();
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    validPrefabs = livingPrefabs_Dic[_size];
                    break;
                case EAssignment.BUSINESS:
                    validPrefabs = buisnessPrefabs_Dic[_size];
                    break;
                case EAssignment.INDUSTRY:
                    validPrefabs = industryPrefabs_Dic[_size];
                    break;
            }
            if (validPrefabs.Count == 0)
            {
                Debug.LogError("No Building withz Size: " + _size + " and assignment: " + _assignment + " found");
                return null;
            }

            int index = Random.Range(0, validPrefabs.Count);
            return validPrefabs[index];
        }

        private void InitDictionary(List<GameObject> _prefList, Dictionary<Vector2Int, List<GameObject>> _dic)
        {
            foreach (GameObject obj in _prefList)
            {
                ABuilding building = obj.GetComponent<ABuilding>();
                if (building == null)
                {
                    Debug.LogError(obj + " have no ABuilding Component");
                    continue;
                }
                if (!_dic.ContainsKey(building.Size))
                {
                    _dic.Add(building.Size, new List<GameObject>());
                }
                _dic[building.Size].Add(obj);
            }
        }

        private EDemand SetDemand(ref float _demandAmount, float _value)
        {
            _demandAmount = Mathf.Clamp(_value, 0f, 1f);
            if (_demandAmount < 0.21f)
                return EDemand.LOW;
            else if (_demandAmount < 0.5f)
                return EDemand.MID;
            else
                return EDemand.HIGH;
        }
    }

    public enum EDemand
    {
        NONE,
        LOW,
        MID,
        HIGH
    }
}
