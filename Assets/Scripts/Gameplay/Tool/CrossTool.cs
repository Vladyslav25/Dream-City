using Gameplay.StreetComponents;
using Gameplay.Tools;
using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossTool : Tool
{
    [SerializeField]
    private GameObject m_crossPrefab;

    private GameObject m_cross;
    private Quaternion m_rotation;

    public override void ToolStart()
    {
        base.ToolStart();
        m_cross = Instantiate(m_crossPrefab, m_hitPos, Quaternion.identity);
        SetSphereActiv(false);
    }

    public override void ToolEnd()
    {
        base.ToolEnd();
        if (m_cross != null)
            Destroy(m_cross);
    }

    public override void ToolUpdate()
    {
        m_cross.transform.position = m_hitPos;

        if (Input.GetMouseButtonDown(0))
        {
            GameObject[] obj = SpawnCross(); //Spawn Crosses + Coll Cross
            m_cross = Instantiate(m_crossPrefab, m_hitPos, m_rotation); //Spawn new Preview Cross
        }

        if (Input.GetKey(KeyCode.Comma))
        {
            m_cross.transform.Rotate(new Vector3(0, -0.5f, 0));
            m_rotation = m_cross.transform.rotation;
        }
        if (Input.GetKey(KeyCode.Period))
        {
            m_cross.transform.Rotate(new Vector3(0, 0.5f, 0));
            m_rotation = m_cross.transform.rotation;
        }
    }

    private GameObject[] SpawnCross()
    {
        GameObject[] output = new GameObject[2];
        //Visible Cross
        output[0] = Instantiate(m_crossPrefab, m_hitPos, m_rotation);
        output[0].transform.parent = StreetComponentManager.Instance.transform;
        Cross c = output[0].GetComponent<Cross>();
        c.Init(null, true);
        for (int i = 0; i < 4; i++)
        {
            StreetComponentManager.CreateDeadEnd(c, i);
        }

        //Collider Cross
        GameObject obj = Instantiate(m_crossPrefab, m_hitPos, m_rotation, StreetComponentManager.Instance.StreetCollisionParent.transform);
        obj.layer = 8;
        obj.name = "Cross_Col";

        //Remove Collision Sphere Collider from Prefab
        Collider[] coll = obj.GetComponents<Collider>();
        foreach (Collider co in coll)
            Destroy(co);

        //Set Coll Material
        MeshRenderer mr = obj.gameObject.GetComponent<MeshRenderer>();
        mr.material = StreetComponentManager.Instance.streetMatColl;
        output[1] = obj;

        return output;
    }
}
