using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splines;
using UnityEditor;

public class Test : MonoBehaviour
{
    Vector3 pos1;
    Vector3 pos2;
    Vector3 pos3;

    bool pos1Set = false;
    bool pos2Set = false;
    bool pos3Set = false;

    Street currendStreet;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (pos1Set == true && pos2Set == false && pos3Set == false && currendStreet != null)
            {
                pos3 = hit.point;
                Vector3 tmpPos2 = (pos3 + pos1) * 0.5f;
                StreetManager.UpdatePreviewStreetTangentPos(currendStreet, tmpPos2);
                StreetManager.UpdatePreviewStreetEndPos(currendStreet, pos3);
            }

            if (pos1Set == true && pos2Set == true && pos3Set == false && currendStreet != null)
            {
                pos3 = hit.point;
                StreetManager.UpdatePreviewStreetTangentPos(currendStreet, pos2);
                StreetManager.UpdatePreviewStreetEndPos(currendStreet, pos3);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (pos1Set == false)
                {
                    pos1 = hit.point;
                    pos1Set = true;
                    currendStreet = StreetManager.InitStreetForPreview(pos1);
                    return;
                }
                else
                if (pos2Set == false)
                {
                    pos2 = hit.point;
                    Debug.Log("Set Pos2 at: " + pos2);
                    pos2Set = true;
                    return;
                }
                else
                if (pos3Set == false)
                {
                    pos3 = hit.point;
                    StreetManager.CreateStreet(pos1, pos2, pos3);
                    pos1 = Vector3.zero;
                    pos2 = Vector3.zero;
                    pos3 = Vector3.zero;
                    pos1Set = false;
                    pos2Set = false;
                    pos3Set = false;
                    Destroy(currendStreet.gameObject);
                    currendStreet = null;
                    return;
                }
            }
        }
    }
}
