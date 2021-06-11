using Gameplay.Building;
using Gameplay.Tools;
using MyCustomCollsion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeardownTool : Tool
{
    public override void ToolStart()
    {
        Cursor.SetActiv(true);
        Cursor.SetColor(Color.black);
    }

    public override void ToolUpdate()
    {
        if (Input.GetMouseButtonDown(0) && CheckAreaCollision(out Building b))
        {
            b.Destroy();
        }
    }

    private bool CheckAreaCollision(out Building b)
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
}
