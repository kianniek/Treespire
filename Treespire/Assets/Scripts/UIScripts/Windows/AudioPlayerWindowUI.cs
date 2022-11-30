using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioPlayerWindowUI : WindowUI
{
    // scene reference
    [SerializeField] private TextMeshProUGUI songTitle;
    [SerializeField] private Slider playProgressSlider;
    [SerializeField] private TextMeshProUGUI currentPlayTimeText;
    [SerializeField] private TextMeshProUGUI totalPlayTimeText;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Image playPauseButtonImage;

    // reference to the program
    private AudioPlayer audioPlayerRef;

    // keep track of play state
    internal bool playing = false;

    internal override void Initialize(Program program = null)
    {
        // call base
        base.Initialize(program);

        // save a reference to the audio player
        audioPlayerRef = (AudioPlayer)program;
    }

    internal override void Open()
    {
        // call base
        base.Open();

        // set title
        title.text = program.programName;

        // not playing
        playing = false;
        playPauseButtonImage.sprite = playIcon;
    }

    /// <summary>
    /// Updates UI elements to show the current clip.
    /// </summary>
    internal void ShowCurrentClip()
    {
        // is there a current clip?
        if(audioPlayerRef.CurrentSong.Equals(default(AudioPlayer.Song)))
        {
            // no current clip, clear texts
            songTitle.text = string.Empty;
            currentPlayTimeText.text = "00:00";
            totalPlayTimeText.text = "00:00";
        }
        else
        {
            // set message text to song name + artist name
            songTitle.text = audioPlayerRef.CurrentSong.songName + " ~ " + audioPlayerRef.CurrentSong.artistName;

            // update the current play times
            ShowCurrentPlayTimes();
        }
    }

    /// <summary>
    /// Updates the current and total play times.
    /// </summary>
    /// <param name="onlyUpdateCurrent">Whether only the current play time should be updated.</param>
    private void ShowCurrentPlayTimes(bool onlyUpdateCurrent = false)
    {
        // declare variables for calculating the play time
        float playTime;
        int min, sec;

        // also update the total time?
        if (!onlyUpdateCurrent)
        {
            // get total play time
            playTime = audioPlayerRef.CurrentSong.totalPlayTime;

            // define min vs sec based on total play time
            min = Mathf.FloorToInt(playTime / 60);
            sec = Mathf.FloorToInt(playTime % 60);

            // show the total play time in 00:00 format
            totalPlayTimeText.text = min.ToString("00") + ":" + sec.ToString("00");
        }

        // get the current play time
        playTime = GameManager.instance.audioManager.GetMusicPlaytime();

        // define min vs sec based on play time
        min = Mathf.FloorToInt(playTime / 60);
        sec = Mathf.FloorToInt(playTime % 60);

        // show the play time in 00:00 format
        currentPlayTimeText.text = min.ToString("00") + ":" + sec.ToString("00");

        // set the sliders value based on current and total play time
        if (playTime > 0)
            playProgressSlider.value = playTime / audioPlayerRef.CurrentSong.totalPlayTime;
        else
            playProgressSlider.value = 0;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        // update timer text while playing
        if (playing)
            ShowCurrentPlayTimes(true);
    }

    /// <summary>
    /// Called when play / pause button is clicked.
    /// </summary>
    public void PressPlayPause()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        playing = !playing;
        playPauseButtonImage.sprite = playing ? pauseIcon : playIcon;

        audioPlayerRef.PressPlayPause();
    }

    /// <summary>
    /// Called when previous button is clicked.
    /// </summary>
    public void PressPrevious()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        audioPlayerRef.PressPrevious();
    }

    /// <summary>
    /// Called when next button is clicked.
    /// </summary>
    public void PressNext()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        audioPlayerRef.PressNext();
    }

    /// <summary>
    /// Called when refresh button is clicked.
    /// </summary>
    public void PressRefresh()
    {
        // return if not interactable
        if (!interactable)
            return;

        OnClick();

        audioPlayerRef.PressRefresh();
        playing = false;
    }
}
