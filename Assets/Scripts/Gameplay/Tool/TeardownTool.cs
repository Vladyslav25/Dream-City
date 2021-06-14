using Gameplay.Buildings;
using Gameplay.Tools;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class TeardownTool : AClickTool
{

    public override void ToolStart()
    {
        Cursor.SetActiv(true);
        Cursor.SetColor(Color.black);
        UIManager.Instance.DeactivateUI();
    }

    public override void ToolUpdate()
    {
        if (CheckAreaCollision(out Building b))
        {
            SetMaterialsColor(b, Color.red);
            if (Input.GetMouseButtonDown(0))
                b.Destroy();
        }
        else
        {
                SetMaterialsColor(null, Color.white);
        }
    }

    public override void ToolEnd()
    {
        SetMaterialsColor(null, Color.white);
        Cursor.SetActiv(false);
    }
}
