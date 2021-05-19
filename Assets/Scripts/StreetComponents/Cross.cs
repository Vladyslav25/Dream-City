using MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class Cross : StreetComponent
    {
        public List<SphereCollider> m_coll = new List<SphereCollider>(4); //assigned in Editor
        public Connection[] m_Connections = new Connection[4];
        public OrientedPoint[] m_OPs = new OrientedPoint[4];

        public Cross Init(Connection _otherConn, bool _needID = false)
        {
            base.Init(_needID);
            m_Connections[0] = new Connection(_otherConn, this, false);
            m_Connections[1] = new Connection(null, this, false);
            m_Connections[2] = new Connection(null, this, false);
            m_Connections[3] = new Connection(null, this, false);

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
                if (m_Connections[i].m_OtherComponent == _other)
                    return i;
            }
            Debug.LogError("No Connection with this Component: " + _other);
            return -1;
        }

        public void SetID()
        {
            ID = StreetComponentManager.GetNewStreetComponentID();
        }

        public override void SetStartConnection(Connection _conn)
        {
            m_Connections[0] = _conn;
        }

        public override Connection GetStartConnection()
        {
            return m_Connections[0];
        }
    }
}
