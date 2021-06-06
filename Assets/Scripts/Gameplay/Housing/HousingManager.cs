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

        private float living_NeedAmount;
        private float business_NeedAmount;
        private float industry_NeedAmount;

        public float m_Living_NeedAmount
        {
            get { return living_NeedAmount; }
            set
            {
                living_NeedAmount = value;
                m_Living_Ratio = GetRatio(living_CurrAmount, living_NeedAmount);
            }
        }
        public float m_Business_NeedAmount
        {
            get { return business_NeedAmount; }
            set
            {
                business_NeedAmount = value;
                m_Business_Ratio = GetRatio(business_CurrAmount, business_NeedAmount);
            }
        }
        public float m_Industry_NeedAmount
        {
            get { return industry_NeedAmount; }
            set
            {
                industry_NeedAmount = value;
                m_Industry_Ratio = GetRatio(industry_CurrAmount, industry_NeedAmount);
            }
        }

        private float living_CurrAmount;
        private float business_CurrAmount;
        private float industry_CurrAmount;

        public float m_Living_CurrAmount
        {
            get { return living_CurrAmount; }
            set
            {
                living_CurrAmount = value;
                m_Living_Ratio = GetRatio(living_CurrAmount, living_NeedAmount);
            }
        }
        public float m_Business_CurrAmount
        {
            get { return business_CurrAmount; }
            set
            {
                business_CurrAmount = value;
                m_Business_Ratio = GetRatio(business_CurrAmount, business_NeedAmount);
            }
        }
        public float m_Industry_CurrAmount
        {
            get { return industry_CurrAmount; }
            set
            {
                industry_CurrAmount = value;
                m_Industry_Ratio = GetRatio(industry_CurrAmount, industry_NeedAmount);
            }
        }

        private float living_Ratio;
        private float business_Ratio;
        private float industry_Ratio;

        private float m_Living_Ratio
        {
            get { return living_Ratio; }
            set
            {
                living_Ratio = value;
                m_LivingDemand = GetDemand(living_Ratio);
                Debug.Log(m_LivingDemand);
                UIManager.Instance.SetDemandRatio(EAssignment.LIVING, living_Ratio);
            }
        }
        private float m_Business_Ratio
        {
            get { return business_Ratio; }
            set
            {
                business_Ratio = value;
                m_BusinessDemand = GetDemand(business_Ratio);
                UIManager.Instance.SetDemandRatio(EAssignment.BUSINESS, business_Ratio);
            }
        }
        private float m_Industry_Ratio
        {
            get { return industry_Ratio; }
            set
            {
                industry_Ratio = value;
                m_IndustryDemand = GetDemand(industry_Ratio);
                UIManager.Instance.SetDemandRatio(EAssignment.INDUSTRY, industry_Ratio);
            }
        }

        [HideInInspector]
        public EDemand m_LivingDemand = 0;
        [HideInInspector]
        public EDemand m_BusinessDemand = 0;
        [HideInInspector]
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

            m_Living_NeedAmount = 5;
            m_Business_NeedAmount = 0;
            m_Industry_NeedAmount = 0;
        }

        public GameObject PlaceBuilding(Area _a)
        {
            GameObject prefab = GetRandomPrefab(_a.m_Assignment, _a.m_Size);
            if (prefab == null) return null;

            foreach (Cell c in _a.m_Cells)
            {
                c.IsBlocked = true;
            }

            Building newBuild = prefab.GetComponent<Building>();
            float[] impacts = newBuild.Impacts;
            float inflow = newBuild.Inflow;

            switch (_a.m_Assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    m_Living_CurrAmount += inflow;
                    break;
                case EAssignment.BUSINESS:
                    m_Business_CurrAmount += inflow;
                    break;
                case EAssignment.INDUSTRY:
                    m_Industry_CurrAmount += inflow;
                    break;
            }

            m_Living_NeedAmount += impacts[0];
            m_Business_NeedAmount += impacts[1];
            m_Industry_NeedAmount += impacts[2];

            if (prefab.GetComponent<Building>().InverseRotation)
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
                Debug.LogError("No Building with Size: " + _size + " and Assignment: " + _assignment + " found.");
                return null;
            }

            //Removes buildings whose density does not match the demand
            int DemandToCheck = 0;
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    DemandToCheck = (int)m_LivingDemand;
                    break;
                case EAssignment.BUSINESS:
                    DemandToCheck = (int)m_BusinessDemand;
                    break;
                case EAssignment.INDUSTRY:
                    DemandToCheck = (int)m_IndustryDemand;
                    break;
                default:
                    break;
            }

            List<GameObject> output = new List<GameObject>();

            for (int i = 0; i < validPrefabs.Count; i++)
            {
                Building b = validPrefabs[i].GetComponent<Building>();
                if (DemandToCheck == (int)b.m_Density)
                    output.Add(validPrefabs[i]);
            }

            if (output.Count == 0)
            {
                Debug.LogError($"No Building with Density: {(EDensity)DemandToCheck} and Assignment: {_assignment} found.");
                return null;
            }

            int index = Random.Range(0, output.Count);
            return output[index];
        }

        private void InitDictionary(List<GameObject> _prefList, Dictionary<Vector2Int, List<GameObject>> _dic)
        {
            foreach (GameObject obj in _prefList)
            {
                Building building = obj.GetComponent<Building>();
                if (building == null)
                {
                    Debug.LogError(obj + " have no Building Component");
                    continue;
                }
                if (!_dic.ContainsKey(building.Size))
                {
                    _dic.Add(building.Size, new List<GameObject>());
                }
                _dic[building.Size].Add(obj);
            }
        }

        private float GetRatio(float _input, float _max)
        {
            float min = _max * 0.7f - 15;
            return Mathf.Clamp(1 - ((_input - min) / (_max - min)), 0f, 1f);
        }

        private EDemand GetDemand(float _ratio)
        {
            if (_ratio < 0.21f)
                return EDemand.LOW;
            else if (_ratio < 0.5f)
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

    public enum EDensity
    {
        NONE,
        LOW,
        MID,
        HIGH
    }
}
