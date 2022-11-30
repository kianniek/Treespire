using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler
{
    // delegate to callback on click
    private WindowUI.ButtonDelegate onClickDelegate;

    /// <summary>
    /// Sets the delegated called on click.
    /// </summary>
    /// <param name="onClickDelegate">delegate to be called on click</param>
    internal void SetOnClickDelegate(WindowUI.ButtonDelegate onClickDelegate)
    {
        // store the delegate
        this.onClickDelegate = onClickDelegate;
    }

    /// <summary>
    /// Called on click on this object
    /// </summary>
    /// <param name="eventData">Eventdata from click event</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // callback on delegate if it exists
        onClickDelegate?.Invoke();
    }
}
