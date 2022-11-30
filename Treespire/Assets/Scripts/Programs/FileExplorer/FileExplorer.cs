using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[CreateAssetMenu(menuName = "Programs/File Explorer")]
public class FileExplorer : Program
{
    // reference to the audio player window prefab
    [SerializeField] private GameObject fileExplorerWindowPrefab;

    // reference to the instantiated audio player window object
    private FileExplorerWindowUI fileExplorerWindow;

    // sprites for icons
    [Space()]
    [SerializeField] private Sprite defaultFileSprite;
    [SerializeField] private Sprite defaultFolderSprite;
    [SerializeField] private List<FEIconDefinition.IconSpritePair> iconSpritePairs;
    [SerializeField] private List<FEIconDefinition.IconExtensionPair> iconExtensionPairs;
    [SerializeField] private List<FEIconDefinition.IconFolderNamePair> iconFolderNamePairs;

    // what extensions to exclude
    [Space()]
    [SerializeField] private string[] excludedExtensions;

    // quick access folder paths
    [Space()]
    [SerializeField] internal string[] quickAccessFolders;

    // folder paths with access blocked
    [Space()]
    [SerializeField] internal string[] lockedFolders;

    // error definitions
    [SerializeField] internal ErrorDefinition accessDeniedToFileError;
    [SerializeField] internal ErrorDefinition accessDeniedToFolderError;
    [SerializeField] internal ErrorDefinition pathNotFoundError;

    // keep track of current path 
    private string currentPath;

    // list to keep track of paths followed to get to current path
    // for back and forward functionality
    private int currentPathIndex = -1;
    private readonly List<string> pathsFollowed = new List<string>();

    // all file entries in current path
    private List<FEEntry> allFileEntries;

    // variations of the current path for file access and for UI
    // since the 'my computer' environment is faked in a folder
    private string CurrentPathAsShown { get { return SwitchSlashes((currentPath + GameManager.SLASH).Replace("/C/", "/C:/"), GameManager.BACK_SLASH); } }
    private string ChangeDriveInPath(string path, bool withColon) { if (withColon) return path.Replace("/C/", "/C:/"); else return path.Replace("/C:/", "/C/"); }
    private string TotalLocalCurrentPath { get { return GameManager.instance.GetStreamingAssetsPath + GameManager.SLASH + currentPath; } }
    private string TotalLocalPath(string path) { return GameManager.instance.GetStreamingAssetsPath + GameManager.SLASH + path; }
    private string FullToPartialPath(string fullPath) { return fullPath.Substring(fullPath.IndexOf(GameManager.MY_COMPUTER_PATH)); }
    private string PartialToFullPath(string partialPath) { return GetCaseSensitivePath(new DirectoryInfo(TotalLocalPath(ChangeDriveInPath(SwitchSlashes(partialPath, GameManager.SLASH), false))).FullName); }

    // the path to start at on startup
    private string DefaultPath { get { return GameManager.MY_COMPUTER_PATH + GameManager.SLASH; } }

    // path to folder hierarchy tree stored in file
    internal string GetFolderHierarchyFilePath { get { return GameManager.instance.GetStreamingAssetsPath + GameManager.SLASH + GameManager.instance.GetAdminUserPath + GameManager.SLASH + "fileExplorerHierarchy.xml"; } }

    internal override void Initialize()
    {
		// call base
		base.Initialize();

        // instantiate the prefab window
        fileExplorerWindow = Instantiate(fileExplorerWindowPrefab, GameManager.instance.windowParent).GetComponent<FileExplorerWindowUI>();

        // setup the window
        fileExplorerWindow.Initialize(this);
        fileExplorerWindow.Close();

        // programs window is same as game window
        // but a different type
        window = fileExplorerWindow;

        // make sure the 'normal' folder structure (stored in a tree)
        // is restored, in case someone messed with it!
        
        // start by getting the tree with the folder structure
        Tree<string> folderHierarchy;
        if (File.Exists(GetFolderHierarchyFilePath))
        {
            // and if the tree exists
            folderHierarchy = XMLSerializer.Deserialize<Tree<string>>(GetFolderHierarchyFilePath);
            if (folderHierarchy != null && folderHierarchy.rootNode != null)
            {
                // call recursive method that checks for all nodes (starting at the root)
                // if the corresponding directory exists, if not it will create it
                EnsureDirectoriesExistsRecursive(folderHierarchy.rootNode, string.Empty);
            }
            else
            {
                // if not, throw an error. this can be problematic when starting / running programs.
                Debug.LogError("Couldn't load original folder stucture definition. " +
                    "Folder structure might diverse from known structure, which can cause errors.");
            }
        }

        // create - each program the GameManager knows -
        // a file in Program Files folder and one in Desktop folder
        // or override if it already exists from a previous time 
        // NOTE: the corresponding program will throw an error if the file is deleted
        for (int i = 0; i < GameManager.instance.programs.Count; i++)
            CreateFilesForProgram(GameManager.instance.programs[i]); 
    }

