using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Michael Jordan
/// </summary>
public class AnimationLink : MonoBehaviour
{
    public UnityEvent eventOne;
    public UnityEvent eventTwo;
    public UnityEvent eventThree;
    public UnityEvent eventFour;
    public UnityEvent eventFive;
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
