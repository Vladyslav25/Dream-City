using Gameplay.Tools;
using Gameplay.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

namespace Gameplay.Tools
{
    public class BuildingInfoClickTool : AClickTool
    {
        public override void ToolStart()
        {
            UIManager.Instance.SetBuildingInfoActiv(false);
        }

        public override void ToolUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (CheckAreaCollision(out ABuilding b))
                {
                    UIManager.Instance.SetBuildingInfoActiv(true);
                    UIManager.Instance.SetBuildingStats((Building)b);
                    SetMaterialsColor(b, Color.cyan);
                }
                else
                {
                    UIManager.Instance.SetBuildingInfoActiv(false);
                    SetMaterialsColor(null, Color.white);
                }
            }
        }

        public override void ToolEnd()
        {
            UIManager.Instance.SetBuildingInfoActiv(false);
            SetMaterialsColor(null, Color.white);
        }
    }
}
