using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

[CreateAssetMenu(menuName = "Programs/Audio Player")]
public class AudioPlayer : Program
{
    internal struct Song
    {
        internal AudioClip clip;

        internal string index;
        internal string songName;
        internal string artistName;

        internal float totalPlayTime;
    }

    private Song[] songs;
    private int currentSong;
    internal Song CurrentSong 
    { 
        get 
        {
            if (songs != null && songs.Length > 0 && currentSong >= 0)
                return songs[currentSong];
            else
                return default(Song);
        } 
    }

    private delegate void OnLoadingCompleteDelegate();

    // reference to the audio player window prefab
    [SerializeField] private GameObject audioPlayerWindowPrefab;

    // reference to the instantiated audio player window object
    private AudioPlayerWindowUI audioPlayerWindow;

    // error definition for when there is no music on disk
    [SerializeField] private ErrorDefinition noDataError;

    // keeps track of whether music is being loaded rn
    private bool loadingSongs = false;
    private bool LoadingSongs { get { return loadingSongs; } set { loadingSongs = value; audioPlayerWindow.EnableInteractions(!value); } }

    internal override void Initialize()
    {
        // call base
        base.Initialize();

        // instantiate the prefab window
        audioPlayerWindow = Instantiate(audioPlayerWindowPrefab, GameManager.instance.windowParent).GetComponent<AudioPlayerWindowUI>();

        // setup the window
        audioPlayerWindow.Initialize(this);
        audioPlayerWindow.Close();

        // programs window is same as game window
        // but a different type
        window = audioPlayerWindow;

        // load songs
        LoadingSongs = false;
        GameManager.instance.StartCoroutine(LoadSongs(null));
    }

    internal override bool StartUp()
    {
        // call base
        if (!base.StartUp())
            return false;

        // set and show the first song
        SetFirstSong();

        // succesfully started
        return true;
    }

