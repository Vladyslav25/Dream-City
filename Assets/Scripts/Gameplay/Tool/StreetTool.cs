using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splines;
using UnityEditor;
using System;
using Gameplay.Tools;
using Gameplay.StreetComponents;
using UnityEditor.IMGUI.Controls;
using UI;

namespace Gameplay.Streets
{
    public class StreetTool : Tools.Tool
    {
        [SerializeField]
        Material previewStreetMaterial;

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
        Connection m_previewLastConnected;

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
            m_Type = TOOLTYPE.STREET;
        }

        public override void ToolUpdate()
        {
            if (Input.GetKeyUp(KeyCode.C)) //Change to Curve Tool
            {
                SetCurveLineTool(false);
            }
            else if (Input.GetKeyUp(KeyCode.L)) //Change to Line Tool
            {
                SetCurveLineTool(true);
            }

            if (m_validHit) //TODO: Change tto RaycastAll to ignore later Houses and other Collider Stuff
            {

                if (pos1Set == false && pos2Set == false && pos3Set == false)
                {
                    FindClosestConnection(m_hitPos); //if no Point is set, look for not far away Street possible combinations
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
                    if (m_previewStreet != null)
                    {
                        DecombinePreview(true);
                        DecombinePreview(false);
                    }
                    ResetTool();
                    return;
                }

                if (m_previewStreet != null)
                {
                    if (!CheckForValidForm() || CheckForCollision())
                    {
                        m_previewStreet.m_IsInvalid = false;
                        return;
                    }
                    else
                    {
                        m_previewStreet.m_IsInvalid = true;
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

                        //Reset
                        ResetTool();
                        return;
                    }
                }
            }
        }

        public override void ToolEnd()
        {
            ResetTool();
            Cursor.SetActiv(false);
        }

        public override void ToolStart()
        {
            Cursor.SetActiv(true);
            Cursor.SetColor(Color.blue);
            isCurrendToolLine = false;
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
            Destroy(m_previewStreet?.gameObject); //Destroy PreviewStreet
            Destroy(m_previewStreet?.GetCollisionStreet()?.gameObject); //Destroy CollStreet
            m_previewStreet = null;
        }

