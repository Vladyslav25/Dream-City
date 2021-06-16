using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Buildings
{
    public abstract class ABuilding : MonoBehaviour
    {
        [SerializeField]
        protected int width;
        [SerializeField]
        protected int depth;
        
        public EAssignment m_Assigment;
        public bool InverseRotation;

        [HideInInspector]
        public Area m_Area;
        protected GameObject m_ColliderPlane;

        public Vector2Int Size { get { return new Vector2Int(depth, width); } }

        public GameObject GetColliderPlane()
        {
            if (m_ColliderPlane != null)
                return m_ColliderPlane;
            else
            {
                foreach (Transform child in transform)
                {
                    if (child.gameObject.layer == 8)
                    {
                        m_ColliderPlane = child.gameObject;
                    }
                }
                if (m_ColliderPlane == null) Debug.LogError("Building Obj: " + gameObject + " couldnt find a Collider Plane");
                return m_ColliderPlane;
            }
        }

        public abstract void Destroy();

        public void OnDrawGizmosSelected()
        {
            if (m_Area == null) return;

            switch (m_Area.m_Assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    Gizmos.color = Color.green;
                    break;
                case EAssignment.BUSINESS:
                    Gizmos.color = Color.blue;
                    break;
                case EAssignment.INDUSTRY:
                    Gizmos.color = Color.yellow;
                    break;
                case EAssignment.PRODUCTION:
                    Gizmos.color = Color.cyan;
                    break;
            }

            foreach (Cell c in m_Area.m_Cells)
            {
                Gizmos.DrawWireSphere(c.m_WorldPosCenter, c.m_Radius * 0.8f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(m_Area.m_OP.Position, m_Area.m_OP.Position + Vector3.up * 3f);
            Gizmos.DrawLine(m_Area.m_OP.Position + Vector3.up * 3f, m_Area.m_OP.Rotation * (Vector3.forward * 2f) + m_Area.m_OP.Position + Vector3.up * 3f);
        }
    }
}
