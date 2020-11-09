using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splines;
using UnityEditor;

public class Test : MonoBehaviour
{
    [SerializeField]
    GameObject spherePrefab;

    Vector3 pos1;
    Vector3 pos2;
    Vector3 pos3;

    bool pos1Set = false;
    bool pos2Set = false;
    bool pos3Set = false;

    Street currendStreet;

    GameObject sphere;

    private void Awake()
    {
        sphere = Instantiate(spherePrefab);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPoint = hit.point;
            hitPoint.y = 0;
            sphere.transform.position = hitPoint;

            if (pos1Set == true && pos2Set == false && pos3Set == false && currendStreet != null)
            {
                pos3 = hitPoint;
                Vector3 pos2Tmp = (pos1 + pos3) * 0.5f;
                Vector3 tangent1 = (pos1 + pos2Tmp) * 0.5f;
                Vector3 tangent2 = (pos2Tmp + pos3) * 0.5f;
                StreetManager.UpdatePreviewStreetTangent1Pos(currendStreet, tangent1);
                StreetManager.UpdatePreviewStreetTangent2Pos(currendStreet, tangent2);
                StreetManager.UpdatePreviewStreetEndPos(currendStreet, pos3);
            }

            if (pos1Set == true && pos2Set == true && pos3Set == false && currendStreet != null)
            {
                pos3 = hitPoint;
                Vector3 tangent1 = (pos1 + pos2) * 0.5f;
                Vector3 tangent2 = (pos2 + pos3) * 0.5f;
                StreetManager.UpdatePreviewStreetTangent1Pos(currendStreet, tangent1);
                StreetManager.UpdatePreviewStreetTangent2Pos(currendStreet, tangent2);
                StreetManager.UpdatePreviewStreetEndPos(currendStreet, pos3);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (pos1Set == false)
                {
                    pos1 = hitPoint;
                    pos1Set = true;
                    currendStreet = StreetManager.InitStreetForPreview(pos1);
                }
                else
                if (pos2Set == false)
                {
                    pos2 = hitPoint;
                    pos2Set = true;
                }
                else
                if (pos3Set == false)
                {
                    pos3 = hitPoint;
                    Vector3 tangent1 = (pos1 + pos2) * 0.5f;
                    Vector3 tangent2 = (pos3 + pos2) * 0.5f;
                    StreetManager.CreateStreet(pos1, tangent1, tangent2, pos3);
                    pos1 = Vector3.zero;
                    pos2 = Vector3.zero;
                    pos3 = Vector3.zero;
                    pos1Set = false;
                    pos2Set = false;
                    pos3Set = false;
                    Destroy(currendStreet.gameObject);
                    currendStreet = null;
                }

                if (currendStreet == null || pos1Set == true) return;

                Collider[] sphereHits = Physics.OverlapSphere(hitPoint, 2);
                List<GameObject> hittedStreetsChildern = new List<GameObject>();

                for (int i = 0; i < sphereHits.Length; i++)
                {
                    if (sphereHits[i].CompareTag("StreetEnd") || sphereHits[i].CompareTag("StreetStart"))
                        hittedStreetsChildern.Add(sphereHits[i].transform.gameObject);
                }
                if (hittedStreetsChildern.Count == 0) return;
                Debug.Log("Combined");
                GameObject hittedStreetChildren = GetClosesedGameObject(hittedStreetsChildern, hitPoint);
                if (hittedStreetChildren.CompareTag("StreetEnd"))
                {
                    currendStreet.m_Spline.SetStartPos(hittedStreetChildren.transform.position);
                }
                else if (hittedStreetChildren.CompareTag("StreetStart"))
                {
                    currendStreet.m_Spline.SetStartPos(hittedStreetChildren.transform.position);
                }
            }
        }
    }

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
