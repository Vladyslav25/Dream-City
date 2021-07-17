using Gameplay.StreetComponents;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Gameplay.Tools
{
    public class CrossTool : ATool
    {
        [SerializeField]
        private GameObject m_crossPrefab;
        [SerializeField]
        private GameObject m_crossCollPrefab;
        private GameObject m_crossColl;
        private Cross m_cross;
        private MeshRenderer m_renderPreview;
        private GameObject m_crossObj;
        private Quaternion m_rotation;

        #region -ComputeShader Member-
        [SerializeField]
        RenderTexture m_renderTexture;
        [SerializeField]
        ComputeShader m_computeShader;
        ComputeBuffer m_computeBuffer;
        int[] m_pixelCount;
        int m_kernelIndex;
        int m_textureIndex;
        int m_dataBufferIndex;
        int m_widthIndex;
        #endregion

        private void Awake()
        {
            m_computeBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Structured);
            m_pixelCount = new int[1];
            m_kernelIndex = m_computeShader.FindKernel("CSMain");
            m_textureIndex = Shader.PropertyToID("inputTexture");
            m_dataBufferIndex = Shader.PropertyToID("dataBuffer");
            m_widthIndex = Shader.PropertyToID("width");
        }

        public override void ToolStart()
        {
            Cursor.SetActiv(false);
            m_rotation = Quaternion.identity;
            SpawnCross();
            UIManager.Instance.DeactivateUI();
        }

        public override void ToolEnd()
        {
            RemoveStartConnection();
            if (m_crossObj != null)
                Destroy(m_crossObj);
            if (m_crossColl != null)
                Destroy(m_crossColl);
        }

        public override void ToolUpdate()
        {
            Connection closestStreetConnection = FindStreetConnection(m_hitPos);
            if (closestStreetConnection != null)
                ConnectCrossToStreet((Street)closestStreetConnection.m_Owner, closestStreetConnection.m_OwnerStart);
            else
            {
                RemoveStartConnection();
                m_crossObj.transform.position = m_hitPos;
                m_crossObj.transform.rotation = m_rotation;
            }

            m_crossColl.transform.position = m_crossObj.transform.position;
            m_crossColl.transform.rotation = m_crossObj.transform.rotation;

            if (Input.GetKey(KeyCode.Comma))
            {
                m_crossObj.transform.Rotate(new Vector3(0, -0.5f, 0));
                m_rotation = m_crossObj.transform.rotation;
            }
            if (Input.GetKey(KeyCode.Period))
            {
                m_crossObj.transform.Rotate(new Vector3(0, 0.5f, 0));
                m_rotation = m_crossObj.transform.rotation;
            }

            if (CheckForCollision())
            {
                m_cross.m_IsInvalid = false;
                return;
            }
            else
                m_cross.m_IsInvalid = true;

            if (Input.GetMouseButtonDown(0))
            {
                PlacePreview();
                SpawnCross(); //Spawn new Preview Cross + Coll Cross
            }
        }

        private void PlacePreview()
        {
            m_crossColl.GetComponent<MeshRenderer>().material = StreetComponentManager.Instance.streetMatColl;
            m_crossObj.GetComponent<MeshRenderer>().material.color = Color.white;
            m_cross.SetOP();
            m_cross.CreateDeadEnds();
            m_cross.SetID();
            m_cross.CheckGridCollision();
            StreetComponentManager.AddCross(m_cross);
        }

        private void RemoveStartConnection()
        {
            Connection otherConnection = m_cross.GetStartConnection().m_OtherConnection;
            if (otherConnection == null) return;
            Connection.DeCombine(m_cross.GetStartConnection(), otherConnection);
            StreetComponentManager.CreateDeadEnd((Street)otherConnection.m_Owner, otherConnection.m_OwnerStart);
        }

        private void SetCrossTransform(Vector3 _pos, Quaternion _rot)
        {
            m_crossObj.transform.position = _pos;
            m_crossObj.transform.rotation = _rot;
        }

        private void ConnectCrossToStreet(Street _street, bool _isStreetStart)
        {
            if (m_cross.GetStartConnection().m_OtherComponent == null)
                if (_isStreetStart)
                {
                    OrientedPoint op = _street.m_Spline.GetFirstOrientedPoint();
                    SetCrossTransform(op.Position - op.LocalToWorldDirection(Vector3.forward) * 1.2f, op.Rotation * Quaternion.Euler(0, 180f, 0));
                    StreetComponentManager.DestroyDeadEnd((DeadEnd)_street.GetStartConnection().m_OtherComponent);
                    Connection.Combine(m_cross.GetStartConnection(), _street.GetStartConnection());
                }
                else
                {
                    OrientedPoint op = _street.m_Spline.GetLastOrientedPoint();
                    SetCrossTransform(op.Position + op.LocalToWorldDirection(Vector3.forward) * 1.2f, op.Rotation);
                    StreetComponentManager.DestroyDeadEnd((DeadEnd)_street.m_EndConnection.m_OtherComponent);
                    Connection.Combine(m_cross.GetStartConnection(), _street.m_EndConnection);
                }

        }

        private GameObject SpawnCross()
        {
            //Spawn Visible Cross
            GameObject obj = Instantiate(m_crossPrefab, m_hitPos, m_rotation, StreetComponentManager.Instance.transform);
            m_cross = obj.GetComponent<Cross>();
            m_cross.Init(null, false);
            m_renderPreview = obj.GetComponent<MeshRenderer>();
            m_crossObj = obj;
            SpawnCollCross();
            return obj;
        }

        private GameObject SpawnCollCross()
        {
            //Collider Cross
            GameObject obj = Instantiate(m_crossPrefab, m_crossObj.transform.position, m_crossObj.transform.rotation, StreetComponentManager.Instance.StreetCollisionParent.transform);
            obj.layer = 8;
            obj.name = "Cross_Col";

            //Remove Collision Sphere Collider from Prefab
            Collider[] coll = obj.GetComponents<Collider>();
            foreach (Collider co in coll)
                Destroy(co);

            Destroy(obj.GetComponent<Cross>());

            //Set Coll Material
            MeshRenderer mr = obj.gameObject.GetComponent<MeshRenderer>();
            mr.material = StreetComponentManager.Instance.previewStreetMatColl;

            m_crossColl = obj;
            return obj;
        }

        private Connection FindStreetConnection(Vector3 _hitPoint)
        {
            Collider[] sphereHits = Physics.OverlapSphere(_hitPoint, 2); //Cast a Sphere on the give Pos
            List<GameObject> hittedStreetsChildern = new List<GameObject>();

            for (int i = 0; i < sphereHits.Length; i++) //Look if the Sphere overlap an valid Street GameObject
            {
                Street tmpStreet = sphereHits[i].GetComponentInParent<Street>();
                if (tmpStreet == null) continue;
                if ((sphereHits[i].CompareTag("StreetEnd") && tmpStreet.m_EndIsConnectable)
                    || (sphereHits[i].CompareTag("StreetStart") && tmpStreet.m_StartIsConnectable))
                    hittedStreetsChildern.Add(sphereHits[i].transform.gameObject); //if found a valid GameObject add it to a List

                Cross tmpCross = sphereHits[i].GetComponent<Cross>();
                if (tmpCross == null) continue;
                hittedStreetsChildern.Add(sphereHits[i].transform.gameObject);
            }
            if (hittedStreetsChildern.Count == 0) return null; //return null if no valid Street GameObbject was found in the Sphere

            GameObject closestStreetChildren = GetClosesetGameObject(hittedStreetsChildern, _hitPoint); //Look for the closest GameObject of all valid GameObjects
            Street s = closestStreetChildren.GetComponentInParent<Street>();
            if (closestStreetChildren.CompareTag("StreetEnd"))
            {
                return s.m_EndConnection;
            }
            else if (closestStreetChildren.CompareTag("StreetStart"))
            {
                return s.GetStartConnection();
            }
            return null;
        }

        /// <summary>
        /// Run the Collision Check beteween Preview Street and others
        /// </summary>
        /// <returns></returns>
        private bool CheckForCollision()
        {
            m_computeBuffer.SetData(new int[1]);

            m_computeShader.SetTexture(m_kernelIndex, m_textureIndex, m_renderTexture);
            m_computeShader.SetBuffer(m_kernelIndex, m_dataBufferIndex, m_computeBuffer);
            m_computeShader.SetInt(m_widthIndex, m_renderTexture.width);
            m_computeShader.Dispatch(m_kernelIndex, m_renderTexture.width / 32, m_renderTexture.height / 32, 1);

            m_computeBuffer.GetData(m_pixelCount);
            if (m_pixelCount[0] > 5)
            {
                return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            m_computeBuffer.Dispose();
        }

        private void OnApplicationQuit()
        {
            m_computeBuffer.Dispose();
        }
    }
}
