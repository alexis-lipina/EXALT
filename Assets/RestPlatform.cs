using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Platform the player can "rest" at to activate it.
public class RestPlatform : MonoBehaviour
{
    public TriggerVolume PlayerDetectVolume;
    public PlayerHandler Player;
    public float RestDurationToStart = 1.0f;
    bool IsActivated = false;
    float timer = 0.0f;

    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerDetectVolume.IsTriggered)
        {
            if (Player.IsResting() && !IsActivated)
            {
                AddRestTimer();
            }
            if (!Player.IsResting() && IsActivated)
            {
                //AddRestTimer();
                IsActivated = false;
                OnDeactivated.Invoke();
            }
        }
        else if (IsActivated)
        {
            IsActivated = false;
            OnDeactivated.Invoke();
        }
    }

    void AddRestTimer()
    {
        timer += Time.deltaTime;
        if (timer > RestDurationToStart)
        {
            IsActivated = true;
            OnActivated.Invoke();
        }
    }
}
