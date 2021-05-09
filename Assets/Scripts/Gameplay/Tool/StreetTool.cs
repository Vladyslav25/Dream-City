using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splines;
using UnityEditor;
using System;
using Gameplay.Tools;
using Gameplay.StreetComponents;

namespace Gameplay.Streets
{
    public class StreetTool : Tools.Tool
    {
        [SerializeField]
        Material previewStreetMaterial;
        [SerializeField]
        RenderTexture m_renderTexture;

        Vector3 pos1;
        Vector3 pos2;
        Vector3 pos3;

        bool pos1Set = false;
        bool pos2Set = false;
        bool pos3Set = false;

        bool isTangent1Locked = false;
        bool isTangent2Locked = false;

        bool isCurrendToolLine = false;

        Street m_previewStreet;

        Street lastConnectedStreet;
        GameObject lastConnectedStreetChildren;

        #region -ComputeShader Member-
        [SerializeField]
        ComputeShader m_computeShader;
        ComputeBuffer m_computeBuffer;
        int[] m_pixelCount;
        int m_kernelIndex;
        int m_textureIndex;
        int m_dataBufferIndex;
        int m_widthIndex;
        #endregion

        private new void Awake()
        {
            base.Awake();
            m_computeBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Structured);
            m_pixelCount = new int[1];
            m_kernelIndex = m_computeShader.FindKernel("CSMain");
            m_textureIndex = Shader.PropertyToID("inputTexture");
            m_dataBufferIndex = Shader.PropertyToID("dataBuffer");
            m_widthIndex = Shader.PropertyToID("width");
            m_Type = TOOLTYPE.STREET;
        }

        public override void ToolUpdate()
        {
            if (Input.GetKeyUp(KeyCode.C)) //Change to Curve Tool
            {
                SetSphereColor(Color.blue);
                isCurrendToolLine = false;
            }
            if (Input.GetKeyUp(KeyCode.L)) //Change to Line Tool
            {
                SetSphereColor(Color.red);
                isCurrendToolLine = true;
            }

            if (m_validHit) //TODO: Change tto RaycastAll to ignore later Houses and other Collider Stuff
            {

                if (pos1Set == false && pos2Set == false && pos3Set == false)
                {
                    FindStreetGameObject(m_hitPos); //if no Point is set, look for not far away Street possible combinations
                }

                if (isCurrendToolLine)
                {
                    if (pos1Set == true && pos3Set == false && m_previewStreet != null) //Line: First Point Set -> Missing Sec Point
                    {
                        Vector3 pos2Tmp = m_previewStreet.m_Spline.GetCentredOrientedPoint().Position; //Get the Midpoint on the Spline
                        Vector3 tangent1 = (pos1 + pos2Tmp) * 0.5f; // 1.Tanget must be between MidPoint and Start
                        Vector3 tangent2 = (pos2Tmp + m_hitPos) * 0.5f; // 2. Tanget must be between MidPoint and End (MousePos)
                        UpdatePreview(tangent1, tangent2, m_hitPos); //Update the Preview (update if Tanget is not locked)
                        CheckForCombine(m_hitPos, false); //If a Combine is possible it Combine (overwrite Tanget Pos)
                    }
                }
                else
                {
                    if (pos1Set == true && pos2Set == false && pos3Set == false && m_previewStreet != null) // Curve: First Point Set -> Wait for Sec Point
                    {
                        Vector3 pos2Tmp = (pos1 + m_hitPos) * 0.5f; //Get the Midpoint between Start and End
                        Vector3 tangent1 = (pos1 + pos2Tmp) * 0.5f; //The 1.Tangent is between the Start and the Mid
                        Vector3 tangent2 = (pos2Tmp + m_hitPos) * 0.5f; //The 2. Tangent is between the End and the Mid
                        UpdatePreview(tangent1, tangent2, m_hitPos); //Update the Preview (update if Tanget is not locked)
                        //The Scecond Point cant Combine to an another Spline
                    }

                    if (pos1Set == true && pos2Set == true && pos3Set == false && m_previewStreet != null)
                    {
                        Vector3 tangent1 = (pos1 + pos2) * 0.5f; //The 1.Tanget is between Start and Sec Point 
                        Vector3 tangent2 = (pos2 + m_hitPos) * 0.5f; // The 2.Tanget is between Sec Point and EndPoint (MousePos)
                        UpdatePreview(tangent1, tangent2, m_hitPos); //Update the Preview (update if Tanget is not locked)
                        CheckForCombine(m_hitPos, false); //If a Combine is possible it Combine (overwrite Tanget Pos)
                    }
                }

                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    ResetTool();
                    return;
                }

                if (m_previewStreet != null)
                {
                    if (!CheckForValidForm() || CheckForCollision())
                    {
                        m_previewStreet.m_HasValidForm = false;
                        return;
                    }
                    else
                    {
                        m_previewStreet.m_HasValidForm = true;
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (pos1Set == false)
                    {
                        pos1 = m_hitPos;
                        pos1Set = true;
                        m_previewStreet = StreetComponentManager.InitStreetForPreview(pos1); //Init a Preview Street (with invalid ID)
                        CheckForCombine(m_hitPos, true); //Check if the Start in the Preview Street can be combined
                        return;
                    }

                    if (!isCurrendToolLine) // In Curve Tool -> Set Sec Pos
                    {
                        if (pos2Set == false)
                        {
                            pos2 = m_hitPos;
                            pos2Set = true;
                            return;
                        }
                    }
                    if (pos3Set == false)
                    {
                        pos3 = m_hitPos;
                        CheckForCombine(m_hitPos, false); //Check if the End in the Preview Street can be combined
                        Street newStreet = StreetComponentManager.CreateStreet(m_previewStreet); //Create a valid Street from the preview Street
                        if (newStreet.m_StartConnection == null)
                            StreetComponentManager.CreateDeadEnd(newStreet, true);
                        if (newStreet.m_EndConnection == null)
                            StreetComponentManager.CreateDeadEnd(newStreet, false);
                        //Reset
                        pos1 = Vector3.zero;
                        pos2 = Vector3.zero;
                        pos3 = Vector3.zero;
                        pos1Set = false;
                        pos2Set = false;
                        pos3Set = false;
                        isTangent1Locked = false;
                        isTangent2Locked = false;
                        Destroy(m_previewStreet.gameObject); //Destroy PreviewStreet
                        m_previewStreet = null;

                        lastConnectedStreet = null;
                        lastConnectedStreetChildren = null;
                        return;
                    }
                }
            }
        }

