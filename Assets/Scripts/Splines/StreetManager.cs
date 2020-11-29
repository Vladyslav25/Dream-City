using MeshGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Streets;

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
        private Material streetMat;
        [SerializeField]
        private Material previewStreetMat;
        public Material DeadEndMat;

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

        public static void SetStreetColor(Street _street, Color _newColor)
        {
            if (_street.m_MeshRendererRef.material.color != _newColor)
                _street.m_MeshRendererRef.material.color = _newColor;
        }

        public static Street InitStreetForPreview(Vector3 _startPos)
        {
            GameObject obj = new GameObject("Street");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.previewStreetMat;

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

            s.Init(start, tangent1, tangent2, end, 20, mf, mr, new StreetShape(), true, false);

            return s;
        }

        #region -Update Tangents, Start and End-
        public static void UpdateStreetStartPos(Street _street, Vector3 _startPos)
        {
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetEndPos(Street _street, Vector3 _endPos)
        {
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangent1Pos(Street _street, Vector3 _tangentPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangentPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangent2Pos(Street _street, Vector3 _tangentPos)
        {
            _street.m_Spline.SetTangent2Pos(_tangentPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangents(Street _street, Vector3 _tangent1Pos, Vector3 _tangent2Pos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangentsAndEndPoint(Street _street, Vector3 _tangent1Pos, Vector3 _tangent2Pos, Vector3 _endPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangentsAndStartPoint(Street _street, Vector3 _tangent1Pos, Vector3 _tangent2Pos, Vector3 _startPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangent1AndEndPoint(Street _street, Vector3 _tangent1Pos, Vector3 _endPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangent2AndEndPoint(Street _street, Vector3 _tangent2Pos, Vector3 _endPos)
        {
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangent1AndStartPoint(Street _street, Vector3 _tangent1Pos, Vector3 _startPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs();
        }

        public static void UpdateStreetTangent2AndStartPoint(Street _street, Vector3 _tangent2Pos, Vector3 _startPos)
        {
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs();
        }
        #endregion

        public static Street CreateStreet(Street _street)
        {
            return CreateStreet(_street.m_Spline.StartPos, _street.m_Spline.Tangent1Pos, _street.m_Spline.Tangent2Pos, _street.m_Spline.EndPos, _street.m_StreetConnect_Start, _street.m_StreetConnect_End);
        }

        public static Street CreateStreet(Vector3 _startPos, Vector3 _tangent1, Vector3 _tangent2, Vector3 _endPos, Street _connectStart, Street _connectEnd)
        {
            GameObject obj = new GameObject("Street");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.streetMat;

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
            bool connectStartIsOtherStart = true; 
            bool connectEndIsOtherStart = true;

            if (_connectStart != null)
            {
                if(_connectStart.m_StreetConnect_Start.ID == -1)
                    connectStartIsOtherStart = true;
                else if (_connectStart.m_StreetConnect_End.ID == -1)
                    connectStartIsOtherStart = false;
            }
            if(_connectEnd != null)
            {
                if(_connectEnd.m_StreetConnect_Start.ID == -1)
                        connectEndIsOtherStart = true;
                else if (_connectEnd.m_StreetConnect_End.ID == -1)
                        connectEndIsOtherStart = false;
            }

            s.Init(start, tangent1, tangent2, end, 20, mf, mr, new StreetShape(), false, true, _connectStart, connectStartIsOtherStart, _connectEnd, connectEndIsOtherStart);
            s.m_Spline.CreateGridOPs();
            splineID_Dic.Add(s.ID, s);
            return s;
        }
    }
}
