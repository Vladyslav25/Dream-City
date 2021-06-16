using Gameplay.Buildings;
using MyCustomCollsion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Tools
{

    public abstract class AClickTool : Tool
    {
        protected MeshRenderer lastRenderer;

        /// <summary>
        /// Check if the HitPos is over an Building/Area
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected bool CheckAreaCollision(out ABuilding b)
        {
            List<Area> SphereSphere = new List<Area>();
            b = null;

            foreach (Area area in HousingManager.m_AllAreas)
            {
                if (MyCollision.SphereSphere(new Vector2(m_hitPos.x, m_hitPos.z), 0.8f, new Vector2(area.m_OP.Position.x, area.m_OP.Position.z), area.m_Radius))
                {
                    SphereSphere.Add(area);
                }
            }
            if (SphereSphere.Count == 0) return false;
            else
            {
                int index = -1;
                float closestDistance = float.MaxValue;
                for (int i = 0; i < SphereSphere.Count; i++)
                {
                    float distance = Vector3.SqrMagnitude(SphereSphere[i].m_OP.Position - m_hitPos);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        index = i;
                    }
                }
                b = SphereSphere[index].m_Building;
                return true;
            }
        }

        /// <summary>
        /// Change the material Color of a building
        /// </summary>
        /// <param name="_b">The Building to change the Material</param>
        /// <param name="_c">The Color to change it</param>
        protected void SetMaterialsColor(ABuilding _b, Color _c)
        {
            Material[] mats;
            if (lastRenderer != null)
            {
                //Reset lastRenderer
                mats = lastRenderer.materials;
                foreach (Material mat in mats)
                {
                    mat.color = Color.white;
                }
                lastRenderer.materials = mats;
            }

            //Set new Renderer
            if (_b == null) return;
            lastRenderer = _b.gameObject.GetComponent<MeshRenderer>();
            if (lastRenderer == null) lastRenderer = _b.gameObject.GetComponentInChildren<MeshRenderer>();
            if (lastRenderer == null || lastRenderer.gameObject.layer == 8) Debug.LogError("No or invalid Renderer found");

            mats = lastRenderer.materials;
            foreach (Material mat in mats)
            {
                mat.color = _c;
            }
            lastRenderer.materials = mats;
        }
    }
}
