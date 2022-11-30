using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FileExplorerWindowUI : WindowUI
{
    // prefab to spawn as file items
    [SerializeField] private GameObject fileItemPrefab;

    // parent to spawn the file item prefabs under
    [SerializeField] private Transform fileItemParent;

    // parent to spawn the file items prefabs for quick access menu under
    [SerializeField] internal Transform quickAccessItemParent;

    // navigation buttons
    [SerializeField] private Button backButton;
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button upButton;

    // input field for path
    [SerializeField] private TMP_InputField pathInputfield;

    // reference to the program
    private FileExplorer fileExplorerRef;

    // variables to keep track of the file items
    private List<FEEntryUI> fileEntries;
    private Stack<FEEntryUI> pooledFileEntries;
    private FEEntryUI[] quickAccessEntries;
    private FEEntryUI selectedFileEntry;

    internal override void Initialize(Program program = null)
    {
        // call base
        base.Initialize(program);

        // store casted ref to program
        fileExplorerRef = (FileExplorer)program;

        // prepare pool with file items
        fileEntries = new List<FEEntryUI>();
        pooledFileEntries = new Stack<FEEntryUI>();

        // display given paths as quick entries
        SetQuickAccessEntries();
    }

    internal override void Open()
    {
        // call base
        base.Open();

        // set title
        title.text = program.programName;

        // disable all buttons to start with
        // will be activated if valid for current path
        backButton.interactable = false;
        forwardButton.interactable = false;
        upButton.interactable = false;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        // 'ENTER' input as double click on selected when input field isn't selected
        if (selectedFileEntry != null && Input.GetKeyUp(KeyCode.Return) 
            && !EventSystem.current.currentSelectedGameObject == pathInputfield)
        {
            OnEntrySelected(selectedFileEntry, true);
            return;
        }
            
        // back and forward with mouse 
        if(Input.GetKeyUp(KeyCode.Mouse3))
        {
            fileExplorerRef.GoBack();
            return;
        }
        if (Input.GetKeyUp(KeyCode.Mouse4))
        {
            fileExplorerRef.GoForward();
            return;
        }
    }

    #region FILES
    /// <summary>
    /// Set the entries shown in the quick access bar.
    /// </summary>
    internal void SetQuickAccessEntries()
    {
        // as many entries as there are quick access folders
        quickAccessEntries = new FEEntryUI[fileExplorerRef.quickAccessFolders.Length];

        // define entry once
        FEEntry entry;

        // for each folder
        for (int i = 0; i < fileExplorerRef.quickAccessFolders.Length; i++)
        {
            // create a new entry gameobject and initialize it
            quickAccessEntries[i] = Instantiate(fileItemPrefab, quickAccessItemParent).GetComponent<FEEntryUI>();
            quickAccessEntries[i].Initialize(fileExplorerRef, this);

            // get the entry data from the path
            entry.path = fileExplorerRef.quickAccessFolders[i];
            entry.name = fileExplorerRef.GetDirectoryName(entry.path);
            entry.isDirectory = fileExplorerRef.IsDirectory(entry.path);
            entry.extension = string.Empty;

            // and activate the entry with that data
            quickAccessEntries[i].Activate(entry);
        }
    }

    /// <summary>
    /// Displays file explore entries in UI according to given entries.
    /// </summary>
    /// <param name="entries">Entries to display</param>
    internal void RefreshFiles(List<FEEntry> entries)
    {
        // if there are active files, clear them
        if (fileEntries.Count > 0)
        {
            for (int i = 0; i < fileEntries.Count; i++)
            {
                // deactivate 
                fileEntries[i].gameObject.SetActive(false);

                // put back in pool and remove from active
                pooledFileEntries.Push(fileEntries[i]);
                fileEntries.RemoveAt(i);

                // go back one since this one is just removed
                i--;
            }
        }

        // get and activate a new file item
        // for each given entry
        FEEntryUI newFileItem;
        for (int i = 0; i < entries.Count; i++)
        {
            newFileItem = GetFileEntryObject();
            newFileItem.Activate(entries[i]);
        }
    }

    /// <summary>
    /// Get a file entry, either new or obtained from pool
    /// </summary>
    /// <returns>A usable file entry</returns>
    private FEEntryUI GetFileEntryObject()
    {
        // declare the entry
        FEEntryUI fileEntry;

        // either get one from pool or create a new one
        if(pooledFileEntries.Count > 0)
        {
            // pop it and activate it
            fileEntry = pooledFileEntries.Pop();
            fileEntry.gameObject.SetActive(true);
        }
        else
        {
            // instantiate it and initialize it 
            fileEntry = Instantiate(fileItemPrefab, fileItemParent).GetComponent<FEEntryUI>();
            fileEntry.Initialize(fileExplorerRef, this);
        }

        // add it to the active items
        fileEntries.Add(fileEntry);

        // return it 
        return fileEntry;
    }
    #endregion

    #region INPUTFIELD
    /// <summary>
    /// Called when program made a change in the path, 
    /// sets the text in the input field to match.
    /// </summary>
    /// <param name="path">The new path to display</param>
    internal void SetPathInputfield(string path)
    {
        pathInputfield.text = path;
    }

    /// <summary>
    /// Called when done editing the input field
    /// </summary>
    public void OnPathChanged()
    {
        fileExplorerRef.SetCurrentPath(pathInputfield.text, false, true);
    }
    #endregion

    #region INTERACTIONS
    /// <summary>
    /// Called when an entry is selected.
    /// </summary>
    /// <param name="entry">The selected entry</param>
    /// <param name="isDoubleClick">Whether the entry was double clicked</param>
    internal void OnEntrySelected(FEEntryUI entry, bool isDoubleClick)
    {
        OnClick();

        // if this entry is already the selected one AND this is not a double click
        // deselect and return early
        if (selectedFileEntry == entry && !isDoubleClick)
        {
            selectedFileEntry.Select(false);
            selectedFileEntry = null;
            return;
        }

        // deselect the currently selected if there is any
        if (selectedFileEntry != null)
            selectedFileEntry.Select(false);

        // remember the newly selected
        selectedFileEntry = entry;

        // nothing new to select or resolve, return
        if (entry == null)
            return;

        // on click: select
        // on double click: interact
        if (!isDoubleClick)
        {
            // select the new item
            entry.Select(true);
        }
        else
        {
            // resolve interaction based on whether the entry is a directory
            if (entry.IsDirectory)
            {
                // if it's a directory, enter it
                if (entry.IsQuickAccessFolder)
                    fileExplorerRef.SetCurrentPath(entry.Path);
                else
                    fileExplorerRef.AddToCurrentPath(entry.Name);
            }
            else
            {
                // for any file, just throw an error that the user isn't allowed to open it
                GameManager.instance.errorWindow.Open();
                GameManager.instance.errorWindow.SetText(fileExplorerRef.accessDeniedToFileError);
                GameManager.instance.errorWindow.SetButtonDelegates(null);
            }
        }
    }

    /// <summary>
    /// Updates the interactable status of the navigation buttons.
    /// </summary>
    /// <param name="backActive">Whether the back button is active.</param>
    /// <param name="forwardActive">Whether the forward button is active.</param>
    /// <param name="upActive">Whether the up button is active.</param>
    internal void ActivateNavigationButtons(bool backActive, bool forwardActive, bool upActive)
    {
        // set the interactable state of each button
        backButton.interactable = backActive;
        forwardButton.interactable = forwardActive;
        upButton.interactable = upActive;
    }

    /// <summary>
    /// Called on back button clicked.
    /// </summary>
    public void ClickBack()
    {
        OnClick();

        fileExplorerRef.GoBack();
    }

    /// <summary>
    /// Called on forward button clicked.
    /// </summary>
    public void ClickForward()
    {
        OnClick();

        fileExplorerRef.GoForward();
    }

    /// <summary>
    /// Called on up button clicked.
    /// </summary>
    public void ClickUp()
    {
        OnClick();

        fileExplorerRef.GoUp();
    }

    /// <summary>
    /// Called on refresh button clicked.
    /// </summary>
    public void ClickRefresh()
    {
        OnClick();

        fileExplorerRef.Refresh();
    }
    #endregion
}
