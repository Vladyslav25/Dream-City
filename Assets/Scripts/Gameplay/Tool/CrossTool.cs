using Gameplay.StreetComponents;
using Gameplay.Tools;
using Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossTool : Tool
{
    [SerializeField]
    private GameObject m_crossPrefab;
    private Cross m_cross;
    private GameObject m_crossObj;
    private Quaternion m_rotation;

    public override void ToolStart()
    {
        base.ToolStart();
        m_crossObj = SpawnCross();
        SetSphereActiv(false);
    }

    public override void ToolEnd()
    {
        base.ToolEnd();
        if (m_crossObj != null)
            Destroy(m_crossObj);
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
        }

        if (Input.GetMouseButtonDown(0))
        {
            SpawnCollCross(); //Spawn Coll Cross
            m_cross.SetID();
            m_crossObj = SpawnCross(); //Spawn new Preview Cross
        }

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
        GameObject obj = Instantiate(m_crossPrefab, m_hitPos, Quaternion.identity);
        m_cross = obj.GetComponent<Cross>();
        m_cross.Init(null, false);
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

        //Set Coll Material
        MeshRenderer mr = obj.gameObject.GetComponent<MeshRenderer>();
        mr.material = StreetComponentManager.Instance.streetMatColl;

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

        GameObject closestStreetChildren = GetClosesedGameObject(hittedStreetsChildern, _hitPoint); //Look for the closest GameObject of all valid GameObjects
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
