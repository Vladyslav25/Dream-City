using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splines;
using UnityEditor;
using System;

namespace Streets
{
    public class StreetTool : MonoBehaviour
    {
        [SerializeField]
        GameObject spherePrefab;
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

        bool previewInValidForm = false;

        Street previewStreet;

        Street lastConnectedStreet;
        GameObject lastConnectedStreetChildren;

        GameObject sphere;

        private void Awake()
        {
            sphere = Instantiate(spherePrefab);
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.C)) //Change to Curve Tool
            {
                sphere.GetComponent<MeshRenderer>().material.color = Color.blue;
                isCurrendToolLine = false;
            }
            if (Input.GetKeyUp(KeyCode.L)) //Change to Line Tool
            {
                sphere.GetComponent<MeshRenderer>().material.color = Color.red;
                isCurrendToolLine = true;
            }

            //Raycast from Mouse to Playground
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) //TODO: Change tto RaycastAll to ignore later Houses and other Collider Stuff
            {
                Vector3 hitPoint = hit.point;
                hitPoint.y = 0; //Set the hitpoint to y = 0 so collider if the Street get ignored
                sphere.transform.position = hitPoint; //Set the Sphere for Debug and see possible combinations

                if (pos1Set == false && pos2Set == false && pos3Set == false)
                {
                    FindStreetGameObject(hitPoint); //if no Point is set, look for not far away Street possible combinations
                }

                if (isCurrendToolLine)
                {
                    if (pos1Set == true && pos3Set == false && previewStreet != null) //Line: First Point Set -> Missing Sec Point
                    {
                        Vector3 pos2Tmp = previewStreet.m_Spline.GetCentredOrientedPoint().Position; //Get the Midpoint on the Spline
                        Vector3 tangent1 = (pos1 + pos2Tmp) * 0.5f; // 1.Tanget must be between MidPoint and Start
                        Vector3 tangent2 = (pos2Tmp + hitPoint) * 0.5f; // 2. Tanget must be between MidPoint and End (MousePos)
                        UpdatePreview(tangent1, tangent2, hitPoint); //Update the Preview (update if Tanget is not locked)
                        CheckForCombine(hitPoint, false); //If a Combine is possible it Combine (overwrite Tanget Pos)
                    }
                }
                else
                {
                    if (pos1Set == true && pos2Set == false && pos3Set == false && previewStreet != null) // Curve: First Point Set -> Wait for Sec Point
                    {
                        Vector3 pos2Tmp = (pos1 + hitPoint) * 0.5f; //Get the Midpoint between Start and End
                        Vector3 tangent1 = (pos1 + pos2Tmp) * 0.5f; //The 1.Tangent is between the Start and the Mid
                        Vector3 tangent2 = (pos2Tmp + hitPoint) * 0.5f; //The 2. Tangent is between the End and the Mid
                        UpdatePreview(tangent1, tangent2, hitPoint); //Update the Preview (update if Tanget is not locked)
                        //The Scecond Point cant Combine to an another Spline

                    }

                    if (pos1Set == true && pos2Set == true && pos3Set == false && previewStreet != null)
                    {
                        Vector3 tangent1 = (pos1 + pos2) * 0.5f; //The 1.Tanget is between Start and Sec Point 
                        Vector3 tangent2 = (pos2 + hitPoint) * 0.5f; // The 2.Tanget is between Sec Point and EndPoint (MousePos)
                        UpdatePreview(tangent1, tangent2, hitPoint); //Update the Preview (update if Tanget is not locked)
                        CheckForCombine(hitPoint, false); //If a Combine is possible it Combine (overwrite Tanget Pos)
                    }
                }

                if (Input.GetKeyUp(KeyCode.Escape))
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

                    if (lastConnectedStreetChildren != null)
                        if (lastConnectedStreetChildren.CompareTag("StreetStart"))
                            lastConnectedStreet.CreateDeadEnd(true);
                        else if (lastConnectedStreetChildren.CompareTag("StreetEnd"))
                            lastConnectedStreet.CreateDeadEnd(false);

                    lastConnectedStreet = null;
                    lastConnectedStreetChildren = null;

                    if (previewStreet != null)
                    {
                        if (previewStreet.m_StreetConnect_Start != null)
                        {
                            if (previewStreet.m_StreetConnect_Start.m_StreetConnect_Start == previewStreet)
                                previewStreet.m_StreetConnect_Start.m_StreetConnect_Start.CreateDeadEnd(true);
                            if (previewStreet.m_StreetConnect_Start.m_StreetConnect_End == previewStreet)
                                previewStreet.m_StreetConnect_Start.m_StreetConnect_End.CreateDeadEnd(false);
                        }
                        if (previewStreet.m_StreetConnect_End != null)
                        {
                            if (previewStreet.m_StreetConnect_End.m_StreetConnect_Start == previewStreet)
                                previewStreet.m_StreetConnect_End.m_StreetConnect_Start.CreateDeadEnd(true);
                            if (previewStreet.m_StreetConnect_End.m_StreetConnect_End == previewStreet)
                                previewStreet.m_StreetConnect_End.m_StreetConnect_End.CreateDeadEnd(false);
                        }
                        Destroy(previewStreet.gameObject); //Destroy PreviewStreet
                    }

                    previewStreet = null;
                    return;
                }

                if (previewStreet != null && !CheckForValidForm())
                    return;


                if (Input.GetMouseButtonDown(0))
                {
                    if (pos1Set == false)
                    {
                        pos1 = hitPoint;
                        pos1Set = true;
                        previewStreet = StreetManager.InitStreetForPreview(pos1); //Init a Preview Street (with invalid ID)
                        CheckForCombine(hitPoint, true); //Check if the Start in the Preview Street can be combined
                        return;
                    }

                    if (!isCurrendToolLine) // In Curve Tool -> Set Sec Pos
                    {
                        if (pos2Set == false)
                        {
                            pos2 = hitPoint;
                            pos2Set = true;
                            return;
                        }
                    }
                    if (pos3Set == false)
                    {
                        pos3 = hitPoint;
                        CheckForCombine(hitPoint, false); //Check if the End in the Preview Street can be combined
                        StreetManager.CreateStreet(previewStreet); //Create a valid Street from the preview Street
                        //Reset
                        pos1 = Vector3.zero;
                        pos2 = Vector3.zero;
                        pos3 = Vector3.zero;
                        pos1Set = false;
                        pos2Set = false;
                        pos3Set = false;
                        isTangent1Locked = false;
                        isTangent2Locked = false;
                        Destroy(previewStreet.gameObject); //Destroy PreviewStreet
                        previewStreet = null;

                        lastConnectedStreet = null;
                        lastConnectedStreetChildren = null;
                        return;
                    }
                }
            }
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
                StreetManager.UpdateStreetTangent1Pos(previewStreet, _tangent1);
            if (!isTangent2Locked)
                StreetManager.UpdateStreetTangent2Pos(previewStreet, _tangent2);
            StreetManager.UpdateStreetEndPos(previewStreet, _endPos);
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
            }
            if (hittedStreetsChildern.Count == 0) return null; //return null if no valid Street GameObbject was found in the Sphere

            GameObject closestStreetChildren = GetClosesedGameObject(hittedStreetsChildern, _hitPoint); //Look for the closest GameObject of all valid GameObjects
            sphere.transform.position = closestStreetChildren.transform.position; //Set the Sphere to the Pos of the closest valid Street GameObject
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
                if (lastConnectedStreet != null && lastConnectedStreetChildren != null)
                    RecreateDeadEnd(lastConnectedStreet, lastConnectedStreetChildren);

                //Unlock the Tangent of the Pos which is not Set
                if (pos1 == Vector3.zero)
                    isTangent1Locked = false;
                if (pos3 == Vector3.zero)
                    isTangent2Locked = false;
                return;
            }

            Street otherStreet = closestStreetChildren.GetComponentInParent<Street>();

            lastConnectedStreetChildren = closestStreetChildren;
            lastConnectedStreet = otherStreet;

            if (closestStreetChildren.CompareTag("StreetEnd"))
            {
                if (isStart)
                {
                    //other Street End and previewStreet Start -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent2Pos - otherStreet.m_Spline.EndPos).normalized; //Get the point symmetrical Direction

                    StreetManager.UpdateStreetTangent1AndStartPoint(
                        previewStreet,
                        dir * 4f + otherStreet.m_Spline.EndPos, //Set the Tanget to the half point symmetrical Position
                        otherStreet.m_Spline.EndPos
                        ); //Set the EndPos

                    isTangent1Locked = true;
                    otherStreet.RemoveDeadEnd(false, previewStreet);
                    previewStreet.m_StreetConnect_Start = otherStreet;
                    return;
                }
                else
                {
                    //other Street End and previewStreet End -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent2Pos - otherStreet.m_Spline.EndPos).normalized;

                    StreetManager.UpdateStreetTangent2AndEndPoint(
                        previewStreet,
                        dir * 4f + otherStreet.m_Spline.EndPos,
                        otherStreet.m_Spline.EndPos
                        );

                    isTangent2Locked = true;
                    otherStreet.RemoveDeadEnd(false, previewStreet);
                    previewStreet.m_StreetConnect_End = otherStreet;
                    return;
                }
            }
            else if (closestStreetChildren.CompareTag("StreetStart"))
            {
                if (isStart)
                {
                    //other Street Start and previewStreet Start -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent1Pos - otherStreet.m_Spline.StartPos).normalized;

                    StreetManager.UpdateStreetTangent1AndStartPoint(
                        previewStreet,
                        dir * 4f + otherStreet.m_Spline.StartPos,
                        otherStreet.m_Spline.StartPos
                        );

                    isTangent1Locked = true;
                    otherStreet.RemoveDeadEnd(true, previewStreet);
                    previewStreet.m_StreetConnect_Start = otherStreet;
                    return;
                }
                else
                {
                    //other Street Start and previewStreet End -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent1Pos - otherStreet.m_Spline.StartPos).normalized;

                    StreetManager.UpdateStreetTangent2AndEndPoint(
                        previewStreet,
                        dir * 4f + otherStreet.m_Spline.StartPos,
                        otherStreet.m_Spline.StartPos
                        );

                    isTangent2Locked = true;
                    otherStreet.RemoveDeadEnd(true, previewStreet);
                    previewStreet.m_StreetConnect_End = otherStreet;
                    return;
                }
            }
        }

        private void RecreateDeadEnd(Street _lastConnectedStreet, GameObject _lastConnectedStreetChildren)
        {
            if (previewStreet.m_StreetConnect_Start != null && previewStreet.m_StreetConnect_End != null
                && previewStreet.m_StreetConnect_End == previewStreet.m_StreetConnect_Start)
                if (_lastConnectedStreetChildren.CompareTag("StreetStart") && _lastConnectedStreet.m_StreetConnect_End != previewStreet
                    || _lastConnectedStreetChildren.CompareTag("StreetEnd") && _lastConnectedStreet.m_StreetConnect_Start != previewStreet)
                {
                    previewStreet.m_StreetConnect_End = null;
                    return;
                }
            if (previewStreet.m_StreetConnect_End == lastConnectedStreet)
                if (_lastConnectedStreetChildren.CompareTag("StreetStart") && _lastConnectedStreet.m_StreetConnect_Start == previewStreet)
                {
                    _lastConnectedStreet.CreateDeadEnd(true);
                    previewStreet.m_StreetConnect_End = null;
                }
                else
                if (_lastConnectedStreetChildren.CompareTag("StreetEnd") && _lastConnectedStreet.m_StreetConnect_End == previewStreet)
                {
                    _lastConnectedStreet.CreateDeadEnd(false);
                    previewStreet.m_StreetConnect_End = null;
                }
        }

        private bool CheckForValidForm()
        {
            if (previewStreet == null) return false;

            float validDistanceStartEnd = 2f;
            float maxDeltaAngel = 17f;

            Vector3 StartPos = previewStreet.m_Spline.StartPos;
            Vector3 EndPos = previewStreet.m_Spline.EndPos;
            Vector3 Tangent1 = previewStreet.m_Spline.Tangent1Pos;
            Vector3 Tangent2 = previewStreet.m_Spline.Tangent2Pos;

            if (Vector3.Distance(StartPos, EndPos) < validDistanceStartEnd)
            {
                StreetManager.SetStreetColor(previewStreet, Color.red);
                return false;
            }

            for (int i = 0; i < previewStreet.m_Spline.OPs.Length - 1; i++)
            {
                Vector3 opTangent1 = previewStreet.m_Spline.GetTangentAt(previewStreet.m_Spline.OPs[i].t);
                Vector3 opTangent2 = previewStreet.m_Spline.GetTangentAt(previewStreet.m_Spline.OPs[i + 1].t);
                if (Vector3.Angle(opTangent1, opTangent2) > maxDeltaAngel)
                {
                    StreetManager.SetStreetColor(previewStreet, Color.red);
                    return false;
                }
            }
            StreetManager.SetStreetColor(previewStreet, Color.green);
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
    }
}