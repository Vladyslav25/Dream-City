using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splines;
using UnityEditor;

namespace Streets
{
    public class StreetTool : MonoBehaviour
    {
        [SerializeField]
        GameObject spherePrefab;

        Vector3 pos1;
        Vector3 pos2;
        Vector3 pos3;

        bool pos1Set = false;
        bool pos2Set = false;
        bool pos3Set = false;

        bool isTangent1Locked = false;
        bool isTangent2Locked = false;

        bool isCurrendToolLine = false;

        Street previewStreet;

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
                if (sphereHits[i].CompareTag("StreetEnd") || sphereHits[i].CompareTag("StreetStart"))
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
                //Unlock the Tanget of the Pos wich is not Set
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
                    Vector3 dir = -(otherStreet.m_Spline.Tangent2Pos - otherStreet.m_Spline.EndPos); //Get the point symmetrical Direction
                    StreetManager.UpdateStreetTangent1AndStartPoint(
                        previewStreet, 
                        dir + otherStreet.m_Spline.EndPos, //Set the Tanget to the point symmetrical Position
                        otherStreet.m_Spline.EndPos);
                    isTangent1Locked = true;
                    return;
                }
                else
                {
                    //other Street End and previewStreet End -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent2Pos - otherStreet.m_Spline.EndPos);
                    StreetManager.UpdateStreetTangent2AndEndPoint(
                        previewStreet,
                        dir + otherStreet.m_Spline.EndPos,
                        otherStreet.m_Spline.EndPos
                        );

                    isTangent2Locked = true;
                    return;
                }
            }
            else if (closestStreetChildren.CompareTag("StreetStart"))
            {
                if (isStart)
                {
                    //other Street Start and previewStreet Start -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent1Pos - otherStreet.m_Spline.StartPos);
                    StreetManager.UpdateStreetTangent1AndStartPoint(
                        previewStreet,
                        dir + otherStreet.m_Spline.StartPos,
                        otherStreet.m_Spline.StartPos
                        );

                    isTangent1Locked = true;
                    return;
                }
                else
                {
                    //other Street Start and previewStreet End -> Combine
                    Vector3 dir = -(otherStreet.m_Spline.Tangent1Pos - otherStreet.m_Spline.StartPos);
                    StreetManager.UpdateStreetTangent2AndEndPoint(
                        previewStreet,
                        otherStreet.m_Spline.StartPos,
                        dir + otherStreet.m_Spline.StartPos
                        );

                    isTangent2Locked = true;
                    return;
                }
            }
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