    /// <summary>
    /// Call with root node of a folder hierarchy to ensure all
    /// corresponding folders exist. They are created if required.
    /// Starts at the root node and calls itself recursively with 
    /// child nodes.
    /// </summary>
    /// <param name="node">Given node</param>
    private void EnsureDirectoriesExistsRecursive(Node<string> node, string previousPath)
    {
        // get paths
        string partialPath;
        if (!string.IsNullOrEmpty(previousPath))
            partialPath = previousPath + GameManager.SLASH + node.content;
        else
            partialPath = node.content;
        string path = PartialToFullPath(partialPath);

        // create a directory if it doesn't exist
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // if this is a leaf node, return since no more nodes
        if (node.childNodes == null)
            return;

        // continue recursive call in pre-order
        // so call first child, which calls all it's childern
        // then call the second child, etc.
        for (int i = 0; i < node.childNodes.Length; i++)
            EnsureDirectoriesExistsRecursive(node.childNodes[i], partialPath);
    }

    /// <summary>
    /// Creates or overrides a file for the program files folder and 
    /// if required one for the desktop shortcuts.
    /// </summary>
    /// <param name="program">The program to create files for</param>
    private void CreateFilesForProgram(Program program)
    {
        string tempPath;

        try
        {
            // determine full path for program files
            tempPath = PartialToFullPath(GameManager.instance.GetProgramFilesPath +
                GameManager.SLASH + program.programName);

            // create the file
            File.Create(tempPath);

            // give the correct program this path 
            program.SetProgramFilePath(tempPath);

            // and do the same for desktop file (shortcut), 
            // but only if the progam actually has a shorcute
            if (program.hasDesktopShortcut)
            {
                tempPath = PartialToFullPath(GameManager.instance.GetDesktopPath +
                    GameManager.SLASH + program.programName);
                File.Create(tempPath);
                program.SetDesktopShortcutPath(tempPath);
            }
        }
        catch (Exception) { }
    }

    internal override bool StartUp()
    {
        // clear path history
        currentPathIndex = -1;
        pathsFollowed.Clear();

        // call base
        if (!base.StartUp())
            return false;

        // set current path
        SetCurrentPath(DefaultPath, true);

        // succesfully started 
        return true;
    }

