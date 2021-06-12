using Gameplay.Buildings;
using Gameplay.Tools;
using MyCustomCollsion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeardownTool : AClickTool
{
    public override void ToolStart()
    {
        Cursor.SetActiv(true);
        Cursor.SetColor(Color.black);
    }

    public override void ToolUpdate()
    {
        if (CheckAreaCollision(out Building b))
        {
            SetMaterialsColor(b, Color.red);

            if (Input.GetMouseButtonDown(0))
                b.Destroy();
        }
    }

    public override void ToolEnd()
    {
        SetMaterialsColor(null, Color.white);
    }
}
