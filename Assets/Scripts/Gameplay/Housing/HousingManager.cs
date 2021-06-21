using Gameplay.Productions;
using Gameplay.StreetComponents;
using Grid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Gameplay.Buildings
{
    public class HousingManager : MonoBehaviour
    {
        [Header("For Testing")]
        [SerializeField]
        bool CheckDensity = true;

        [Header("Prefabs")]
        [SerializeField]
        private List<GameObject> m_LivingPrefabs;
        [SerializeField]
        private List<GameObject> m_BusinessPrefabs;
        [SerializeField]
        private List<GameObject> m_IndustryPrefabs;
        [SerializeField]
        private List<GameObject> m_ProductionPrefabs;

        private Dictionary<Vector2Int, List<GameObject>> livingPrefabs_Dic = new Dictionary<Vector2Int, List<GameObject>>();
        private Dictionary<Vector2Int, List<GameObject>> businessPrefabs_Dic = new Dictionary<Vector2Int, List<GameObject>>();
        private Dictionary<Vector2Int, List<GameObject>> industryPrefabs_Dic = new Dictionary<Vector2Int, List<GameObject>>();
        private Dictionary<Production, GameObject> productionPrefabs_Dic = new Dictionary<Production, GameObject>();

        public static List<Area> m_AllAreas = new List<Area>();
        [MyReadOnly]
        public List<Production> m_ProductionBuildWaitingList = new List<Production>();

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

        private int living_CurrAmount;
        private int business_CurrAmount;
        private int industry_CurrAmount;

        public int m_Living_CurrAmount
        {
            get { return living_CurrAmount; }
            set
            {
                living_CurrAmount = value;
                m_Living_Ratio = GetRatio(living_CurrAmount, living_NeedAmount);
            }
        }
        public int m_Business_CurrAmount
        {
            get { return business_CurrAmount; }
            set
            {
                business_CurrAmount = value;
                m_Business_Ratio = GetRatio(business_CurrAmount, business_NeedAmount);
            }
        }
        public int m_Industry_CurrAmount
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
            InitDictionary(m_BusinessPrefabs, businessPrefabs_Dic);
            InitDictionary(m_IndustryPrefabs, industryPrefabs_Dic);
            InitDictionary(m_ProductionPrefabs, productionPrefabs_Dic);

            m_Living_NeedAmount = 50;
            m_Business_NeedAmount = 0;
            m_Industry_NeedAmount = 0;
        }

        public void AddProductionBuildingToList(ProductionBuilding _pb)
        {
            UIManager.Instance.AddProductionItem(_pb);
            m_ProductionBuildWaitingList.Add(_pb.m_Production);
        }

        public void RemoveProductionBuilingInList(int _index = 0)
        {
            UIManager.Instance.RemoveProductionItem(_index);
            m_ProductionBuildWaitingList.RemoveAt(_index);
        }

        public GameObject PlaceProductionBuilding(Production _p, Street _s, bool _isLeftSide)
        {
            if (!productionPrefabs_Dic.ContainsKey(_p)) return null;
            GameObject prefab = productionPrefabs_Dic[_p];
            ProductionBuilding PB = prefab.GetComponent<ProductionBuilding>();

            if (_isLeftSide)
            {
                if (_s.FindAreaLeftSide(PB.Size, EAssignment.INDUSTRY, out Area a))
                    return SpawnPrefab(a, PB, prefab);
            }
            else
            {
                if (_s.FindAreaRightSide(PB.Size, EAssignment.INDUSTRY, out Area a))
                    return SpawnPrefab(a, PB, prefab);
            }

            return null;
        }

        private GameObject SpawnPrefab(Area _a, ProductionBuilding _pb, GameObject _prefab)
        {
            foreach (Cell c in _a.m_Cells)
                _a.m_Street.SetCellBlocked(c);

            int[] impacts = _pb.Impacts;
            int inflow = _pb.Inflow;

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


            GameObject obj = Instantiate(_prefab, _a.m_OP.Position, _a.m_OP.Rotation, _a.m_Street.transform);
            _a.Init(obj.GetComponent<ProductionBuilding>());
            m_AllAreas.Add(_a);
            RemoveProductionBuilingInList();
            Inventory.Instance.AddProductionBuilding(_pb);

            return obj;
        }

        private GameObject SpawnPrefab(Area _a, Building _b, GameObject _prefab)
        {
            foreach (Cell c in _a.m_Cells)
                _a.m_Street.SetCellBlocked(c);

            int[] impacts = _b.Impacts;
            int inflow = _b.Inflow;

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

            GameObject obj;
            if (_prefab.GetComponent<Building>().InverseRotation)
            {
                obj = Instantiate(_prefab, _a.m_OP.Position, _a.m_OP.Rotation * Quaternion.Euler(0, 180, 0), _a.m_Street.transform);
            }
            else
            {
                obj = Instantiate(_prefab, _a.m_OP.Position, _a.m_OP.Rotation * Quaternion.Euler(0, 0, 0), _a.m_Street.transform);
            }

            _a.Init(obj.GetComponent<Building>());
            m_AllAreas.Add(_a);
            return obj;
        }

        public GameObject PlaceBuilding(EAssignment _assignment, Street _s, bool _isLeftSide)
        {
            EDemand density = EDemand.NONE;
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    density = m_LivingDemand;
                    break;
                case EAssignment.BUSINESS:
                    density = m_BusinessDemand;
                    break;
                case EAssignment.INDUSTRY:
                    density = m_IndustryDemand;
                    break;
            }

            GameObject prefab = GetRandomPrefab(_assignment, density);
            if (prefab == null) return null;
            Building b = prefab.GetComponent<Building>();

            if (_isLeftSide)
            {
                if (_s.FindAreaLeftSide(b.Size, _assignment, out Area a))
                    return SpawnPrefab(a, b, prefab);
            }
            else
            {
                if (_s.FindAreaRightSide(b.Size, _assignment, out Area a))
                    return SpawnPrefab(a, b, prefab);
            }

            return null;
        }

        public GameObject PlaceBuilding(Area _a)
        {
            GameObject prefab = GetRandomPrefab(_a.m_Assignment, _a.m_Size);
            if (prefab == null) return null;
            Building newBuild = prefab.GetComponent<Building>();

            return SpawnPrefab(_a, newBuild, prefab);
        }

        private GameObject GetRandomPrefab(EAssignment _assignment, EDemand _density)
        {
            List<GameObject> validPrefabs = new List<GameObject>();
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    foreach (List<GameObject> objList in livingPrefabs_Dic.Values)
                    {
                        foreach (GameObject obj in objList)
                        {
                            if (CheckDensity)
                            {
                                Building b = obj.GetComponent<Building>();
                                if (b.m_Density == _density)
                                    validPrefabs.Add(obj);
                            }
                            else
                            {
                                validPrefabs.Add(obj);
                            }
                        }
                    }
                    break;
                case EAssignment.BUSINESS:
                    foreach (List<GameObject> objList in businessPrefabs_Dic.Values)
                    {
                        foreach (GameObject obj in objList)
                        {
                            if (CheckDensity)
                            {
                                Building b = obj.GetComponent<Building>();
                                if (b.m_Density == _density)
                                    validPrefabs.Add(obj);
                            }
                            else
                            {
                                validPrefabs.Add(obj);
                            }
                        }
                    }
                    break;
                case EAssignment.INDUSTRY:
                    foreach (List<GameObject> objList in industryPrefabs_Dic.Values)
                    {
                        foreach (GameObject obj in objList)
                        {
                            if (CheckDensity)
                            {
                                Building b = obj.GetComponent<Building>();
                                if (b.m_Density == _density)
                                    validPrefabs.Add(obj);
                            }
                            else
                            {
                                validPrefabs.Add(obj);
                            }
                        }
                    }
                    break;
            }
            if (validPrefabs.Count == 0) return null;
            int index = Random.Range(0, validPrefabs.Count);
            return validPrefabs[index];
        }

        private GameObject GetRandomPrefab(EAssignment _assignment, Vector2Int _size)
        {
            List<GameObject> validPrefabs = new List<GameObject>();
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    if (livingPrefabs_Dic.ContainsKey(_size))
                        validPrefabs = livingPrefabs_Dic[_size];
                    break;
                case EAssignment.BUSINESS:
                    if (businessPrefabs_Dic.ContainsKey(_size))
                        validPrefabs = businessPrefabs_Dic[_size];
                    break;
                case EAssignment.INDUSTRY:
                    if (industryPrefabs_Dic.ContainsKey(_size))
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
                if (CheckDensity)
                {
                    Building b = validPrefabs[i].GetComponent<Building>();
                    if (DemandToCheck == (int)b.m_Density)
                        output.Add(validPrefabs[i]);
                }
                else
                    output.Add(validPrefabs[i]);
            }

            if (output.Count == 0)
            {
                Debug.LogError($"No Building with Density: {(EDemand)DemandToCheck} and Assignment: {_assignment} found.");
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

        private void InitDictionary(List<GameObject> _prefList, Dictionary<Production, GameObject> _dic)
        {
            foreach (GameObject obj in _prefList)
            {
                ProductionBuilding pb = obj.GetComponent<ProductionBuilding>();
                if (pb == null)
                {
                    Debug.LogError("No ProductionBuilding Component found in: " + obj);
                    return;
                }

                if (!_dic.ContainsKey(pb.m_Production))
                {
                    _dic.Add(pb.m_Production, obj);
                    UIManager.Instance.InitProductionUI(pb);
                    pb.m_Production.m_Ratio = 1f;
                    Inventory.Instance.m_Productions.Add(pb.m_Production);
                }
                else
                {
                    Debug.LogError("Production: " + pb.m_Production + " already exist in Dictionary");
                }
            }
        }

        private float GetRatio(float _input, float _max)
        {
            float min = _max * 0.2f - 200;
            float max = _max;
            return Mathf.Clamp(1 - ((_input - min) / (max - min)), 0f, 1f);
        }

        private EDemand GetDemand(float _ratio)
        {
            if (_ratio == 0)
                return EDemand.NONE;
            else if (_ratio < 0.3f)
                return EDemand.LOW;
            else if (_ratio < 0.6f)
                return EDemand.LOWMID;
            else if (_ratio < 0.81f)
                return EDemand.HIGHMID;
            else
                return EDemand.HIGH;
        }
    }

    public enum EDemand
    {
        NONE,
        LOW,
        LOWMID,
        HIGHMID,
        HIGH
    }
}
