using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModuleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{


    public  bool mouseOverItemDropLocation;
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOverItemDropLocation = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOverItemDropLocation = false;
    }
    public int ModuleType;
    
}
