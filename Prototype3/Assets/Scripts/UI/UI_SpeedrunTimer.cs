using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_SpeedrunTimer : UI_Element
{
    public TimeSpan timeElapsed { get; private set; }

    public TimeSpan fastestTime { get; private set; }

    private DateTime startTime;
    private TextMeshProUGUI m_display;
    private bool timerRunning = false;
    private bool show = false;
    private bool newRecord = false;
    public void Start()
    {
        m_display = GetComponent<TextMeshProUGUI>();
        m_display.enabled = false;
        string time = PlayerPrefs.GetString("myFastestTime", "");

        if (time != "")
            fastestTime = TimeSpan.ParseExact(time, "g", null);
        else
            fastestTime = TimeSpan.Zero;

#if UNITY_EDITOR
        fastestTime = TimeSpan.Zero;
#endif
    }
    // Start is called before the first frame update
    public void StartTimer()
    {
        timerRunning = true;
        show = true;
        startTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if(timerRunning)
            timeElapsed = DateTime.Now - startTime;

        if(m_display != null)
        {
            m_display.enabled = GameManager.instance.enableTimer && show;
            string text = timeElapsed.ToString("g");

            if (text.Length >= 11)
                m_display.text = text.Substring(0, 11);

            if (newRecord)
                m_display.faceColor = Color.green;
            else
                m_display.faceColor = Color.white;

            if(m_display.enabled && InputManager.instance.IsKeyDown(KeyType.R))
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void StopTimer()
    {
        if(timeElapsed < fastestTime || fastestTime == TimeSpan.Zero)
        {
            PlayerPrefs.SetString("myFastestTime", timeElapsed.ToString("g"));
            newRecord = true;
        }
        
        timerRunning = false;
    }

    public override bool IsContainingVector(Vector2 _pos)
    {
        return false;
    }

    public override void OnMouseDownEvent()
    {
        //Do nothing
    }

    public override void OnMouseUpEvent()
    {
        //Do nothing
    }
}
