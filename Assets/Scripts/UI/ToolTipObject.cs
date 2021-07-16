using UI;
using UnityEngine;
using UnityEngine.EventSystems;

//cant use EventTrigger beacuse it blocks the scrollrect from scrolling
public class ToolTipObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ProductionBuildingUIItem pbi;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pbi.OnHowerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pbi.OnHowerEnd();
    }
}
