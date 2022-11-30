using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FEIconDefinition 
{
    // icon types
    public enum IconType
    {
        DefaultFolder = 0,
        DesktopFolder,
        ImageFolder,
        MusicFolder,
        DocumentFolder,
        LockedFolder,
        UserFolder,

        DefaultFile = 50,
        ImageFile,
        AudioFile,
        DocumentFile
    }

    // pairs an icon type to one or more extensions
    [Serializable]
    public struct IconExtensionPair
    {
        public List<string> extensions;
        public IconType iconType;
    }

    // pairs an icon type to a foldername / directory
    [Serializable]
    public struct IconFolderNamePair
    {
        public string folderName;
        public IconType iconType;
    }

    // pairs an icontype to a sprite to display in the file explorer
    [Serializable]
    public struct IconSpritePair
    {
        public IconType iconType;
        public Sprite sprite;
    }
}