        public override void ToolEnd()
        {
            base.ToolEnd();
            ResetTool();
        }

        public override void ToolStart()
        {
            base.ToolStart();
            SetSphereActiv(true);
        }

        private void ResetTool()
        {
            //Reset
            pos1 = Vector3.zero;
            pos2 = Vector3.zero;
            pos3 = Vector3.zero;
            pos1Set = false;
            pos2Set = false;
            pos3Set = false;
            isTangent1Locked = false;
            isTangent2Locked = false;

            //if (m_previewStreet != null)
            //{   //Recreate DeadEnds on PreviewStreet Conections
            //    if (m_previewStreet.m_StreetConnect_Start != null)
            //    {
            //        if (m_previewStreet.m_StreetConnect_Start.m_StreetConnect_Start == m_previewStreet)
            //            m_previewStreet.m_StreetConnect_Start.CreateDeadEnd(true);
            //        if (m_previewStreet.m_StreetConnect_Start.m_StreetConnect_End == m_previewStreet)
            //            m_previewStreet.m_StreetConnect_Start.CreateDeadEnd(false);
            //    }
            //    if (m_previewStreet.m_StreetConnect_End != null)
            //    {
            //        if (m_previewStreet.m_StreetConnect_End.m_StreetConnect_Start == m_previewStreet)
            //            m_previewStreet.m_StreetConnect_End.CreateDeadEnd(true);
            //        if (m_previewStreet.m_StreetConnect_End.m_StreetConnect_End == m_previewStreet)
            //            m_previewStreet.m_StreetConnect_End.CreateDeadEnd(false);
            //    }
            //    if (m_previewStreet.GetCollisionStreet().gameObject != null)
            //        Destroy(m_previewStreet.GetCollisionStreet().gameObject);
            //    if (m_previewStreet.gameObject != null)
            //        Destroy(m_previewStreet.gameObject); //Destroy PreviewStreet
            //}
            //
            //if (lastConnectedStreetChildren != null)
            //    if (lastConnectedStreetChildren.CompareTag("StreetStart"))
            //        lastConnectedStreet.CreateDeadEnd(true);
            //    else if (lastConnectedStreetChildren.CompareTag("StreetEnd"))
            //        lastConnectedStreet.CreateDeadEnd(false);

            lastConnectedStreet = null;
            lastConnectedStreetChildren = null;

            m_previewStreet = null;
            return;
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
            if (m_pixelCount[0] > 3)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update the PreviewStreet
        /// </summary>
        /// <param name="_tangent1">The new Pos of the 1.Tangent</param>
        /// <param name="_tangent2">The new Pos of the 2.Tangent</param>
        /// <param name="_endPos">The new Pos of the EndPoint</param>
        private void UpdatePreview(Vector3 _tangent1, Vector3 _tangent2, Vector3 _endPos)
        {
            if (!isTangent1Locked)
                StreetComponentManager.UpdateStreetTangent1Pos(m_previewStreet, _tangent1);
            if (!isTangent2Locked)
                StreetComponentManager.UpdateStreetTangent2Pos(m_previewStreet, _tangent2);
            StreetComponentManager.UpdateStreetEndPos(m_previewStreet, _endPos);
        }

        /// <summary>
        /// Look for the closest Street End or Start GameObject
        /// </summary>
        /// <param name="_hitPoint">The Pos from where to look</param>
        /// <returns>The GameObject closest from the Pos (null if its too far away)</returns>
        private GameObject FindStreetGameObject(Vector3 _hitPoint)
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

            GameObject closestStreetChildren = GetClosesedGameObject(hittedStreetsChildern, _hitPoint); //Look for the closest GameObject of all valid GameObjects
            SetSpherePos(closestStreetChildren.transform.position); //Set the Sphere to the Pos of the closest valid Street GameObject
            return closestStreetChildren;
        }

