using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References for desktop")]
    [SerializeField] private DesktopUI desktop;
    [SerializeField] private Transform desktopShortcutParent;
    [SerializeField] private GameObject desktopShortcutPrefab;

    [Header("References for start menu")]
    [SerializeField] internal StartMenuUI startMenu;
    [SerializeField] private Transform startMenuShortcutParent;
    [SerializeField] private GameObject startMenuShortcutPrefab;

    [Header("References for task bar")]
    [SerializeField] internal Transform taskbarParent;
    [SerializeField] private Transform taskbarIconParent;
    [SerializeField] private GameObject taskbarIconPrefab;

    [Header("References for windows")]
    [SerializeField] internal Transform windowParent;
    [SerializeField] internal DecisionWindowUI decisionWindow;
    [SerializeField] internal ErrorWindowUI errorWindow;

    [Header("Cursor references")]
    [SerializeField] internal RectTransform cursorRect;
    [SerializeField] internal RectTransform cursorTrailParent;
    [SerializeField] internal CursorUI cursor;

    [Header("Other references")]
    [SerializeField] internal RectTransform canvasRect;
    [SerializeField] internal ClickDetector clickDetector;
    [SerializeField] internal AudioManager audioManager;
    [SerializeField] internal VolumeUI volumeMenu;

    [Header("Maximized window settings")]
    [SerializeField] internal Vector2 windowMinOffsetMaximized;
    [SerializeField] internal Vector2 windowMaxOffsetMaximized;

    [Header("Programs in this OS")]
    [SerializeField] internal List<Program> programs;

    [Header("Errors")]
    [SerializeField] internal ErrorDefinition programCantRunError;
    [SerializeField] internal ErrorDefinition programFileNotFoundError;

    // often used anchors, define once
    internal Vector2 bottomLeftAnchor = new Vector2(0, 0);
    internal Vector2 bottomCenterAnchor = new Vector2(0.5f, 0);
    internal Vector2 centerAnchor = new Vector2(0.5f, 0.5f);
    internal Vector2 topRightAnchor = new Vector2(1, 1);
    internal Vector2 topLeftAnchor = new Vector2(1, 0);

    // paths variables
    #region PATHS
    internal const string SLASH = "/";
    internal const string BACK_SLASH = "\\"; 
    
    internal string GetStreamingAssetsPath { get { return Application.streamingAssetsPath; } }
    internal const string MY_COMPUTER_PATH = "My Computer";
    internal const string C_DRIVE_PATH = "C";

    internal string GetRootExplorerPath { get { return MY_COMPUTER_PATH + SLASH + C_DRIVE_PATH; } }

    internal string GetProgramFilesPath { get { return GetRootExplorerPath + SLASH + "Program Files"; } }
    internal string GetAdminUserPath { get { return GetRootExplorerPath + SLASH + "Users/Admin"; } }
    internal string GetGuestUserPath { get { return GetRootExplorerPath + SLASH + "Users/Guest"; } }

    internal string GetDesktopPath { get { return GetGuestUserPath + SLASH + "Desktop/"; } }
    internal string GetDocumentsPath { get { return GetGuestUserPath + SLASH + "Documents/"; } }
    internal string GetImagesPath { get { return GetGuestUserPath + SLASH + "Images/"; } }
    internal string GetMusicPath { get { return GetGuestUserPath + SLASH + "Music/"; } }
    #endregion

    // lazy singleton
    public static GameManager instance;

    // variables to keep track of active programs
    private List<Program> activePrograms;
    internal Program focusProgram { get; private set; }

    /// <summary>
    /// Called when script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // lazy singleton
        instance = this;

        // initialize audiomanager
        audioManager.Initialize();

        // initialize desktop
        desktop.Initialize();

        // initialize volume menu
        volumeMenu.Initialize();

        // initialize list for active programs
        activePrograms = new List<Program>();

        // initialize all existing programs
        programs.ForEach(p => p.Initialize());

        // initialize system windows
        decisionWindow.Initialize();
        errorWindow.Initialize();

        // DEBUG CODE: to make tree and save for default folder hierarchy
        // NOTE: this hierarchy will be restored to the best of the programs ability on startup
        /*Tree<string> folderHierarchyTree = new Tree<string>(new Node<string>("My Computer", new Node<string>[]
        {
             new Node<string>("C", new Node<string>[]
             {
                 new Node<string>("Program Files"),
                 new Node<string>("Users", new Node<string>[]
                 {
                     new Node<string>("Admin"),
                     new Node<string>("Guest", new Node<string>[]
                     {
                         new Node<string>("Desktop"),
                         new Node<string>("Documents", new Node<string>[]
                         {
                             new Node<string>("20Questions")
                         }),
                         new Node<string>("Images"),
                         new Node<string>("Music")
                     })
                 })
             })
        }));

        XMLSerializer.Serialize(folderHierarchyTree, GetStreamingAssetsPath + SLASH + GetAdminUserPath + SLASH + "fileExplorerHierarchy.xml");*/
    }

    /// <summary>
    /// Called just before any update is called for the first time.
    /// </summary>
    private void Start()
    {
        // play boot sound
        audioManager.PlaySound(AudioManager.Sound.Type.StartUp);
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        // no update if the application isn't focused
        if (!Application.isFocused)
            return;

        // update active programs
        for (int i = 0; i < activePrograms.Count; i++)
        {
            // if the update returned false, 
            // program closed itself during update
            // and with that removed itself from this list
            if (!activePrograms[i].Update())
                i--;
        }
    }

    #region PROGRAM_MANAGEMENT
    /// <summary>
    /// Finds and returns a program by name.
    /// </summary>
    /// <param name="programName">Name of the program to get</param>
    /// <returns>Program if any was found</returns>
    internal Program GetProgram(string programName)
    {
        // find the program with matching name and return it
        return programs.Find(p => p.programName == programName);
    }

    /// <summary>
    /// Called when program starts up.
    /// </summary>
    /// <param name="program">The started program</param>
    internal void OnProgramStartUp(Program program)
    {
        // return if it's already in our active programs
        if (activePrograms.Contains(program))
        {
            Debug.Log("Program " + program.programName + " already started.");
            return;
        }

        // add it to active programs to keep track
        activePrograms.Add(program);
    }

    /// <summary>
    /// Called when program shuts down.
    /// </summary>
    /// <param name="program">The shut down program</param>
    internal void OnProgramShutDown(Program program)
    {
        // return if it's not in active programs
        if (!activePrograms.Contains(program))
        {
            Debug.Log("Program " + program.programName + " not started yet.");
            return;
        }

        // remove it from active programs
        activePrograms.Remove(program);
    }

    /// <summary>
    /// Called when program is focussed.
    /// </summary>
    /// <param name="program">The program in focus</param>
    internal void OnProgramFocus(Program program)
    {
        // already the focus, return
        if (program == focusProgram)
            return;

        // if other is in focus, 
        // put program out of focus
        if (focusProgram != null)
            focusProgram.Focus(false);

        // keep track of the new focussed window
        focusProgram = program;
    }

    /// <summary>
    /// Creates a shortcut on the desktop.
    /// </summary>
    /// <param name="program">Program the shortcut refers to</param>
    /// <returns>Newly created shortcut</returns>
    internal ShortcutUI CreateDesktopShortcut(Program program)
    {
        // create and setup new shortcut
        ShortcutUI newShortcut = GameObject.Instantiate(desktopShortcutPrefab, desktopShortcutParent).GetComponent<ShortcutUI>();
        newShortcut.Initialize(program, ShortcutUI.Type.Desktop);
        newShortcut.transform.SetAsLastSibling();
        desktop.AddShortcut(newShortcut);

        // return the newly created shortcut
        return newShortcut;
    }

    /// <summary>
    /// Creates a shortcut in the start menu.
    /// </summary>
    /// <param name="program">Program the shortcut refers to</param>
    /// <returns>Newly created shortcut</returns>
    internal ShortcutUI CreateStartMenuShortcut(Program program)
    {
        // create and setup new shortcut
        ShortcutUI newShortcut = GameObject.Instantiate(startMenuShortcutPrefab, startMenuShortcutParent).GetComponent<ShortcutUI>();
        newShortcut.Initialize(program, ShortcutUI.Type.StartMenu);
        newShortcut.transform.SetAsFirstSibling();

        // return the newly created shortcut
        return newShortcut;
    }

    /// <summary>
    /// Creates a taskbar icon.
    /// </summary>
    /// <param name="program">Program the taskbar icon refers to</param>
    /// <returns>Newly created taskbar icon</returns>
    internal TaskbarIconUI CreateTaskbarIcon(Program program)
    {
        // create and setup new taskbar icon
        TaskbarIconUI newTaskbarIcon = GameObject.Instantiate(taskbarIconPrefab, taskbarIconParent).GetComponent<TaskbarIconUI>();
        newTaskbarIcon.Initialize(program);
        newTaskbarIcon.transform.SetSiblingIndex(1);

        // return the newly created taskbar icon
        return newTaskbarIcon;
    }

    /// <summary>
    /// Brings the click detector to front, just behind the given window.
    /// </summary>
    /// <param name="clickDelegate">The callback on click on detector</param>
    /// <param name="behindTransform">The transform it should be behind</param>
    /// <param name="tempParentTransform">The transform it temporarily parents to</param>
    /// /// <param name="inFrontTaskBar">Whether the click detector should be in front of the taskbar. Often not the case</param>
    internal void BringClickDetectorToFront(WindowUI.ButtonDelegate clickDelegate, Transform behindTransform = null, Transform tempParentTransform = null, bool inFrontTaskBar = false)
    {
        // set the delegate
        clickDetector.SetOnClickDelegate(clickDelegate);

        // set the temp new parent if any is given
        if (tempParentTransform != null)
            clickDetector.transform.SetParent(tempParentTransform);

        // bring click detector to fron
        clickDetector.transform.SetAsLastSibling();

        // bring the transform one in front of the click detector
        // can be null, if only the clickdetector needs to move forward
        if (behindTransform != null)
            behindTransform.SetAsLastSibling();

        // check how to position the taskbar
        if (tempParentTransform != null && inFrontTaskBar)
        {
            taskbarParent.SetParent(tempParentTransform);
            taskbarParent.SetSiblingIndex(tempParentTransform.childCount - 3);
        }
        else if (!inFrontTaskBar)
        {
            taskbarParent.SetAsLastSibling();
        }
            
        // bring cursor to the front, since it should ALWAYS be above click 
        cursorRect.SetAsLastSibling();
        cursorTrailParent.SetAsLastSibling();
    }

    /// <summary>
    /// Brings the click detector back.
    /// </summary>
    internal void BringClickDetectorToBack(bool calledFromTaskbarMenu = false)
    {
        // reset parent, since that might have been changed
        clickDetector.transform.SetParent(transform);

        // move click detector all the way back
        clickDetector.transform.SetAsFirstSibling();

        // also reset the taskbar and set cursor over everything
        taskbarParent.SetParent(transform);
        taskbarParent.SetAsLastSibling();
        cursorRect.SetAsLastSibling();
        cursorTrailParent.SetAsLastSibling();

        // check to see if any other taskbar menus should be closed as well
        if (calledFromTaskbarMenu)
        {
            if (startMenu.IsOpen) startMenu.Close();
            if (volumeMenu.IsOpen) volumeMenu.Close();
        }
    }
    #endregion

    #region SHUTDOWN
    /// <summary>
    /// Shuts the application down with safety check.
    /// </summary>
    internal void PrepareShutDown()
    {
        // make sure to close system windows
        decisionWindow.Close();
        errorWindow.Close();

        // if programs are still active,
        // prompt whether user is sure before actually shutting down
        // else just shut down
        if (activePrograms.Count > 0)
        {
            decisionWindow.Open();
            decisionWindow.SetText("Are you sure?", "Programs are still running. Are you sure you want to shut down?");
            decisionWindow.SetButtonDelegates(delegate { ShutDownAllActivePrograms(); ShutDown(); }, null);
        }
        else
        {
            ShutDown();
        }
    }

    /// <summary>
    /// Actually shut down the application.
    /// </summary>
    private void ShutDown()
    {
        // quit application
        Application.Quit();
    }

    /// <summary>
    /// Shut down all active programs.
    /// </summary>
    private void ShutDownAllActivePrograms()
    {
        // go over all programs and close them
        for (int i = 0; i < activePrograms.Count; i++)
        {
            // shut down
            activePrograms[i].ShutDown();

            // go one back since it's removed from the list
            i--;
        }
    }
    #endregion
}
