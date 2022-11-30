using System.IO;

/// <summary>
/// Keeps track of data of a file explorer program entry.
/// </summary>
public struct FEEntry
{
    // path of this entry
    public string path;

    // file or folder name 
    public string name;

    // file extension
    public string extension;

    // whether this is a directory
    public bool isDirectory;

    /// <summary>
    /// Constructor for settings each variable.
    /// </summary>
    /// <param name="path">Path of this entry</param>
    /// <param name="name">File or folder name of this entry</param>
    /// <param name="extension">File extension of this entry</param>
    /// <param name="isDirectory">Whether this entry is a directory</param>
    public FEEntry(string path, string name, string extension, bool isDirectory)
    {
        // set all variables
        this.path = path;
        this.name = name;
        this.extension = extension;
        this.isDirectory = isDirectory;
    }

    /// <summary>
    /// Constructor to build entry from FileSystemInfo
    /// </summary>
    /// <param name="fileInfo">The FileSystemInfo object to build this entry from</param>
    public FEEntry(FileSystemInfo fileInfo)
    {
        // keep path and name reference
        path = fileInfo.FullName;
        name = fileInfo.Name;

        // determine whether this is a directory
        isDirectory = (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        // set extenstion to empty initially
        extension = string.Empty;

        // attempt to find the extension based on . in name
        int length = name.Length;
        for (int i = length - 2; i >= 0; i--)
        {
            if (name[i] == '.')
            {
                extension = name.Substring(i, length - i).ToLowerInvariant();
                break;
            }
        }
    }
}