using MeshGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid;
using Unity.Jobs;
using TMPro;

namespace Gameplay.StreetComponents
{
    public class StreetComponentManager : MonoBehaviour
    {
        #region -SingeltonPattern-
        private static StreetComponentManager _instance;
        public static StreetComponentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<StreetComponentManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("SplineManager");
                        _instance = container.AddComponent<StreetComponentManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        [SerializeField]
        private Material streetMat;

        public Material streetMatColl;

        [SerializeField]
        private Material previewStreetMat;

        [SerializeField]
        private Material previewStreetMatColl;

        public Material DeadEndMat;

        public GameObject StreetCollisionParent;

        private static Dictionary<int, Street> StreetComponentID_Dic = new Dictionary<int, Street>(); //Contains all Streets by ID

        private static int setComponentId;

        public static Street GetStreetByID(int _id)
        {
            if (StreetComponentID_Dic.ContainsKey(_id))
            {
                return StreetComponentID_Dic[_id];
            }
            Debug.LogError($"No StreetComponent with ID: {_id} found");
            return null;
        }

        public static int GetNewStreetComponentID()
        {
            setComponentId++;
            return setComponentId;
        }

        public static void SetStreetColor(Street _street, Color _newColor)
        {
            if (_street.m_MeshRenderer.material.color != _newColor)
                _street.m_MeshRenderer.material.color = _newColor;
        }

        /// <summary>
        /// Initialize / Create a Street as Preview 
        /// </summary>
        /// <param name="_startPos">The World Location of the starting Point</param>
        /// <returns>The new Preview Street</returns>
        public static Street InitStreetForPreview(Vector3 _startPos)
        {
            GameObject obj = new GameObject("Street:Preview");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.previewStreetMat;

            Street s = obj.gameObject.AddComponent<Street>();
            Vector3[] pos = new Vector3[] { _startPos, _startPos, _startPos, _startPos };

            s.Init(pos, null, null, 20, false, true);

            s.SetCollisionStreet(InitStreetForPreviewColl(_startPos));
            return s;
        }

        /// <summary>
        /// Initialize / Create a CollisionStreet for the Preview 
        /// </summary>
        /// <param name="_startPos"></param>
        /// <returns></returns>
        public static Street InitStreetForPreviewColl(Vector3 _startPos)
        {
            GameObject obj = new GameObject("StreetColl_Preview");
            obj.transform.position = _startPos;
            obj.layer = 8;
            obj.transform.SetParent(Instance.StreetCollisionParent.transform);

            obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.previewStreetMatColl;

            Street s = obj.gameObject.AddComponent<Street>();
            Vector3[] pos = new Vector3[] { _startPos, _startPos, _startPos, _startPos };

            s.Init(pos, null, null, 20, false, true);

            return s;
        }

        /// <summary>
        /// Create a Dead End and Combine the Street with it
        /// </summary>
        /// <param name="_street">The Street the Dead End is Connected to</param>
        /// <param name="_isStart">Is it the Start of the Street</param>
        /// <returns>The new Dead End</returns>
        public static DeadEnd CreateDeadEnd(Street _street, bool _isStart)
        {
            GameObject obj = new GameObject("DeadEnd");
            DeadEnd de = obj.AddComponent<DeadEnd>();

            OrientedPoint op;
            if (_isStart) op = _street.m_Spline.GetFirstOrientedPoint();
            else op = _street.m_Spline.GetLastOrientedPoint();
            de.Init(_street, _isStart, op);

            if (_isStart) Connection.Combine(_street.m_StartConnection, de.m_StartConnection);
            else Connection.Combine(_street.m_EndConnection, de.m_StartConnection);

            return de;
        }

        /// <summary>
        /// Decombine and Destroy the Dead End
        /// </summary>
        /// <param name="_de">The Dead End to Destroy</param>
        public static void DestroyDeadEnd(DeadEnd _de)
        {
            Connection.DeCombine(_de.m_StartConnection, _de.m_StartConnection.m_OtherConnection);
            Destroy(_de.gameObject);
        }

