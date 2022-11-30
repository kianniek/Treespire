using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Called on enter object.
    /// </summary>
    /// <param name="eventData">Eventdata of click event</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // set cursor back to hover
        GameManager.instance.cursor.PointerEntered();
    }

    /// <summary>
    /// Called on exit object.
    /// </summary>
    /// <param name="eventData">Eventdata of click event</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        // set cursor back to normal
        GameManager.instance.cursor.PointerExited();
    }

    /// <summary>
    /// Called on disable object.
    /// </summary>
    public void OnDisable()
    {
        // set cursor back to normal
        // only on exit isn't enough since
        // that isn't triggered if a parent gameobject
        // is disabled (known unity issue)
        GameManager.instance.cursor.PointerExited();
    }
}