        /// <summary>
        /// Check on the given Pos if a combine to another Street is possible
        /// </summary>
        /// <param name="_hitPoint">The Pos to check around</param>
        /// <param name="isStart">Is it the Start of the PreviewStreet</param>
        /// <returns></returns>
        private void CheckForCombine(Vector3 _hitPoint, bool isStart)
        {
            GameObject closestStreetChildren = FindStreetGameObject(_hitPoint);

            if (closestStreetChildren == null) // if there is no valid Street GameObject around
            {
                //TODO: Recreate Dead End

                //Unlock the Tangent of the Pos which is not Set
                if (pos1 == Vector3.zero)
                    isTangent1Locked = false;
                if (pos3 == Vector3.zero)
                    isTangent2Locked = false;
                return;
            }

            Street otherStreet = closestStreetChildren.GetComponentInParent<Street>();

            if (closestStreetChildren.CompareTag("StreetEnd"))
            {
                if (isStart)
                {
                    //other Street End and previewStreet Start -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent2Pos - otherStreet.m_Spline.EndPos).normalized; //Get the point symmetrical Direction

                    StreetComponentManager.UpdateStreetTangent1AndStartPoint(
                        m_previewStreet,
                        dir * 5f + otherStreet.m_Spline.EndPos, //Set the Tanget to the half point symmetrical Position
                                    otherStreet.m_Spline.EndPos
                                    ); //Set the EndPos

                    isTangent1Locked = true;

                    //Combine(_others End, preview Start) + Remove others DeadEnd at End
                    if (otherStreet.m_EndConnection.m_OtherComponent is DeadEnd)
                    {
                        StreetComponentManager.DestroyDeadEnd((DeadEnd)otherStreet.m_EndConnection.m_OtherComponent);
                        Debug.Log("Remove DeadEnd");
                    }
                    CombinePreview(otherStreet, false, true);

                    return;
                }
                else
                {
                    //other Street End and previewStreet End -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent2Pos - otherStreet.m_Spline.EndPos).normalized;

                    StreetComponentManager.UpdateStreetTangent2AndEndPoint(
                        m_previewStreet,
                        dir * 5f + otherStreet.m_Spline.EndPos,
                                            otherStreet.m_Spline.EndPos
                                            );

                    isTangent2Locked = true;

                    //Combine(_others End, preview End) + Remove others DeadEnd at End
                    if (otherStreet.m_EndConnection.m_OtherComponent is DeadEnd)
                    {
                        StreetComponentManager.DestroyDeadEnd((DeadEnd)otherStreet.m_EndConnection.m_OtherComponent);
                        Debug.Log("Remove DeadEnd");
                    }
                    CombinePreview(otherStreet, false, false);

                    return;
                }
            }
            else if (closestStreetChildren.CompareTag("StreetStart"))
            {
                if (isStart)
                {
                    //other Street Start and previewStreet Start -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent1Pos - otherStreet.m_Spline.StartPos).normalized;

                    StreetComponentManager.UpdateStreetTangent1AndStartPoint(
                        m_previewStreet,
                        dir * 5f + otherStreet.m_Spline.StartPos,
                                            otherStreet.m_Spline.StartPos
                                            );

                    isTangent1Locked = true;

                    //Combine(_others Start, preview Start) + Remove others DeadEnd at Start
                    if (otherStreet.m_StartConnection.m_OtherComponent is DeadEnd)
                    {
                        StreetComponentManager.DestroyDeadEnd((DeadEnd)otherStreet.m_StartConnection.m_OtherComponent);
                        Debug.Log("Remove DeadEnd");
                    }
                    CombinePreview(otherStreet, true, true);

                    return;
                }
                else
                {
                    //other Street Start and previewStreet End -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent1Pos - otherStreet.m_Spline.StartPos).normalized;

                    StreetComponentManager.UpdateStreetTangent2AndEndPoint(
                        m_previewStreet,
                        dir * 5f + otherStreet.m_Spline.StartPos,
                                            otherStreet.m_Spline.StartPos
                                            );

                    isTangent2Locked = true;

                    //Combine(_others Start, preview Start) + Remove others DeadEnd at Start
                    if (otherStreet.m_StartConnection.m_OtherComponent is DeadEnd)
                    {
                        StreetComponentManager.DestroyDeadEnd((DeadEnd)otherStreet.m_StartConnection.m_OtherComponent);
                        Debug.Log("Remove DeadEnd");
                    }
                    CombinePreview(otherStreet, true, false);

                    return;
                }
            }
        }

