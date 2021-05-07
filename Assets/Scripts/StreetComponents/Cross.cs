using MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class Cross : StreetComponent
    {
        public List<SphereCollider> m_coll = new List<SphereCollider>(4);
        public Connection[] m_Connections = new Connection[4];
        public OrientedPoint[] m_OPs = new OrientedPoint[4];

        public Cross Init(Connection _conn, bool _needID = false)
        {
            base.Init(_needID);
            m_StartConnection = null;
            m_Connections[0] = _conn;
            Vector3 v = Vector3.back;
            for (int i = 0; i < 4; i++)
            {
                m_OPs[i] = new OrientedPoint(m_coll[i].center + transform.position, transform.rotation * Quaternion.LookRotation(v, Vector3.up));
                v.y += 90;
            }
            return this;
        }

        public int GetIndex(StreetComponent _other)
        {
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (m_Connections[i].GetOtherComponent(this) == _other)
                    return i;
            }
            Debug.LogError("No Connection with this Component: " + _other);
            return -1;
        }
    }
}
