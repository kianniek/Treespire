using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragableWindowComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // reference to the transform to drag
    [SerializeField] private RectTransform windowRect;

    // vectors for click positions during drag
    private Vector2 initialClickPos = Vector2.zero;
    private Vector2 clickPos = Vector2.zero;

    // extra bounds on sides of the canvas to keep the transform in
    private static float BORDER = 20f;

    // vectors to calculate position in bounds
    private Vector2 windowSize, windowHalfSize, clampedWindowPos;
    private Vector2 canvasSize, canvasHalfSize;

    /// <summary>
    /// Called on begin drag.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // position window rect based on
        // initial click position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(windowRect, eventData.pressPosition, eventData.pressEventCamera, out initialClickPos);
    }

    /// <summary>
    /// Called during drag.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        // position window rect based on
        // difference between initial click and current click position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(windowRect, eventData.position, eventData.pressEventCamera, out clickPos);
        windowRect.anchoredPosition += clickPos - initialClickPos;
    }

    /// <summary>
    /// Called on end drag.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // ensure window is in bounds
        EnsureWindowIsWithinBounds();
    }

    /// <summary>
    /// Keeps the windows position within the given borders.
    /// </summary>
    private void EnsureWindowIsWithinBounds()
	{
        // (re-)get vectors since they might change
        clampedWindowPos = windowRect.anchoredPosition;

        if (canvasSize != GameManager.instance.canvasRect.sizeDelta)
        {
            canvasSize = GameManager.instance.canvasRect.sizeDelta;
            canvasHalfSize = canvasSize * 0.5f;
        }

        if (windowSize != windowRect.sizeDelta)
        {
            windowSize = windowRect.sizeDelta;
            windowHalfSize = windowSize * 0.5f;
        }

        // clamp position vector, to make it  within bounds
        clampedWindowPos.x = Mathf.Clamp(clampedWindowPos.x, -canvasHalfSize.x + windowHalfSize.x + BORDER, canvasHalfSize.x - windowHalfSize.x - BORDER);
        clampedWindowPos.y = Mathf.Clamp(clampedWindowPos.y, -canvasHalfSize.y + windowHalfSize.y + BORDER, canvasHalfSize.y - windowHalfSize.y - BORDER);

        // position the window in bounds
        windowRect.anchoredPosition = clampedWindowPos;
    }
}