        public void SetCurveLineTool(bool _setLine)
        {
            if (_setLine)
            {
                Cursor.SetColor(Color.red);
                isCurrendToolLine = true;
            }
            else
            {
                Cursor.SetColor(Color.blue);
                isCurrendToolLine = false;
            }
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
        private Connection FindClosestConnection(Vector3 _hitPoint)
        {
            Collider[] colls = Physics.OverlapSphere(_hitPoint, 2); //Cast a Sphere on the give Pos
            List<SphereCollider> validCollider = new List<SphereCollider>();
            List<SphereCollider> sphereColls = new List<SphereCollider>();
            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i] is SphereCollider)
                    sphereColls.Add((SphereCollider)colls[i]);
            }

            Street tmpStreet;
            Cross tmpCross;

            //look for all collider if there are valid for connection
            for (int i = 0; i < sphereColls.Count; i++)
            {
                tmpStreet = sphereColls[i].GetComponentInParent<Street>();
                if (tmpStreet != null && ((sphereColls[i].CompareTag("StreetEnd") && tmpStreet.m_EndIsConnectable)
                    || (sphereColls[i].CompareTag("StreetStart") && tmpStreet.m_StartIsConnectable)))
                    validCollider.Add(sphereColls[i]); //if found a valid GameObject add it to a List

                tmpCross = sphereColls[i].GetComponent<Cross>();
                if (tmpCross != null && tmpCross.IsConnectabel(sphereColls[i]))
                    validCollider.Add(sphereColls[i]);
            }

            if (validCollider.Count == 0) return null; //return null if no valid Collider was found in the Range

            SphereCollider closestCollider = GetClosesetCollider(validCollider, _hitPoint); //Look for the closest GameObject of all valid GameObjects
            Cursor.SetPosition(closestCollider.transform.position + closestCollider.transform.rotation * closestCollider.center); //Set the Sphere to the Pos of the closest valid Street GameObject
            tmpStreet = closestCollider.GetComponentInParent<Street>();
            tmpCross = closestCollider.GetComponentInParent<Cross>();
            Connection conn = null;
            if (tmpStreet != null)
            {
                if (closestCollider.CompareTag("StreetStart"))
                    conn = tmpStreet.GetStartConnection();
                else if (closestCollider.CompareTag("StreetEnd"))
                    conn = tmpStreet.m_EndConnection;
            }
            else if (tmpCross != null)
            {
                Vector3 collCenter = closestCollider.center;
                if (collCenter.x == 0 && collCenter.z == -1.2f)
                    conn = tmpCross.GetConnectionByIndex(0);
                else
                if (collCenter.x == -1.2f && collCenter.z == 0)
                    conn = tmpCross.GetConnectionByIndex(1);
                else
                if (collCenter.x == 0 && collCenter.z == 1.2f)
                    conn = tmpCross.GetConnectionByIndex(2);
                else
                if (collCenter.x == 1.2f && collCenter.z == 0)
                    conn = tmpCross.GetConnectionByIndex(3);
            }
            return conn;
        }

        /// <summary>
        /// Check on the given Pos if a combine to another Street is possible
        /// </summary>
        /// <param name="_hitPoint">The Pos to check around</param>
        /// <param name="_isStart">Is it the Start of the PreviewStreet</param>
        /// <returns></returns>
        private void CheckForCombine(Vector3 _hitPoint, bool _isStart)
        {
            Connection closestConnection = FindClosestConnection(_hitPoint);
            if (closestConnection != null && m_previewLastConnected != closestConnection)
            {
                //Recreate Dead End + Decombine
                DecombinePreview(false);
            }

            if (closestConnection == null) // if there is no valid Street GameObject around
            {
                //Recreate Dead End + Decombine
                DecombinePreview(false);

                //Unlock the Tangent of the Pos which is not Set
                if (pos1 == Vector3.zero)
                {
                    isTangent1Locked = false;
                }
                if (pos3 == Vector3.zero)
                {
                    isTangent2Locked = false;
                }
                return;
            }

            m_previewLastConnected = closestConnection; //check next Update if new closestConnection has chanded to create a DeadEnd on the old place
            if (m_previewStreet.GetStartConnection().m_OtherConnection == closestConnection) return;
            if (closestConnection.m_Owner is Street)
            {
                Street otherStreet = (Street)closestConnection.m_Owner;
                if (!closestConnection.m_OwnerStart)
                {
                    if (_isStart)
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
                        }
                        CombinePreview(otherStreet, false, false);

                        return;
                    }
                }
                else if (closestConnection.m_OwnerStart)
                {
                    if (_isStart)
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
                        if (otherStreet.GetStartConnection().m_OtherComponent is DeadEnd)
                        {
                            StreetComponentManager.DestroyDeadEnd((DeadEnd)otherStreet.GetStartConnection().m_OtherComponent);
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
                        if (otherStreet.GetStartConnection().m_OtherComponent is DeadEnd)
                            StreetComponentManager.DestroyDeadEnd((DeadEnd)otherStreet.GetStartConnection().m_OtherComponent);

                        CombinePreview(otherStreet, true, false);

                        return;
                    }
                }
            }

            if (closestConnection.m_Owner is Cross)
            {
                Cross otherCross = (Cross)closestConnection.m_Owner;
                int otherCrossConnectionIndex = otherCross.GetIndexByConnection(closestConnection);
                OrientedPoint op = otherCross.m_OPs[otherCrossConnectionIndex];

                if (otherCross.m_Connections[otherCrossConnectionIndex].m_OtherComponent is DeadEnd)
                {
                    StreetComponentManager.DestroyDeadEnd((DeadEnd)otherCross.m_Connections[otherCrossConnectionIndex].m_OtherComponent);
                }

                if (_isStart)
                {
                    StreetComponentManager.UpdateStreetTangent1AndStartPoint(m_previewStreet, op.Position + op.Rotation * Vector3.forward * 3f, op.Position);
                    isTangent1Locked = true;
                    Connection.Combine(m_previewStreet.GetStartConnection(), closestConnection);
                }
                else
                {
                    StreetComponentManager.UpdateStreetTangent2AndEndPoint(m_previewStreet, op.Position + op.Rotation * Vector3.forward * 3f, op.Position);
                    isTangent2Locked = true;
                    Connection.Combine(m_previewStreet.m_EndConnection, closestConnection);
                }
            }

        }

        private void CombinePreview(Street _other, bool _otherStart, bool _previewStart)
        {
            if (_previewStart && _otherStart)
            {
                Connection.Combine(m_previewStreet.GetStartConnection(), _other.GetStartConnection());
                return;
            }
            if (_previewStart && !_otherStart)
            {
                Connection.Combine(m_previewStreet.GetStartConnection(), _other.m_EndConnection);
                return;
            }
            if (!_previewStart && _otherStart && m_previewStreet.GetStartConnection().m_OtherConnection != _other.GetStartConnection())
            {
                Connection.Combine(m_previewStreet.m_EndConnection, _other.GetStartConnection());
                return;
            }
            if (!_previewStart && !_otherStart && m_previewStreet.GetStartConnection().m_OtherConnection != _other.m_EndConnection)
            {
                Connection.Combine(m_previewStreet.m_EndConnection, _other.m_EndConnection);
                return;
            }
        }

        private void DecombinePreview(bool _isStart)
        {
            if (_isStart && m_previewStreet.GetStartConnection().m_OtherConnection != null)
            {
                Connection oldConnection = m_previewStreet.GetStartConnection().m_OtherConnection;
                Connection.DeCombine(m_previewStreet.GetStartConnection(), m_previewStreet.GetStartConnection().m_OtherConnection);
                if (oldConnection.m_Owner is Street)
                    StreetComponentManager.CreateDeadEnd((Street)oldConnection.m_Owner, oldConnection.m_OwnerStart);
                else if (oldConnection.m_Owner is Cross)
                {
                    Cross c = (Cross)oldConnection.m_Owner;
                    StreetComponentManager.CreateDeadEnd((Cross)oldConnection.m_Owner, c.GetIndexByConnection(oldConnection));
                }
            }
            else if (m_previewStreet.m_EndConnection.m_OtherConnection != null)
            {
                Connection oldConnection = m_previewStreet.m_EndConnection.m_OtherConnection;
                Connection.DeCombine(m_previewStreet.m_EndConnection, m_previewStreet.m_EndConnection.m_OtherConnection);
                if (oldConnection.m_Owner is Street)
                    StreetComponentManager.CreateDeadEnd((Street)oldConnection.m_Owner, oldConnection.m_OwnerStart);
                else if (oldConnection.m_Owner is Cross)
                {
                    Cross c = (Cross)oldConnection.m_Owner;
                    StreetComponentManager.CreateDeadEnd((Cross)oldConnection.m_Owner, c.GetIndexByConnection(oldConnection));
                }
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