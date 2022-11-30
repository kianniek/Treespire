using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Program : ScriptableObject
{
    // settings for the program
    [SerializeField] internal string programName;
    [SerializeField] internal Sprite iconSprite;
    [SerializeField] internal bool hasDesktopShortcut;
    [SerializeField] private bool hasStartMenuShortcut;

    // references to shorcuts and taskbar icon
    private ShortcutUI desktopShortcut;
    private ShortcutUI startMenuShortcut;
    private TaskbarIconUI taskBarIcon;

    // reference to the window that belongs to this program
    // can be null if this program only uses system windows or none at all
    protected WindowUI window;

    // variables to keep track of visibility state while running
    internal bool inFocus;
    internal bool minimized;

    // whether we are running rn
    private bool running;

    // path to its 'program file'
    // cannot (or rather won't) run if program file is deleted 
    // but program file will be regenerated on start up
    internal string programFilePath { get; private set; }

    // path it its shortcut file on the desktop
    internal string desktopShortcutPath { get; private set; }

    /// <summary>
    /// Initializes the program.
    /// </summary>
    internal virtual void Initialize()
    {
        // create a taskbar icon to show when program is running
        taskBarIcon = GameManager.instance.CreateTaskbarIcon(this);
        taskBarIcon.gameObject.SetActive(false);

        // if this program has shortcuts, create em and store a reference
        if (hasDesktopShortcut)
            desktopShortcut = GameManager.instance.CreateDesktopShortcut(this);
        if(hasStartMenuShortcut)
            startMenuShortcut = GameManager.instance.CreateStartMenuShortcut(this);

        // start: not running
        running = false;
    }

    /// <summary>
    /// Called on start of program.
    /// </summary>
    internal virtual bool StartUp() 
    {
        // if this program file doesn't exist anymore, 
        // throw an error and return
        if (!ProgramFileExists())
        {
            ThrowProgramFileDoesntExistsErrror();
            return false;
        }

        // if we're already running, just focus this program
        if (running)
        {
            Focus(true);
            return false;
        }

        // started, so running!
        running = true;

        // add this to the active programs
        GameManager.instance.OnProgramStartUp(this);
     
        // activate the taskbar icon and position correctly
        taskBarIcon.gameObject.SetActive(true);
        taskBarIcon.transform.SetSiblingIndex(1);
        
        // open the window if we have any
        if(window != null)
            window.Open();

        // setup variables for just having started
        minimized = false;
        inFocus = false;
        Focus(true);

        // succesfully started
        return true;
    }

    /// <summary>
    /// Called every frame on active programs.
    /// </summary>
    internal virtual bool Update() 
    {
        // check to see if program file still exists 
        if (!ProgramFileExists())
        {
            // if it doesn't, throw error and return a failed update
            ThrowProgramFileDoesntExistsErrror();
            return false;
        }

        //succesfully updated
        return true;
    }

    /// <summary>
    /// Called to shut the program down.
    /// </summary>
    internal virtual void ShutDown()
    {
        // nothing to shutdown if this isn't running
        if (!running)
            return;

        // not running anymore
        running = false;

        // remove this from the active programs
        GameManager.instance.OnProgramShutDown(this);

        // make sure related window is closed
        window?.Close();

        // deactivate the taskbar icon
        taskBarIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called to shut program down with a delay.
    /// </summary>
    /// <param name="delay">Delay until shutdown</param>
    /// <returns></returns>
    internal IEnumerator ShutDownDelayed(float delay)
    {
        // wait out the delay
        yield return new WaitForSecondsRealtime(delay);

        // call a normal shutdown after delay
        ShutDown();
    }

    /// <summary>
    /// Brings the program in or out focus
    /// </summary>
    /// <param name="inFocus">Whether it is in focus</param>
    internal virtual void Focus(bool inFocus) 
    {
        // already in correct focus state? return
        if (this.inFocus == inFocus)
            return;

        // save focus state
        this.inFocus = inFocus;

        // if we are in focus now,
        if (inFocus)
        {
            // bring it to front and update to the GM
            window?.BringToFront();
            GameManager.instance.OnProgramFocus(this);

            // and if it's minimized, deminimize it
            if (minimized)
                Minimize(false);
        }   

        // make the taskbar icon match the focus state
        taskBarIcon.ProgramInFocus(inFocus);       
    }

    /// <summary>
    /// Toggles minimize state of this program.
    /// </summary>
    internal void ToggleMinimize()
    {
        // toggle minimize to the opposite value
        Minimize(!minimized);
    }

    /// <summary>
    /// Minimizes the program.
    /// </summary>
    /// <param name="minimized">Whether the program is minimzed</param>
    internal virtual void Minimize(bool minimized) 
    {
        // already in correct minimzed state? return
        if (this.minimized == minimized)
            return;

        // save minimized state
        this.minimized = minimized;

        // bring the program in/out focus
        if (minimized)
            Focus(false);
        else
            Focus(true);

        // if we have a window, also make it aware of minimized state
        window?.Minimize(minimized);
    }

    /// <summary>
    /// Sets the reference to the program files path for this program.
    /// </summary>
    /// <param name="programFilePath">The path to the program file</param>
    internal void SetProgramFilePath(string programFilePath)
    {
        this.programFilePath = programFilePath;
    }

    /// <summary>
    /// Sets the reference to the desktop shortcut path for this program.
    /// </summary>
    /// <param name="programFilePath">The path to the desktop shortcut</param>
    internal void SetDesktopShortcutPath(string desktopShortcutPath)
    {
        this.desktopShortcutPath = desktopShortcutPath;
    }

    /// <summary>
    /// Checks whether the program file exists.
    /// </summary>
    /// <returns>True if the file exists</returns>
    internal bool ProgramFileExists()
    {
        return System.IO.File.Exists(programFilePath);
    }

    /// <summary>
    /// Checks whether the desktop shortcut exists.
    /// </summary>
    /// <returns>True if the shortcut exists</returns>    
    internal bool DesktopShortcutExists()
    {
        return System.IO.File.Exists(desktopShortcutPath);
    }

    /// <summary>
    /// Called when program starts up but can't find its program file.
    /// Opens the error window and shuts down the program.
    /// </summary>
    private void ThrowProgramFileDoesntExistsErrror()
    {
        // set the error window
        GameManager.instance.errorWindow.Open();
        GameManager.instance.errorWindow.SetText(GameManager.instance.programFileNotFoundError);
        GameManager.instance.errorWindow.SetButtonDelegates(null);

        // shut this down
        ShutDown();
    }
}
