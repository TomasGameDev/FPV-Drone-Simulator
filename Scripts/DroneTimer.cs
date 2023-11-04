using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneTimer : MonoBehaviour
{
    public Text timeText;
    public Clock clock = new Clock();
    public Clock lockedClock = new Clock();
    public bool isWorking = true;
    [System.Serializable]
    public class Clock
    {
        public int frames, seconds, minutes, hours;
    }
    public void lockClock()
    {
        lockedClock.frames = clock.frames;
        lockedClock.seconds = clock.seconds;
        lockedClock.minutes = clock.minutes;
        lockedClock.hours = clock.hours;
    }
    public void ReloadClock()
    {
        clock.frames = lockedClock.frames;
        clock.seconds = lockedClock.seconds;
        clock.minutes = lockedClock.minutes;
        clock.hours = lockedClock.hours;
    }
    public void ResetClock()
    {
        clock.frames = lockedClock.frames;
        clock.seconds = lockedClock.seconds;
        clock.minutes = lockedClock.minutes;
        clock.hours = lockedClock.hours;
    }
    void FixedUpdate()
    {
        if (!isWorking)
            return;
        clock.frames++;
        if (clock.frames >= 60)
        {
            clock.frames = 0;
            clock.seconds++;
        }
        if (clock.seconds >= 60)
        {
            clock.seconds = 0;
            clock.minutes++;
        }
        if (clock.minutes >= 60)
        {
            clock.minutes = 0;
            clock.hours++;
        }
        if (clock.hours >= 24)
            clock.hours = 0;
        timeText.text = (clock.hours > 0 ? intToTimeText(clock.hours) + ":" : "") + intToTimeText(clock.minutes) + ":" + intToTimeText(clock.seconds);// + ":" + intToTimeText(clock.frames)
    }
    public void SetNullTime()
    {
        timeText.text = "--:--:--";
    }
    string intToTimeText(int _val)
    {
        string text = _val.ToString();
        if (_val < 10)
            text = "0" + text;
        return text;
    }
}