    internal override void ShutDown()
    {
        // call base
        base.ShutDown();

        // stop playing
        GameManager.instance.audioManager.StopMusic();
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    internal override bool Update()
    {
        // call base
        if (!base.Update())
            return false;

        // don't update if we're loading rn
        if (LoadingSongs)
            return true;

        // if the player should be playing but the source isn't playing anymore, 
        // goto next song
        if (audioPlayerWindow.playing && 
            !GameManager.instance.audioManager.GetMusicIsPlaying() &&
            GameManager.instance.audioManager.GetMusicPlaytime() > 0)
            PressNext();

        // succesfully updated
        return true;
    }

    /// <summary>
    /// Called when play / pause button is clicked.
    /// </summary>
    internal void PressPlayPause()
    {
        // don't react on buttons if we're loading rn
        if (LoadingSongs)
            return;

        // toggle play / pause
        if (audioPlayerWindow.playing)
            GameManager.instance.audioManager.PlayMusic(true);
        else
            GameManager.instance.audioManager.PlayMusic(false);
    }

    /// <summary>
    /// Called when previous button is clicked.
    /// </summary>
    internal void PressPrevious()
    {
        // don't react on buttons if we're loading rn
        if (LoadingSongs)
            return;

        // go back one clip
        currentSong--;

        // loop it if we already played first clip
        if (currentSong < 0)
            currentSong = songs.Length - 1;
        
        SetCurrentSong();
    }

    /// <summary>
    /// Called when next button is clicked.
    /// </summary>
    internal void PressNext()
    {
        // don't react on buttons if we're loading rn
        if (LoadingSongs)
            return;

        // go further one clip
        currentSong++;

        // loop it if we already played last clip
        if (currentSong >= songs.Length)
            currentSong = 0;
        
        SetCurrentSong();
    }

    /// <summary>
    /// Called when refresh button is clicked.
    /// </summary>
    internal void PressRefresh()
    {
        // don't react on buttons if we're loading rn
        if (LoadingSongs)
            return;

        // pause if we're playing
        if (audioPlayerWindow.playing)
            PressPlayPause();

        // re-load songs
        // and reset current song on complete
        GameManager.instance.StartCoroutine(LoadSongs(delegate { SetFirstSong(); }));
    }

    /// <summary>
    /// Starts song.
    /// </summary>
    private void SetCurrentSong()
    {
        // don't allow to set a song if we're loading rn
        if (LoadingSongs)
            return;

        // set the clip
        GameManager.instance.audioManager.SetMusicClip(songs[currentSong].clip);

        // and start playing if we were playing
        if (audioPlayerWindow.playing)
            GameManager.instance.audioManager.PlayMusic(true);

        // update UI
        audioPlayerWindow.ShowCurrentClip();
    }

    /// <summary>
    /// Sets the first song as clip and adjusts the UI.
    /// </summary>
    private void SetFirstSong()
    {
        // don't allow to set a song if we're loading rn
        if (LoadingSongs)
            return;

        // if there arn't any songs,
        // show error message and shut down
        if (songs.Length == 0)
        {
            GameManager.instance.errorWindow.Open();
            GameManager.instance.errorWindow.SetText(noDataError);
            GameManager.instance.errorWindow.SetButtonDelegates(ShutDown);
            return;
        }

        // start with first clip but don't play yet
        currentSong = 0;
        GameManager.instance.audioManager.SetMusicClip(songs[currentSong].clip);

        // show UI elements for this clip
        audioPlayerWindow.ShowCurrentClip();
    }

    #region LOADING
    /// <summary>
    /// Loads songs from the streaming assets folder.
    /// </summary>
    /// <param name="onLoadingCompleteDelegate">Called on loading complete</param>
    /// <returns></returns>
    private IEnumerator LoadSongs(OnLoadingCompleteDelegate onLoadingCompleteDelegate)
    {
        // only start loading if we're 
        if (!LoadingSongs)
        {
            // start loading songs, disables program
            LoadingSongs = true;
                        
            // make sure everything is unloaded
            UnloadSongs();

            // makes UI elements update before we continue
            yield return new WaitForEndOfFrame();

            // get the music path in the streaming assets folder
            string musicPath = GameManager.instance.GetStreamingAssetsPath + GameManager.SLASH + GameManager.instance.GetMusicPath;

            // get all the mp3 files in the music path
            string[] files = System.IO.Directory.GetFiles(musicPath, "*.mp3");

            // initialize song array with correct length
            songs = new Song[files.Length];

            // for each file, load it and store it in a new song struct
            for (int i = 0; i < files.Length; i++)
            {
                // get the audio as a request
                WWW request = LoadSongFromFile(files[i]);
                yield return request;

                // create the song
                songs[i] = new Song();

                // set its clip
                songs[i].clip = request.GetAudioClip();

                // and its name by removing the path and .mp3 part
                songs[i].clip.name = files[i].Substring(files[i].LastIndexOf(GameManager.SLASH) + 1);
                songs[i].clip.name = songs[i].clip.name.Substring(0, songs[i].clip.name.Length - 4);

                // figure out the name of song and artist
                // according to template: songName-artistName
                string[] splitName = songs[i].clip.name.Split('-');
                songs[i].index = Regex.Replace(splitName[0], "([A-Z])", " $1").Trim().ToLower();
                songs[i].songName = Regex.Replace(splitName[1], "([A-Z])", " $1").Trim().ToLower();
                songs[i].artistName = Regex.Replace(splitName[2], "([A-Z])", " $1").Trim().ToLower();

                // set total duration
                songs[i].totalPlayTime = songs[i].clip.length;
            }

            // done loading, release controls
            LoadingSongs = false;
        }

        // wait again. some UI elements update by also waiting until the end of the frame
        // so we have to wait that out as well
        yield return new WaitForEndOfFrame();

        onLoadingCompleteDelegate?.Invoke();
    }

    /// <summary>
    /// Unloads songs from memory.
    /// </summary>
    private void UnloadSongs()
    {
        // if there are no songs, nothing to unload so return
        if (songs == null || songs.Length == 0)
            return;

        // stop the song if it's playing
        GameManager.instance.audioManager.StopMusic();
        GameManager.instance.audioManager.SetMusicClip(null);
        currentSong = -1;
        audioPlayerWindow.ShowCurrentClip();

        // for each of the songs
        for (int i = 0; i < songs.Length; i++)
        {
            // unload and destroy 
            songs[i].clip.UnloadAudioData();
            DestroyImmediate(songs[i].clip, false);
            songs[i].clip = null;
        }
    }

    /// <summary>
    /// Loads a song from a given path.
    /// </summary>
    /// <param name="path">The path to load from</param>
    /// <returns>The request containing the song</returns>
    private WWW LoadSongFromFile(string path)
    {
        return new WWW(path);
    }
    #endregion
}
