using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DesktopTimeUI : MonoBehaviour
{
    // reference to text object
    [SerializeField] private TextMeshProUGUI text;

    /// <summary>
    /// Update is called once per frame 
    /// </summary>
    private void Update()
    {
        // update time text to specific format (00:00 AM)
        text.text = DateTime.Now.ToString("hh:mm tt");
    }
}