    #region CHANGE PATH
    /// <summary>
    /// Sets a new current path, loads it as well.
    /// </summary>
    /// <param name="newPath">The new path to go to</param>
    /// <param name="forced">Whether an update is forced</param>
    /// <param name="userInput">Whether the input comes from the user</param>
    internal void SetCurrentPath(string newPath, bool forced = false, bool userInput = false)
    {
        // trim the last '/' and empty space away
        if (newPath != null)
        {
            newPath = newPath.Trim();
            newPath = newPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // if the new path is nothing, return to current path
        if (string.IsNullOrEmpty(newPath))
        {
            fileExplorerWindow.SetPathInputfield(CurrentPathAsShown);
            return;
        }

        // user input should be casted since ... issues with
        // C: vs C, caps, local path vs resources path vs full path
        if (userInput)
            newPath = SwitchSlashes(FullToPartialPath(PartialToFullPath(newPath)),GameManager.SLASH);

        // need to update if there is a change!
        if (currentPath != newPath || forced)
        {
            // if the directory doesn't exists, set back and return
            if (!DirectoryExists(newPath))
            {
                fileExplorerWindow.SetPathInputfield(CurrentPathAsShown);
                return;
            }

            // if the directory is a locked folder, throw error and return
            if (IsLockedFolder(newPath))
            {
                GameManager.instance.errorWindow.Open();
                GameManager.instance.errorWindow.SetText(accessDeniedToFolderError);
                GameManager.instance.errorWindow.SetButtonDelegates(null);

                fileExplorerWindow.SetPathInputfield(CurrentPathAsShown);
                return;
            }

            // update, the path is valid
            currentPath = newPath;
            fileExplorerWindow.SetPathInputfield(CurrentPathAsShown);

            // add to paths followed if required
            if (currentPathIndex == -1 || pathsFollowed[currentPathIndex] != currentPath)
            {
                currentPathIndex++;
                if (currentPathIndex < pathsFollowed.Count)
                {
                    pathsFollowed[currentPathIndex] = newPath;
                    for (int i = pathsFollowed.Count - 1; i >= currentPathIndex + 1; i--)
                        pathsFollowed.RemoveAt(i);
                }
                else
                    pathsFollowed.Add(currentPath);
            }

            // determine whether buttons should be active
            fileExplorerWindow.ActivateNavigationButtons(currentPathIndex > 0,
                currentPathIndex < pathsFollowed.Count - 1,
                ParentDirectoryExists(currentPath));

            // visual refresh of file list
            RefreshFiles();
        }
    }

    /// <summary>
    /// Adds a partial path to the current path and sets the new path.
    /// </summary>
    /// <param name="addedPath">The partial path to add</param>
    internal void AddToCurrentPath(string addedPath)
    {
        SetCurrentPath(currentPath + GameManager.SLASH + addedPath);
    }

    /// <summary>
    /// (Re)loads the files from disk and refreshes the UI.
    /// </summary>
    private void RefreshFiles()
    {
        // if there is a current path, load the file entries
        if (!string.IsNullOrEmpty(currentPath))
            allFileEntries = GetEntriesInPath(currentPath);
        else
            allFileEntries = null;

        // sort the files and folders in order:
        // 1. directories come before files
        // 2. directories and files are sorted by their names
        allFileEntries.Sort((a, b) =>
        {
            if (a.isDirectory != b.isDirectory)
                return a.isDirectory ? -1 : 1;
            else
                return a.name.CompareTo(b.name);
        });

        // update UI list of items
        fileExplorerWindow.RefreshFiles(allFileEntries);
    }
    #endregion

    #region EVENTS
    /// <summary>
    /// Call to go back in followed paths.
    /// </summary>
    internal void GoBack()
    {
        // if there is a path to go back to,
        // go back one in followed paths
        if (currentPathIndex > 0)
            SetCurrentPath(pathsFollowed[--currentPathIndex]);
    }

    /// <summary>
    /// Call to go forward in followed paths.
    /// </summary>
    internal void GoForward()
    {
        // if there is a path to go forward to,
        // go forward one in followed paths
        if (currentPathIndex < pathsFollowed.Count - 1)
            SetCurrentPath(pathsFollowed[++currentPathIndex]);
    }

    /// <summary>
    /// Call to go up in directory structure.
    /// </summary>
    internal void GoUp()
    {
        // attempt to get a parent directory
        DirectoryInfo parentPath = Directory.GetParent(TotalLocalCurrentPath);

        // if we found one
        if (parentPath != null)
        {
            // modify the path name so that
            // the 'real' desktop part is dropped,
            // since our computer starts at MY_COMPUTER_PATH
            // and switch the slashes
            string modifiedPath = SwitchSlashes(FullToPartialPath(parentPath.FullName), GameManager.SLASH);
            SetCurrentPath(modifiedPath);
        }
    }

    /// <summary>
    /// Call to refresh the file entries.
    /// </summary>
    internal void Refresh()
    {
        RefreshFiles();
    }
    #endregion

    #region HELPERS
    /// <summary>
    /// Get the matching icon for given entry.
    /// </summary>
    /// <param name="entry">The entry to find the icon for</param>
    /// <returns>The matching icon</returns>
    internal Sprite GetIconForEntry(FEEntry entry)
    {
        // find either a folder or file icon and return it
        if (entry.isDirectory)
            return GetFolderIcon(entry);
        else
            return GetFileIcon(entry);
    }

    /// <summary>
    /// Get the matching file icon for given entry.
    /// </summary>
    /// <param name="entry">The entry to find the icon for</param>
    /// <returns>The matching icon</returns>
    private Sprite GetFileIcon(FEEntry entry)
    {
        // find icon extension pair for this extension
        FEIconDefinition.IconExtensionPair iePair = iconExtensionPairs.Find(p => p.extensions.Contains(entry.extension));

        // return the default sprite if this extension wasn't found
        if (iePair.Equals(default(FEIconDefinition.IconExtensionPair)))
            return defaultFileSprite;

        // find icon sprite pair for this icon type
        FEIconDefinition.IconSpritePair isPair = iconSpritePairs.Find(p => p.iconType == iePair.iconType);

        // return default if no sprite was found,
        // else return correct sprite
        if (isPair.Equals(default(FEIconDefinition.IconSpritePair)))
            return defaultFileSprite;
        else
            return isPair.sprite;
    }

    /// <summary>
    /// Get the matching folder icon for given entry.
    /// </summary>
    /// <param name="entry">The entry to find the icon for</param>
    /// <returns>The matching icon</returns>
    private Sprite GetFolderIcon(FEEntry entry)
    {
        // define the icon folder pair
        FEIconDefinition.IconFolderNamePair ifPair;

        // check whether it's a locked folder, overrides other icons
        if (IsLockedFolder(entry.path))
        {
            // find pair for locked folders
            ifPair = iconFolderNamePairs.Find(p => p.iconType == FEIconDefinition.IconType.LockedFolder);
        }
        else
        {
            // find pair for this entry
            ifPair = iconFolderNamePairs.Find(p => p.folderName == entry.name);
        }

        // return the default sprite if this folder wasn't found
        if (ifPair.Equals(default(FEIconDefinition.IconExtensionPair)))
            return defaultFolderSprite;

        // find icon sprite pair for this icon type
        FEIconDefinition.IconSpritePair isPair = iconSpritePairs.Find(p => p.iconType == ifPair.iconType);

        // return default if no sprite was found,
        // else return correct sprite
        if (isPair.Equals(default(FEIconDefinition.IconSpritePair)))
            return defaultFolderSprite;
        else
            return isPair.sprite;
    }

    /// <summary>
    /// Get all file entries in the current path directory.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>All found file entries</returns>
    private List<FEEntry> GetEntriesInPath(string path)
    {
        // get all from current path
        FileSystemInfo[] items = new DirectoryInfo(TotalLocalPath(path)).GetFileSystemInfos();

        // store all found items in structs for further use
        List<FEEntry> result = new List<FEEntry>();
        for (int i = 0; i < items.Length; i++)
            result.Add(new FEEntry(items[i]));

        // if there are excluded extensions
        if (excludedExtensions.Length > 0)
        {
            // go over all files
            for (int i = 0; i < result.Count; i++)
            {
                // and compare the extensions
                for (int j = 0; j < excludedExtensions.Length; j++)
                {
                    // if this extension is excluded
                    if(result[i].extension == excludedExtensions[j])
                    {
                        // remove the file 
                        result.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }

        // return the resulting items as a list
        return result;
    }

    /// <summary>
    /// Whether this path directory exists in environment.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns></returns>
    internal bool DirectoryExists(string path)
    {
        return Directory.Exists(TotalLocalPath(path));
    }

    /// <summary>
    /// Whether this directory's parent exists in environment.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns></returns>
    internal bool ParentDirectoryExists(string path)
    {
        // override: can't go up further than the My Computer folder
        if (GetDirectoryName(TotalLocalPath(path)) == GameManager.MY_COMPUTER_PATH)
            return false;

        return Directory.GetParent(TotalLocalPath(path)) != null;
    }

    /// <summary>
    /// Whether this path is a directory.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns></returns>
    internal bool IsDirectory(string path)
    {
        // if this directory exists, it's a directory 
        if (Directory.Exists(TotalLocalPath(path)))
            return true;

        // if this file exists, it's not a directory
        if (File.Exists(TotalLocalPath(path)))
            return false;

        // if we didn't figure it out yet, 
        // check if the extension doesn't exist,
        // which indicates a directory as well
        string extension = Path.GetExtension(TotalLocalPath(path));
        return extension == null || extension.Length <= 1; // includes '.'
    }

    /// <summary>
    /// Gets the name of the directory on this path.
    /// </summary>
    /// <param name="path">Path to find directory name of</param>
    /// <returns></returns>
    internal string GetDirectoryName(string path)
    {
        return new DirectoryInfo(TotalLocalPath(path)).Name;
    }

    /// <summary>
    /// Gets the case sensitive path of the given path.
    /// Credits: Yogi - StackOverflow
    /// https://stackoverflow.com/questions/4763117/how-can-i-obtain-the-case-sensitive-path-on-windows/48627366#48627366
    /// </summary>
    /// <param name="path">The given path</param>
    /// <returns></returns>
    private string GetCaseSensitivePath(string path)
    {
        var root = Path.GetPathRoot(path);
        try
        {
            foreach (var name in path.Substring(root.Length).Split(Path.DirectorySeparatorChar))
                root = Directory.GetFileSystemEntries(root, name)[0];
        }
        catch (Exception)
        {
            root += path.Substring(root.Length);
        }
        return root;
    }

    /// <summary>
    /// Switches '/' to '\\' and vise versa.
    /// </summary>
    /// <param name="switchString">The string to switch slashes in</param>
    /// <returns></returns>
    internal string SwitchSlashes(string switchString, string wantedSlash)
    {
        if (wantedSlash == GameManager.BACK_SLASH)
            return switchString.Replace(GameManager.SLASH, GameManager.BACK_SLASH);
        else
            return switchString.Replace(GameManager.BACK_SLASH, GameManager.SLASH);
    }

    /// <summary>
    /// Returns whether the folder on path is locked.
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns></returns>
    internal bool IsLockedFolder(string path)
    {
        // compare the locked folders
        for (int i = 0; i < lockedFolders.Length; i++)
        {
            // return true if this path is included in locked folders
            if (path == lockedFolders[i])
                return true;
        }

        return false;
    }
    #endregion
}