        #region -Update Tangents, Start and End-
        public static void UpdateStreetStartPos(Street _street, Vector3 _startPos)
        {
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs(_street);

            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetStartPos(_startPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetEndPos(Street _street, Vector3 _endPos)
        {
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs(_street);

            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetEndPos(_endPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangent1Pos(Street _street, Vector3 _tangent1Pos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent1Pos(_tangent1Pos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangent2Pos(Street _street, Vector3 _tangent2Pos)
        {
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent2Pos(_tangent2Pos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangents(Street _street, Vector3 _tangent1Pos, Vector3 _tangent2Pos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent1Pos(_tangent1Pos);
                _street.GetCollisionStreet().m_Spline.SetTangent2Pos(_tangent2Pos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangentsAndEndPoint(Street _street, Vector3 _tangent1Pos, Vector3 _tangent2Pos, Vector3 _endPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent1Pos(_tangent1Pos);
                _street.GetCollisionStreet().m_Spline.SetTangent2Pos(_tangent2Pos);
                _street.GetCollisionStreet().m_Spline.SetEndPos(_endPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangentsAndStartPoint(Street _street, Vector3 _tangent1Pos, Vector3 _tangent2Pos, Vector3 _startPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent1Pos(_tangent1Pos);
                _street.GetCollisionStreet().m_Spline.SetTangent2Pos(_tangent2Pos);
                _street.GetCollisionStreet().m_Spline.SetStartPos(_startPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangent1AndEndPoint(Street _street, Vector3 _tangent1Pos, Vector3 _endPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent1Pos(_tangent1Pos);
                _street.GetCollisionStreet().m_Spline.SetEndPos(_endPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangent2AndEndPoint(Street _street, Vector3 _tangent2Pos, Vector3 _endPos)
        {
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetEndPos(_endPos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent2Pos(_tangent2Pos);
                _street.GetCollisionStreet().m_Spline.SetEndPos(_endPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangent1AndStartPoint(Street _street, Vector3 _tangent1Pos, Vector3 _startPos)
        {
            _street.m_Spline.SetTangent1Pos(_tangent1Pos);
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent1Pos(_tangent1Pos);
                _street.GetCollisionStreet().m_Spline.SetStartPos(_startPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }

        public static void UpdateStreetTangent2AndStartPoint(Street _street, Vector3 _tangent2Pos, Vector3 _startPos)
        {
            _street.m_Spline.SetTangent2Pos(_tangent2Pos);
            _street.m_Spline.SetStartPos(_startPos);
            _street.m_Spline.UpdateOPs(_street);
            if (_street.GetCollisionStreet() != null)
            {
                _street.GetCollisionStreet().m_Spline.SetTangent2Pos(_tangent2Pos);
                _street.GetCollisionStreet().m_Spline.SetStartPos(_startPos);
                _street.GetCollisionStreet().m_Spline.UpdateOPs(_street);
            }
        }
        #endregion

        private static Street CreateCollisionStreet(Vector3 _startPos, Vector3 _tangent1, Vector3 _tangent2, Vector3 _endPos, Connection _startConnection, Connection _endConnection)
        {
            GameObject obj = new GameObject("Street_Col");
            obj.transform.position = _startPos;
            obj.layer = 8;
            obj.transform.SetParent(Instance.StreetCollisionParent.transform);

            MeshFilter mf = obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.streetMatColl;

            Street s = obj.gameObject.AddComponent<Street>();
            Vector3[] pos = new Vector3[] { _startPos, _tangent1, _tangent2, _endPos };

            s.Init(pos, null, null, 20, false);
            return s;
        }

        /// <summary>
        /// Create a finshed Street
        /// </summary>
        /// <param name="_street">The Preview Street</param>
        /// <returns>Return the new finished Street</returns>
        public static Street CreateStreet(Street _street)
        {
            CreateCollisionStreet(_street.m_Spline.StartPos, _street.m_Spline.Tangent1Pos, _street.m_Spline.Tangent2Pos, _street.m_Spline.EndPos, _street.m_StartConnection, _street.m_EndConnection);
            Destroy(_street.GetCollisionStreet().gameObject);
            return CreateStreet(_street.m_Spline.StartPos, _street.m_Spline.Tangent1Pos, _street.m_Spline.Tangent2Pos, _street.m_Spline.EndPos, _street.m_StartConnection, _street.m_EndConnection);
        }

        private static Street CreateStreet(Vector3 _startPos, Vector3 _tangent1, Vector3 _tangent2, Vector3 _endPos, Connection _startConn, Connection _endConn)
        {
            GameObject obj = new GameObject("Street");
            obj.transform.position = _startPos;
            obj.transform.SetParent(Instance.transform);

            obj.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.gameObject.AddComponent<MeshRenderer>();
            mr.material = Instance.streetMat;

            Street s = obj.gameObject.AddComponent<Street>();

            GameObject start = new GameObject("Start");
            GameObject end = new GameObject("End");

            start.transform.position = _startPos;
            end.transform.position = _endPos;

            obj.tag = "Street";
            start.tag = "StreetStart";
            end.tag = "StreetEnd";

            Collider c = start.AddComponent<SphereCollider>();
            c.isTrigger = true;
            c = end.AddComponent<SphereCollider>();
            c.isTrigger = true;

            start.transform.SetParent(obj.transform);
            end.transform.SetParent(obj.transform);

            Vector3[] pos = new Vector3[] { _startPos, _tangent1, _tangent2, _endPos };

            s.Init(pos, _startConn, _endConn, 20);
            s.m_Spline.CreateGridOPs();
            s.CheckCollision();
            GridManager.CreateGrid(s);

            StreetComponentID_Dic.Add(s.ID, s);
            return s;
        }
    }
}
