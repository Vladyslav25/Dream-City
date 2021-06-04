using Gameplay.StreetComponents;
using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
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
        }

        private GameObject PlaceAt(EAssignment _assignment, Vector2Int _size, OrientedPoint _op, Street _s)
        {
            return Instantiate(GetRandomPrefab(_assignment, _size),_op.Position, _op.Rotation, _s.transform);
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

            System.Random rng = new System.Random();
            int index = rng.Next(0, validPrefabs.Count);
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
    }
}
