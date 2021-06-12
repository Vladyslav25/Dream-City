using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Tools
{
    public abstract class Tool : MonoBehaviour
    {
        public TOOLTYPE m_Type;
        [HideInInspector]
        protected Vector3 m_hitPos;
        [HideInInspector]
        protected bool m_validHit;

        private int pointerID = -1;

        private void Awake()
        {
#if !UNITY_EDITOR
            pointerID = 0;  // needs to be in a standalone version 0 to check if the mouse i shovering over UI
#endif
        }

        public void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject(pointerID) == true) return;

            //Raycast from Mouse to Playground
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) //TODO: Change tto RaycastAll to ignore later Houses and other Collider Stuff
            {
                Vector3 hitPoint = hit.point;
                hitPoint.y = 0; //Set the hitpoint to y = 0 so collider if the Street get ignored
                Cursor.SetPosition(hitPoint); //Set the Sphere for Debug and see possible combinations
                m_hitPos = hitPoint;
                m_validHit = true;
                ToolUpdate();
            }
            else
            {
                m_validHit = false;
                //Debug.LogError("Tool: " + m_Type + " faild Raycast: Mouse -> Screen -> Ground");
            }

        }

        /// <summary>
        /// Get the closest GameObject from the given List
        /// </summary>
        /// <param name="_objs">The List of GameObject</param>
        /// <param name="_point">The Point from where to look</param>
        /// <returns>The closest GameObject to the Point</returns>
        protected GameObject GetClosesetGameObject(List<GameObject> _objs, Vector3 _point)
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

        protected SphereCollider GetClosesetCollider(List<SphereCollider> _objs, Vector3 _point)
        {
            if (_objs.Count == 1) return _objs[0];

            float shortestDistance = float.MaxValue;
            SphereCollider output = null;

            for (int i = 0; i < _objs.Count; i++)
            {
                float distance = Vector3.SqrMagnitude(_objs[i].transform.position - _point);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    output = _objs[i];
                }
            }
            return output;
        }

        public abstract void ToolUpdate();

        public virtual void ToolStart() { }

        public virtual void ToolEnd() { }
    }

    public enum TOOLTYPE
    {
        NONE,
        STREET,
        CROSS,
        ASSIGNMENT,
        TEARDOWN,
        BUILDINGCLICK
    }
}
