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
    public bool IsPlayerRestingOn = false;
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

    //normalized charge / hold amount, which we will use a bunch probably
    public float ChargeRate = 1.0f; // when held
    public float DecayRate = 1.0f; // when not held
    public float CurrentChargeAmount = 0.0f;
    public bool StaysFullCharge = false;
    public UnityEvent OnChargeAmountChanged;
    public UnityEvent OnFullyCharged;

    private bool _isUseable = true; // whether player can interact with it at this time.
    public bool IsUseable
    {
        get { return _isUseable; }
        set
        {
            if (!value)
            {
                if (IsActionPressed)
                {
                    OnActionReleased.Invoke();
                    IsActionPressed = false;
                }
                InputDirection = Vector2.zero;
                if (IsActivated)
                {
                    OnDeactivated.Invoke();
                    IsActivated = false;
                }
            }
            _isUseable = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (StaysFullCharge && CurrentChargeAmount == 1.0f) return;

        if (GlowEffect_Aura && GlowEffect_Pattern)
        {
            GlowEffect_Pattern.material.SetFloat("_GlowLevel", currentGlowAmount);
            GlowEffect_Aura.material.SetFloat("_GlowLevel", currentGlowAmount);
            currentGlowAmount = Mathf.Lerp(currentGlowAmount, targetGlowAmount, 0.2f);
        }
        
        if (IsActionPressed && CurrentChargeAmount != 1.0f && IsUseable)
        {
            CurrentChargeAmount = Mathf.Clamp01(CurrentChargeAmount + ChargeRate * Time.deltaTime);
            OnChargeAmountChanged.Invoke();
            if (CurrentChargeAmount == 1.0f)
            {
                OnFullyCharged.Invoke();
            }
        }
        else if (!IsActionPressed && CurrentChargeAmount != 0.0f)
        {
            CurrentChargeAmount = Mathf.Clamp01(CurrentChargeAmount - DecayRate * Time.deltaTime);
            OnChargeAmountChanged.Invoke();
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