        private void CombinePreview(Street _other, bool _otherStart, bool _previewStart)
        {
            if (_previewStart && _otherStart)
            {
                Connection.Combine(m_previewStreet.m_StartConnection, _other.m_StartConnection);
                return;
            }
            if(_previewStart && !_otherStart)
            {
                Connection.Combine(m_previewStreet.m_StartConnection, _other.m_EndConnection);
                return;
            }
            if (!_previewStart && _otherStart)
            {
                Connection.Combine(m_previewStreet.m_EndConnection, _other.m_StartConnection);
                return;
            }
            if (!_previewStart && !_otherStart)
            {
                Connection.Combine(m_previewStreet.m_EndConnection, _other.m_EndConnection);
                return;
            }
        }      

        /// <summary>
        /// Run the Check if the Form of the Street is Valid
        /// </summary>
        /// <returns></returns>
        private bool CheckForValidForm()
        {
            if (m_previewStreet == null) return false;
            float validDistanceStartEnd = 2f;
            float maxDeltaAngel = 18f;

            Vector3 StartPos = m_previewStreet.m_Spline.StartPos;
            Vector3 EndPos = m_previewStreet.m_Spline.EndPos;

            if (Vector3.Distance(StartPos, EndPos) < validDistanceStartEnd)
            {
                return false;
            }

            for (int i = 0; i < m_previewStreet.m_Spline.OPs.Length - 1; i++)
            {
                Vector3 opTangent1 = m_previewStreet.m_Spline.GetTangentAt(m_previewStreet.m_Spline.OPs[i].t);
                Vector3 opTangent2 = m_previewStreet.m_Spline.GetTangentAt(m_previewStreet.m_Spline.OPs[i + 1].t);
                if (Vector3.Angle(opTangent1, opTangent2) > maxDeltaAngel)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the closest GameObject from the given List
        /// </summary>
        /// <param name="_objs">The List of GameObject</param>
        /// <param name="_point">The Point from where to look</param>
        /// <returns>The closest GameObject to the Point</returns>
        private GameObject GetClosesedGameObject(List<GameObject> _objs, Vector3 _point)
        {
            if (_objs.Count == 1) return _objs[0];

            float shortestDistance = float.MaxValue;
            GameObject output = null;

            for (int i = 0; i < _objs.Count; i++)
            {
                float distance = Vector3.Distance(_objs[i].transform.position, _point);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    output = _objs[i];
                }
            }
            return output;
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