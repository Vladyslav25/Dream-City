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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (pos1 == Vector3.zero)
                {
                    pos1 = hit.point;
                    Debug.Log(pos1);
                    return;
                }
                if (pos2 == Vector3.zero)
                {
                    pos2 = hit.point;
                    Debug.Log(pos2);
                    return;
                }
                if (pos3 == Vector3.zero)
                {
                    pos3 = hit.point;
                    Debug.Log(pos3);
                    SplineManager.CreateSpline(pos1, pos2, pos3);
                    pos1 = Vector3.zero;
                    pos2 = Vector3.zero;
                    pos3 = Vector3.zero;
                    return;
                }
            }
        }
    }
}
