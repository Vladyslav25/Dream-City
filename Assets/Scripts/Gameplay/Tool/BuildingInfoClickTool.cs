using Gameplay.Tools;
using Gameplay.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Gameplay.Productions;

namespace Gameplay.Tools
{
    public class BuildingInfoClickTool : AClickTool
    {
        public override void ToolStart()
        {
            UIManager.Instance.SetBuildingInfoActiv(false);
            UIManager.Instance.SetProductionInfoActiv(false);
        }

        public override void ToolUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                UIManager instance = UIManager.Instance;

                UIManager.Instance.SetBuildingInfoActiv(false);
                UIManager.Instance.SetProductionInfoActiv(false);
                SetMaterialsColor(null, Color.white);

                if (CheckAreaCollision(out ABuilding b))
                {
                    if (b is Building)
                    {
                        instance.SetBuildingStats((Building)b);
                        instance.SetBuildingInfoActiv(true);
                    }
                    else if (b is ProductionBuilding)
                    {
                        instance.SetBuildingStats((ProductionBuilding)b);
                        instance.SetBuildingInfoActiv(true);
                        instance.SetProductionInfo((ProductionBuilding)b);
                        instance.SetProductionInfoActiv(true);
                    }

                    SetMaterialsColor(b, Color.cyan);
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
