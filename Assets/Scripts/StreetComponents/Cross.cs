using Grid;
using MeshGeneration;
using MyCustomCollsion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class Cross : StreetComponent
    {
        public List<SphereCollider> m_coll = new List<SphereCollider>(4); //assigned in Editor
        public Connection[] m_Connections = new Connection[4];
        public OrientedPoint[] m_OPs = new OrientedPoint[4];
        public Vector2 m_center;
        public Vector2[] m_corners = new Vector2[4];

        public Cross Init(Connection _otherConn, bool _needID = false)
        {
            base.Init(_needID);
            m_Connections[0] = new Connection(_otherConn, this, false);
            m_Connections[1] = new Connection(null, this, false);
            m_Connections[2] = new Connection(null, this, false);
            m_Connections[3] = new Connection(null, this, false);
            m_MeshRenderer = GetComponent<MeshRenderer>();
            if (m_MeshRenderer == null) Debug.LogError("No MeshRender found in Cross: " + ID);

            return this;
        }

        public void CreateDeadEnds()
        {
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (m_Connections[i].m_OtherConnection == null)
                    StreetComponentManager.CreateDeadEnd(this, i);

            }
        }

        public bool IsConnectabel(SphereCollider _coll)
        {
            int index = -1;
            for (int i = 0; i < m_coll.Count; i++)
            {
                if (_coll == m_coll[i])
                {
                    index = i;
                }
            }

            if (index == -1)
            {
                Debug.LogError("Cant find Collider in Cross");
                return false;
            }

            if (m_Connections[index].m_OtherConnection == null || m_Connections[index].m_OtherComponent.ID <= 0) return true;
            return false;
        }

        public void SetOP()
        {
            Vector3 v = -transform.forward;
            for (int i = 0; i < 4; i++)
            {
                m_OPs[i] = new OrientedPoint(transform.rotation * m_coll[i].center + transform.position, Quaternion.LookRotation(v, Vector3.up));
                v = Quaternion.Euler(0, 90f, 0) * v;
            }
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

        public int GetIndexByConnection(Connection _conn)
        {
            for (int i = 0; i < m_Connections.Length; i++)
            {
                if (m_Connections[i] == _conn)
                    return i;
            }
            Debug.LogError("Connection is not in the Cross");
            return -1;
        }

        public Connection GetConnectionByIndex(int _index)
        {
            return m_Connections[_index];
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

        public void CheckGridCollision()
        {
            m_center = new Vector2(transform.position.x, transform.position.z);
            m_corners[0] = m_center + new Vector2(1.2f, 1.2f);
            m_corners[1] = m_center + new Vector2(-1.2f, 1.2f);
            m_corners[2] = m_center + new Vector2(-1.2f, -1.2f);
            m_corners[3] = m_center + new Vector2(1.2f, -1.2f);

            HashSet<int> StreetsToRecreate = new HashSet<int>();
            List<Cell> PolyPolyCheckList = new List<Cell>();
            List<Cell> CellsToCheck = GridManager.m_AllCells;

            //Sphere Sphere Coll
            for (int i = 0; i < CellsToCheck.Count; i++)
            {
                if (MyCollision.SphereSphere(m_center, 1.7f, CellsToCheck[i].m_PosCenter, CellsToCheck[i].CellSquareSize))
                {
                    PolyPolyCheckList.Add(CellsToCheck[i]);
                }
            }

            //Poly Poly Coll
            for (int i = 0; i < PolyPolyCheckList.Count; i++)
            {
                if (MyCollision.PolyPoly(PolyPolyCheckList[i].m_Corner, m_corners))
                {
                    PolyPolyCheckList[i].Delete();
                    int id = PolyPolyCheckList[i].m_Street.ID;
                    StreetsToRecreate.Add(id);
                }
            }

            //Recreate Grid
            foreach (int i in StreetsToRecreate)
            {
                Street s = StreetComponentManager.GetStreetByID(i);
                GridManager.RemoveGridMesh(s);
                MeshGenerator.CreateGridMesh(s.m_StreetCells.Values.ToList(), s.m_GridObj.GetComponent<MeshFilter>());
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_OPs[0] == null) return;
            Gizmos.color = Color.black;
            foreach (OrientedPoint op in m_OPs)
            {
                Gizmos.DrawWireSphere(op.Position, 0.2f);
                Gizmos.DrawLine(op.Position, op.Position + op.Rotation * Vector3.forward);
            }
        }
    }
}
