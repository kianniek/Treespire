using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FEEntryUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // references
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject selectedBackground;
    [SerializeField] private GameObject hoverBackground;

    // reference to the window and program
    private FileExplorer fileExplorerRef;
    private FileExplorerWindowUI fileExplorerWindowRef;

    // keep track of selected (clicked once)
    private bool isSelected;

    // current entry this UI element is displaying
    private FEEntry entry;

    // getters for info about current entry
    internal string Name { get { return entry.name; } }
    internal string Path { get { return entry.path; } }
    internal bool IsDirectory { get { return entry.isDirectory; } }
    internal bool IsLockedFolder { get; private set; }

    // getter for quick acces folders
    // those CHANGE the path instead of ADDING to the path
    internal bool IsQuickAccessFolder { get { return IsDirectory &&
                transform.parent == fileExplorerWindowRef.quickAccessItemParent; } }

    /// <summary>
    /// Call to initialize this UI entry, ready for use.
    /// </summary>
    /// <param name="fileExplorerWindowRef">Reference to the window this entry is in</param>
    internal void Initialize(FileExplorer fileExplorerRef, FileExplorerWindowUI fileExplorerWindowRef)
    {
        // store ref to window and program
        this.fileExplorerRef = fileExplorerRef;
        this.fileExplorerWindowRef = fileExplorerWindowRef;
    }

    /// <summary>
    /// Activates this and sets it up according to entry.
    /// </summary>
    /// <param name="entry">Entry that corresponds</param>
    internal void Activate(FEEntry entry)
    {
        // reset for new file
        Select(false);
        hoverBackground.SetActive(false);

        // deactivate and return if entry is not provided
        if (entry.Equals(default(FEEntry)))
        {
            gameObject.SetActive(false);
            return;
        }

        // determine whether this is a locked folder
        IsLockedFolder = fileExplorerRef.IsLockedFolder(entry.path);

        // set icon and text
        icon.sprite = fileExplorerRef.GetIconForEntry(entry);
        title.text = entry.name;

        // add a sneaky ':' after C to make it look more real (:
        if (title.text == "C")
            title.text += ":";

        // keep reference to current entry
        this.entry = entry;

        // move in order
        transform.SetAsLastSibling();

        // and activate
        gameObject.SetActive(true);
    }

    #region INTERACTIONS
    /// <summary>
    /// Selects this entry.
    /// </summary>
    /// <param name="select">Whether it is selected or deselected</param>
    internal void Select(bool select)
    {
        // already selected, nothing to do
        if (select == isSelected)
            return;

        // (de)select self by turning on/off a visual
        isSelected = select;
        selectedBackground.SetActive(isSelected);
    }

    /// <summary>
    /// Called on pointer click.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        fileExplorerWindowRef.OnEntrySelected(this, eventData.clickCount == 2);
    }

    /// <summary>
    /// Called on pointer enter.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverBackground.SetActive(true);
    }

    /// <summary>
    /// Called on pointer exit.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        hoverBackground.SetActive(false);
    }
    #endregion
}