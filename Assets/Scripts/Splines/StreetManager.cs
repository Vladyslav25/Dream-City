using MeshGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    public class StreetManager : MonoBehaviour
    {
        #region -SingeltonPattern-
        private static StreetManager _instance;
        public static StreetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<StreetManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("SplineManager");
                        _instance = container.AddComponent<StreetManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        [SerializeField]
        private Material StreetMat;
        [SerializeField]
        private Material SidewalkMat;

        private static Dictionary<int, Street> splineID_Dic = new Dictionary<int, Street>();
        private static int setSplineId;

        public static Street GetStreetByID(int _id)
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

        public static Street InitStreetForPreview(Vector3 _startPos)
        {
            GameObject obj = new GameObject("Street");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.materials = new Material[]
            {
                Instance.StreetMat
            };

            Street s = obj.gameObject.AddComponent<Street>();
            GameObject start = new GameObject("Start");
            GameObject tangent = new GameObject("Tangent");
            GameObject end = new GameObject("End");

            start.transform.position = _startPos;
            tangent.transform.position = _startPos;
            end.transform.position = _startPos;

            start.transform.SetParent(obj.transform);
            tangent.transform.SetParent(obj.transform);
            end.transform.SetParent(obj.transform);

            s.Init(
               start, tangent, end, 3,
                mf,
                new ExtrudeShapeBase[]
                {
                    new StreetShapeComplete()
                });

            return s;
        }

        public static void UpdatePreviewStreetEndPos(Street _street, Vector3 _endPos)
        {
            _street.m_Spline.SetEndPos(_endPos);
        }

        public static void UpdatePreviewStreetTangentPos(Street _street, Vector3 _tangentPos)
        {
            _street.m_Spline.SetTangentPos(_tangentPos);
        }

        public static Street CreateStreet(Vector3 _startPos, Vector3 _tangent, Vector3 _endPos)
        {
            GameObject obj = new GameObject("Street");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.materials = new Material[]
            {
                Instance.StreetMat
            };

            Street s = obj.gameObject.AddComponent<Street>();
            GameObject start = new GameObject("Start");
            GameObject tangent = new GameObject("Tangent");
            GameObject end = new GameObject("End");

            start.transform.position = _startPos;
            tangent.transform.position = _tangent;
            end.transform.position = _endPos;

            start.transform.SetParent(obj.transform);
            tangent.transform.SetParent(obj.transform);
            end.transform.SetParent(obj.transform);

            s.Init(
               start, tangent, end, 3,
                mf,
                new ExtrudeShapeBase[]
                {
                    new StreetShapeComplete()
                });

            splineID_Dic.Add(s.ID, s);
            return s;
        }
    }
}
