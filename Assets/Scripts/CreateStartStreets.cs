using Gameplay.StreetComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStartStreets : MonoBehaviour
{
    [SerializeField]
    private Transform streetStart;
    [SerializeField]
    private Transform streetEnd;

    void Start()
    {
        Vector3 midPoint = (streetStart.position - streetEnd.position) * 0.5f + streetEnd.position;
        Vector3 tangent1Pos = (midPoint - streetStart.position) * 0.1f + streetStart.position;
        Vector3 tangent2Pos = (midPoint - streetEnd.position) * 0.1f + streetEnd.position;

        Street s = StreetComponentManager.InitStreetForPreview(streetStart.position);
        StreetComponentManager.UpdateStreet(s, streetStart.position, streetEnd.position, tangent1Pos, tangent2Pos);
        StreetComponentManager.CreateStreet(s);
        Destroy(s.gameObject);
    }
}
