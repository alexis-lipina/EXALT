using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnvironmentPhysics))]
// Platform the player can "rest" at to activate it.
public class RestPlatform : MonoBehaviour
{
    public TriggerVolume PlayerDetectVolume;
    public PlayerHandler Player;
    public float RestDurationToStart = 1.0f;
    public bool IsPlayerStandingOn = false;
    public bool IsPlayerRestingOn = false;
    //public bool IsActivated = false;
    float timer = 0.0f;
    public List<SpriteRenderer> GlowOnPulseEmissives;
    public AnimationCurve GlowCurve_ChargeUp_LowerBound;
    public AnimationCurve GlowCurve_ChargeUp_UpperBound;
    public AnimationCurve GlowCurve_OutlinePulse;
    public AnimationCurve GlowCurve_ChargedFlash;

    //public UnityEvent OnActivated;
    //public UnityEvent OnDeactivated;

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
    private EnvironmentPhysics _environmentPhysics; 

    [SerializeField] private bool _isUseable = true; // whether player can interact with it at this time.
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
//                 if (IsActivated)
//                 {
//                     OnDeactivated.Invoke();
//                     IsActivated = false;
//                 }
            }
            _isUseable = value;
        }
    }
    private void Awake()
    {
        _environmentPhysics = GetComponent<EnvironmentPhysics>();
        _environmentPhysics.TopSprite.material.SetFloat("_GlowUpperClip", GlowCurve_ChargeUp_UpperBound.Evaluate(CurrentChargeAmount));
        _environmentPhysics.TopSprite.material.SetFloat("_GlowLowerClip", GlowCurve_ChargeUp_LowerBound.Evaluate(CurrentChargeAmount));
        foreach (SpriteRenderer renderer in GlowOnPulseEmissives)
        {
            renderer.material.SetFloat("_Opacity", 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (StaysFullCharge && CurrentChargeAmount == 1.0f)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_GlowUpperClip", GlowCurve_ChargeUp_UpperBound.Evaluate(CurrentChargeAmount));
            _environmentPhysics.TopSprite.material.SetFloat("_GlowLowerClip", GlowCurve_ChargeUp_LowerBound.Evaluate(CurrentChargeAmount));
            return;
        }

        if (!IsUseable)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_OutlineStrength", 0);
            _environmentPhysics.TopSprite.material.SetFloat("_IconStrength", 0);
        }
        else if (!IsPlayerStandingOn && !IsPlayerRestingOn)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_OutlineStrength", GlowCurve_OutlinePulse.Evaluate((Time.timeSinceLevelLoad * 0.5f) % 1.0f));
            _environmentPhysics.TopSprite.material.SetFloat("_IconStrength", 1.0f);

        }
        else if (IsPlayerStandingOn && !IsPlayerRestingOn)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_OutlineStrength", 1.0f);
            _environmentPhysics.TopSprite.material.SetFloat("_IconStrength", GlowCurve_OutlinePulse.Evaluate((Time.timeSinceLevelLoad * 1.0f) % 1.0f));
        }
        else if (IsPlayerRestingOn)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_OutlineStrength", 0.0f);
            _environmentPhysics.TopSprite.material.SetFloat("_IconStrength", 0.0f);
        }

        if (IsPlayerRestingOn || CurrentChargeAmount > 0)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_GlowUpperClip", GlowCurve_ChargeUp_UpperBound.Evaluate(CurrentChargeAmount));
            _environmentPhysics.TopSprite.material.SetFloat("_GlowLowerClip", GlowCurve_ChargeUp_LowerBound.Evaluate(CurrentChargeAmount));
        }
        else // dead glow
        {
            _environmentPhysics.TopSprite.material.SetFloat("_GlowUpperClip", 0.0f);
            _environmentPhysics.TopSprite.material.SetFloat("_GlowLowerClip", -1.0f);
        }

        if (IsActionPressed && CurrentChargeAmount != 1.0f && IsUseable)
        {
            CurrentChargeAmount = Mathf.Clamp01(CurrentChargeAmount + ChargeRate * Time.deltaTime);
            OnChargeAmountChanged.Invoke();
            if (CurrentChargeAmount == 1.0f)
            {
                OnFullyCharged.Invoke();
                StartCoroutine(MaxOutFlashCoroutine());
            }
        }
        else if (!IsActionPressed && CurrentChargeAmount != 0.0f)
        {
            CurrentChargeAmount = Mathf.Clamp01(CurrentChargeAmount - DecayRate * Time.deltaTime);
            OnChargeAmountChanged.Invoke();
        }
    }
    /*
    void AddRestTimer()
    {
        timer += Time.deltaTime;
        if (timer > RestDurationToStart)
        {
            IsActivated = true;
            OnActivated.Invoke();
            Player.CurrentRestPlatform = this;
        }
    }*/

    IEnumerator MaxOutFlashCoroutine()
    {
        float timer = 0;
        float flashDuration = 0.5f;
        while (timer < flashDuration)
        {
            _environmentPhysics.TopSprite.material.SetFloat("_ChargedFlash", GlowCurve_ChargedFlash.Evaluate(timer / flashDuration));

            foreach (SpriteRenderer renderer in GlowOnPulseEmissives)
            {
                renderer.material.SetFloat("_Opacity", GlowCurve_ChargedFlash.Evaluate(timer / flashDuration));
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _environmentPhysics.TopSprite.material.SetFloat("_ChargedFlash", 0);

        foreach (SpriteRenderer renderer in GlowOnPulseEmissives)
        {
            renderer.material.SetFloat("_Opacity", 0);
        }
    }
}
