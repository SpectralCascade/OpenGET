using OpenGET;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Timer (run as co-routine) to track how much time has passed since starting.
/// </summary>
[Serializable]
public class Timer
{
    public delegate float GetTimeDelta();

    /// <summary>
    /// Time since this timer started in seconds.
    /// </summary>
    public float time { get => _time; private set => _time = value; }
    [SerializeField]
    private float _time = 0;

    /// <summary>
    /// Is this timer running, or is it stopped?
    /// </summary>
    public bool isRunning => coroutine != null;

    /// <summary>
    /// Whether to use unscaled delta time.
    /// </summary>
    private bool unscaledDeltaTime = false;

    /// <summary>
    /// Associated coroutine, if running.
    /// </summary>
    private Coroutine coroutine = null;

    /// <summary>
    /// Associated delta time function, when not using built-in delta time.
    /// </summary>
    private GetTimeDelta deltaTime = null;

    /// <summary>
    /// Start the timer. By default uses scaled Unity delta time; specify true to use unscaled delta time instead.
    /// </summary>
    public void Start(bool unscaledDeltaTime = false)
    {
        Stop();
        this.unscaledDeltaTime = unscaledDeltaTime;
        coroutine = Coroutines.Start(UpdateTime());
    }

    public void Start(GetTimeDelta deltaTime)
    {
        Stop();
        this.deltaTime = deltaTime;
        coroutine = Coroutines.Start(UpdateTime());
    }

    private IEnumerator UpdateTime()
    {
        yield return new WaitForEndOfFrame();
        while (isRunning)
        {
            time += deltaTime != null ? deltaTime.Invoke() : (unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    /// <summary>
    /// Stop the timer.
    /// </summary>
    public void Stop()
    {
        time = 0;
        if (coroutine != null)
        {
            Coroutines.Stop(coroutine);
            coroutine = null;
        }
    }

}
