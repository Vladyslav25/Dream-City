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

        protected bool CheckAreaCollision(out Building b)
        {
            List<Area> PolyPolyCheck = new List<Area>();
            b = null;

            foreach (Area area in HousingManager.m_AllAreas)
            {
                if (MyCollision.SphereSphere(new Vector2(m_hitPos.x, m_hitPos.z), 0.8f, new Vector2(area.m_OP.Position.x, area.m_OP.Position.z), area.m_Radius))
                {
                    PolyPolyCheck.Add(area);
                }
            }
            if (PolyPolyCheck.Count == 0) return false;
            else
            {
                b = PolyPolyCheck[0].m_Building;
                return true;
            }
        }

        protected void SetMaterialsColor(Building _b, Color _c)
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
