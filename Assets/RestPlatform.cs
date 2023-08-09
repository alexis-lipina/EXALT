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
    public bool IsActivated = false;
    float timer = 0.0f;
    float currentGlowAmount = 0.0f;
    float targetGlowAmount = 0.0f;
    public SpriteRenderer GlowEffect_Pattern;
    public SpriteRenderer GlowEffect_Aura;

    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    public bool DoesPlatformUseActionPress = false;
    public bool IsActionPressed = false; // controlled by player when interacting
    public bool DoesPlatformUseInputDirection = false;
    public Vector2 InputDirection; // controlled by player when interacting

    public UnityEvent OnActionPressed;
    public UnityEvent OnActionReleased;
    public UnityEvent OnDirectionInputReceived;

    // Update is called once per frame
    void Update()
    {
        if (GlowEffect_Aura && GlowEffect_Pattern)
        {
            GlowEffect_Pattern.material.SetFloat("_GlowLevel", currentGlowAmount);
            GlowEffect_Aura.material.SetFloat("_GlowLevel", currentGlowAmount);
            currentGlowAmount = Mathf.Lerp(currentGlowAmount, targetGlowAmount, 0.2f);
        }
        /*
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
                Player.CurrentRestPlatform = this;
            }
        }
        else if (IsActivated)
        {
            IsActivated = false;
            OnDeactivated.Invoke();
            Player.CurrentRestPlatform = null;
        }*/
    }

    void AddRestTimer()
    {
        timer += Time.deltaTime;
        if (timer > RestDurationToStart)
        {
            IsActivated = true;
            OnActivated.Invoke();
            Player.CurrentRestPlatform = this;
        }
    }

    public void SetTargetGlowAmount(float amount)
    {
        targetGlowAmount = amount;
    }
}
