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
            mr.material = Instance.StreetMat;

            Street s = obj.gameObject.AddComponent<Street>();
            GameObject start = new GameObject("Start");
            GameObject tangent1 = new GameObject("Tangent1");
            GameObject tangent2 = new GameObject("Tangent2");
            GameObject end = new GameObject("End");

            start.transform.position = _startPos;
            tangent1.transform.position = _startPos;
            tangent2.transform.position = _startPos;
            end.transform.position = _startPos;

            start.transform.SetParent(obj.transform);
            tangent1.transform.SetParent(obj.transform);
            tangent2.transform.SetParent(obj.transform);
            end.transform.SetParent(obj.transform);

            s.Init(start, tangent1, tangent2, end, 20, mf, new ExtrudeShapeBase(), true, false);

            return s;
        }

        public static void UpdatePreviewStreetEndPos(Street _street, Vector3 _endPos)
        {
            _street.m_Spline.SetEndPos(_endPos);
        }

        public static void UpdatePreviewStreetTangent1Pos(Street _street, Vector3 _tangentPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangentPos);
        }

        public static void UpdatePreviewStreetTangent2Pos(Street _street, Vector3 _tangentPos)
        {
            _street.m_Spline.SetTangent2Pos(_tangentPos);
        }

        public static Street CreateStreet(Street _street)
        {
            return CreateStreet(_street.m_Spline.StartPos, _street.m_Spline.Tangent1Pos, _street.m_Spline.Tangent2Pos, _street.m_Spline.EndPos);
        }

        public static Street CreateStreet(Vector3 _startPos, Vector3 _tangent1, Vector3 _tangent2, Vector3 _endPos)
        {
            GameObject obj = new GameObject("Street");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.StreetMat;

            Street s = obj.gameObject.AddComponent<Street>();
            GameObject start = new GameObject("Start");
            GameObject tangent1 = new GameObject("Tangent1");
            GameObject tangent2 = new GameObject("Tangent2");
            GameObject end = new GameObject("End");

            start.transform.position = _startPos;
            tangent1.transform.position = _tangent1;
            tangent2.transform.position = _tangent2;
            end.transform.position = _endPos;

            obj.tag = "Street";
            start.tag = "StreetStart";
            end.tag = "StreetEnd";

            Collider c = start.AddComponent<SphereCollider>();
            c.isTrigger = true;
            c = end.AddComponent<SphereCollider>();
            c.isTrigger = true;

            start.transform.SetParent(obj.transform);
            tangent1.transform.SetParent(obj.transform);
            tangent2.transform.SetParent(obj.transform);
            end.transform.SetParent(obj.transform);

            s.Init(start, tangent1, tangent2, end, 20, mf, new ExtrudeShapeBase());

            splineID_Dic.Add(s.ID, s);
            return s;
        }
    }
}
