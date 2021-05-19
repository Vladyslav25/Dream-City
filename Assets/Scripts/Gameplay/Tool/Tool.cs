using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Tools
{
    public abstract class Tool : MonoBehaviour
    {
        public TOOLTYPE m_Type;
        [HideInInspector]
        protected Vector3 m_hitPos;
        [HideInInspector]
        protected bool m_validHit;

        private GameObject m_sphere;

        private MeshRenderer m_sphereRender;

        public void Awake()
        {
            m_sphere = Instantiate(ToolManager.Instance.m_spherePrefab);
            m_sphereRender = m_sphere.GetComponent<MeshRenderer>();
        }

        public void Update()
        {
            //Raycast from Mouse to Playground
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) //TODO: Change tto RaycastAll to ignore later Houses and other Collider Stuff
            {
                Vector3 hitPoint = hit.point;
                hitPoint.y = 0; //Set the hitpoint to y = 0 so collider if the Street get ignored
                m_sphere.transform.position = hitPoint; //Set the Sphere for Debug and see possible combinations
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

        public void SetSphereColor(Color _color)
        {
            m_sphereRender.material.color = _color;
        }

        public void SetSphereActiv(bool _input)
        {
            m_sphereRender.enabled = _input;
        }

        public void SetSpherePos(Vector3 _pos)
        {
            m_sphere.transform.position = _pos;
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
        AREA
    }
}
