using MeshGeneration;
using Streets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cross : MonoBehaviour
{
    public Connection[] m_Connections;

    private void Awake()
    {
        m_Connections = new Connection[4];
        for (int i = 0; i < m_Connections.Length; i++)
        {
            m_Connections[i] = new Connection();
        }

        m_Connections[0].m_Direction = Vector3.down;
        m_Connections[1].m_Direction = Vector3.up;
        m_Connections[2].m_Direction = Vector3.right;
        m_Connections[3].m_Direction = Vector3.left;

        m_Connections[0].m_Trans.position = new Vector3(-1.25f, 0, 0) + transform.position;
        m_Connections[1].m_Trans.position = new Vector3(1.25f, 0, 0) + transform.position;
        m_Connections[2].m_Trans.position = new Vector3(0, 0, 1.25f) + transform.position;
        m_Connections[3].m_Trans.position = new Vector3(0, 0, -1.25f) + transform.position;

        m_Connections[0].m_Trans.rotation = Quaternion.Euler(m_Connections[0].m_Trans.eulerAngles + new Vector3(0, 180, 0));
        m_Connections[1].m_Trans.rotation = m_Connections[0].m_Trans.rotation = Quaternion.Euler(m_Connections[0].m_Trans.eulerAngles + new Vector3(0, 0, 0));
        m_Connections[2].m_Trans.rotation = m_Connections[0].m_Trans.rotation = Quaternion.Euler(m_Connections[0].m_Trans.eulerAngles + new Vector3(0, 90, 0));
        m_Connections[3].m_Trans.rotation = m_Connections[0].m_Trans.rotation = Quaternion.Euler(m_Connections[0].m_Trans.eulerAngles + new Vector3(0, -90, 0));
    }

    public void CreateConnection(Vector3 _dir, Street _street = null)
    {
        foreach (Connection conn in m_Connections)
        {
            if (conn.m_Direction == _dir)
                if (_street == null)
                {
                    CreateDeadEnd(conn);
                }
                else
                {
                    conn.m_Street = _street;
                }
        }

    }

    private DeadEnd CreateDeadEnd(Connection _conn)
    {
        GameObject obj = new GameObject("DeadEnd");
        DeadEnd de = obj.AddComponent<DeadEnd>();
        de.Init(new DeadEndShape(), _conn.m_Trans, transform);
        _conn.m_Street = de;
        return de;
    }
}
