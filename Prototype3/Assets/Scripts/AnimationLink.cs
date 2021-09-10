using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Michael Jordan
/// </summary>
public class AnimationLink : MonoBehaviour
{
    [Serializable]
    public struct AnimationEvent
    {
        public string _name;
        public UnityEvent _event;
    };

    public UnityEvent eventOne;
    public UnityEvent eventTwo;
    public UnityEvent eventThree;
    public UnityEvent eventFour;
    public UnityEvent eventFive;

    [Header("Custom Events")]
    public AnimationEvent[] animEvents;

    public void RunCustomEvent(int index)
    {
        animEvents[index]._event.Invoke();
    }

    public void RunCustomEventWithName(string name)
    {
        foreach (var animEvent in animEvents)
        {
            if(animEvent._name == name)
            {
                animEvent._event.Invoke();
                return;
            }
        }
    }

    /// <summary>
    /// Invokes an event from animation to an external script.
    /// </summary>
    public void PlayEventOne()
    {
        eventOne.Invoke();
    }

    /// <summary>
    /// Invokes an event from animation to an external script.
    /// </summary>
    public void PlayEventTwo()
    {
        eventTwo.Invoke();
    }

    /// <summary>
    /// Invokes an event from animation to an external script.
    /// </summary>
    public void PlayEventThree()
    {
        eventThree.Invoke();
    }
    /// <summary>
    /// Invokes an event from animation to an external script.
    /// </summary>
    public void PlayEventFour()
    {
        eventFour.Invoke();
    }
    /// <summary>
    /// Invokes an event from animation to an external script.
    /// </summary>
    public void PlayEventFive()
    {
        eventFive.Invoke();
    }
}
