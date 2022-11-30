using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class WindowUI : MonoBehaviour, IPointerDownHandler
{
    // delegate for button call backs
    internal delegate void ButtonDelegate();

    [Header("Scene references")]
    [SerializeField] protected TextMeshProUGUI title;
    [SerializeField] protected RectTransform rectTransform;

    // all buttons under this parent to enable / disable at once
    private List<Button> allButtons;

    // keep track of currently able to interact with this window
    protected bool interactable;

    // reference to the program
    internal Program program;

    // start size to return to after maximizing
    private Vector2 originalSize;

    // bools for show-state
    internal bool minimized;
    internal bool maximized;

    /// <summary>
    /// Initializes the window.
    /// </summary>
    /// <param name="program">Optional program for this window. If null, it's a system window.</param>
    internal virtual void Initialize(Program program = null)
    {
        // keep a reference to given program
        this.program = program;

        // store the starting size of this rect
        originalSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        // find all buttons under this parent
        allButtons = new List<Button>(GetComponentsInChildren<Button>());
    }

    /// <summary>
    /// Opens the window.
    /// </summary>
    internal virtual void Open()
    {
        // activate self
        gameObject.SetActive(true);

        // position in center
        rectTransform.anchoredPosition = Vector2.zero;

        // reset variables for just started up
        minimized = false;
        maximized = false;

        // start interactable, force an update 
        EnableInteractions(true, true);

        // move to front
        BringToFront();
    }

    /// <summary>
    /// Closes the window.
    /// </summary>
    internal virtual void Close()
    {
        // reset all components
        title.text = string.Empty;

        // and disable self
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Brings the window back to front.
    /// </summary>
    internal void BringToFront()
    {
        // bring to front
        transform.SetAsLastSibling();
    }

    #region MINIMIZE AND MAXIMIZE
    /// <summary>
    /// Minimizes this window.
    /// </summary>
    /// <param name="minimized">Whether the window is minimized</param>
    internal virtual void Minimize(bool minimized)
    {
        if (this.minimized == minimized)
            return;

        this.minimized = minimized;

        gameObject.SetActive(!minimized);
    }

    /// <summary>
    /// Toggles between normal and maximized.
    /// </summary>
    internal void ToggleMaximize()
    {
        // call maximize with
        // the opposite of current maximize state
        Maximize(!maximized);
    }

    /// <summary>
    /// Maximizes this window.
    /// </summary>
    /// <param name="maximized">Whether the window is maximized</param>
    internal void Maximize(bool maximized)
    {
        // if this window is already in the desired state, 
        // return immediately
        if (this.maximized == maximized)
            return;

        // keep track of current maximized state
        this.maximized = maximized;

        // resolve the state
        if (maximized)
        {
            // maximize the window by setting the anchors and offsets to spread
            rectTransform.anchorMin = GameManager.instance.bottomLeftAnchor;
            rectTransform.anchorMax = GameManager.instance.topRightAnchor;
            rectTransform.offsetMin = GameManager.instance.windowMinOffsetMaximized;
            rectTransform.offsetMax = GameManager.instance.windowMaxOffsetMaximized;
        }
        else
        {
            // de-maximize the window by centering the anchors and restoring original size
            rectTransform.anchorMin = GameManager.instance.centerAnchor;
            rectTransform.anchorMax = GameManager.instance.centerAnchor;
            rectTransform.sizeDelta = originalSize;
        }
    }
    #endregion

    #region INTERACTIONS
    /// <summary>
    /// Call to enable or disable all interactions in this window.
    /// </summary>
    /// <param name="enable">Whether interaction should be en- or disabled</param>
    /// <param name="forced">Whether the update is forced</param>
    internal void EnableInteractions(bool enable, bool forced = false)
    {
        // no need to update if we're already in the correct state
        if (interactable == enable && !forced)
            return;

        // keep track of current interactable state
        interactable = enable;

        // en- or disable all buttons
        allButtons.ForEach(b => b.interactable = enable);
    }

    /// <summary>
    /// Called on minimized clicked.
    /// </summary>
    public void ClickMinimize()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        // call minimize on the program
        // or on self
        if (program != null)
            program.Minimize(true);
        else
            Minimize(true);
    }

    /// <summary>
    /// Called on maximized clicked.
    /// </summary>
    public void ClickMaximize()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        // toggle whether this window is maximized
        ToggleMaximize();
    }

    /// <summary>
    /// Called on close clicked.
    /// </summary>
    public void ClickClose()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        // call close on the program
        // or on self
        if (program != null)
            program.ShutDown();
        else
            Close();
    }

    protected void OnClick()
    {
        // return if not interactable
        if (!interactable)
            return;

        // focus the program if it exists
        // else bring this to front
        if (program != null)
        {
            if (GameManager.instance.focusProgram != program)
                program.Focus(true);
        }
        else
        {
            BringToFront();
        }
    }

    /// <summary>
    /// Called on click down on this window.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // call on click
        // also called from certain buttons interactions
        OnClick();
    }
    #endregion
}
