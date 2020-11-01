using MeshGeneration;
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

        [SerializeField]
        private Material myDefault;

        private static Dictionary<int, Spline> splineID_Dic = new Dictionary<int, Spline>();
        private static int setSplineId;

        public static Spline GetSplineByID(int _id)
        {
            if (splineID_Dic.ContainsKey(_id))
            {
                return splineID_Dic[_id];
            }
            Debug.LogError($"No Spline with ID: {_id} found");
            return null;
        }

        public static int GetNewSplineID()
        {
            setSplineId++;
            return setSplineId;
        }

        public static Spline CreateSpline(Vector3 _startPos, Vector3 _tangent, Vector3 _endPos)
        {
            GameObject obj = new GameObject("Spline");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);
            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.myDefault;

            Spline s = obj.gameObject.AddComponent<Spline>();
            GameObject start = new GameObject("Start");
            GameObject tangent = new GameObject("Tangent");
            GameObject end = new GameObject("End");

            start.transform.position = _startPos;
            tangent.transform.position = _tangent;
            end.transform.position = _endPos;

            start.transform.SetParent(obj.transform);
            tangent.transform.SetParent(obj.transform);
            end.transform.SetParent(obj.transform);

            s.Init(start, tangent, end, mf);
            MeshGenerator.Extrude(mf.mesh, new ExtrudeShape(), s, obj.transform.position);

            splineID_Dic.Add(s.ID, s);
            return s;
        }
    }
}
