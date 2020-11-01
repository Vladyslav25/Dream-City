using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    public class SplineManager : MonoBehaviour
    {
        #region -SingeltonPattern-
        private static SplineManager _instance;
        public static SplineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<SplineManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("SplineManager");
                        _instance = container.AddComponent<SplineManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private static Dictionary<int, Spline> splineDic = new Dictionary<int, Spline>();
        private static int splineId;

        public static Spline GetSplineByID(int _id)
        {
            if (splineDic.ContainsKey(_id))
            {
                return splineDic[_id];
            }
            Debug.LogError($"No Spline with ID: {_id} found");
            return null;
        }

        public static int GetNewSplineID()
        {
            splineId++;
            return splineId;
        }

        public static Spline CreateSpline(Vector3 _startPos, Vector3 _tangent, Vector3 _endPos)
        {
            Spline s = Instance.gameObject.AddComponent<Spline>();
            s.Init(_startPos, _tangent, _endPos);
            splineDic.Add(s.ID, s);
            return s;
        }
    }
}
