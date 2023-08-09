using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentPhysics))]
public class SummonblePlatform : MonoBehaviour
{
    private bool _isSummoned = true;
    private float _summonedElevation = 0.0f; //set on start
    [SerializeField] private AnimationCurve _interpolateCurve;
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private SummonblePlatform[] adjacents;
    [SerializeField] bool StartSummoned = false;
    private const float _hiddenElevation = -25.0f;
    private const float _animationDuration = 0.3f;

    private EnvironmentPhysics environmentPhysics;


    // Start is called before the first frame update
    void Start()
    {
        environmentPhysics = GetComponent<EnvironmentPhysics>();
        _summonedElevation = environmentPhysics.GetBottomHeight();
        if (!StartSummoned) Disappear();
    }

    // Update is called once per frame
    void Update()
    {
        //I want this to be raised only if it or an adjacent platform has the player on it
        bool shouldBeSummoned = false;
        foreach (SummonblePlatform platform in adjacents)
        {
            if (platform.IsPlayerHere()) shouldBeSummoned = true;
        }
        if (IsPlayerHere()) shouldBeSummoned = true;

        if (shouldBeSummoned)
        {
            Summon();
        }
        else
        {
            Disappear();
        }
    }
    
    public void Summon()
    {
        if (_isSummoned) return;
        _isSummoned = true;
        StartCoroutine(SummonCoroutine());
    }

    public void Disappear()
    {
        if (StartSummoned) return;
        if (!_isSummoned) return;
        _isSummoned = false;
        StartCoroutine(DisappearCoroutine());
    }


    private IEnumerator DisappearCoroutine()
    {
        //Drop
        Debug.Log("Dropping!");
        float opacity = 1.0f;
        environmentPhysics._isCollapsed = true;
        float timer = 0.0f;
        float currentElevation = _summonedElevation;
        float objectHeight = environmentPhysics.ObjectHeight;

        while (timer < _animationDuration)
        {
            opacity = _interpolateCurve.Evaluate( (_animationDuration - timer) / _animationDuration);
            currentElevation = Mathf.Lerp(_summonedElevation, _hiddenElevation, 1 - opacity); //opacity basically acts as a normalized version of the timer

            environmentPhysics.BottomHeight = currentElevation;
            environmentPhysics.TopHeight =  currentElevation + objectHeight;

            environmentPhysics.TopSprite.material.SetFloat("_Opacity", opacity);
            environmentPhysics.FrontSprite.material.SetFloat("_Opacity", opacity);

            environmentPhysics.TopSprite.gameObject.transform.localPosition = Vector2.up * currentElevation;
            environmentPhysics.FrontSprite.gameObject.transform.localPosition = Vector2.up * (-2 + currentElevation);
            timer += Time.deltaTime;
            yield return null;
        }
        environmentPhysics.TopSprite.material.SetFloat("_Opacity", 0);
        environmentPhysics.FrontSprite.material.SetFloat("_Opacity", 0);
    }

    private IEnumerator SummonCoroutine()
    {
        //Drop
        Debug.Log("Raising!");
        float opacity = 0.0f;
        environmentPhysics._isCollapsed = true;
        float timer = 0.0f;
        float currentElevation = _summonedElevation;
        float objectHeight = environmentPhysics.ObjectHeight;

        while (timer < _animationDuration)
        {
            opacity = _interpolateCurve.Evaluate( timer / _animationDuration);
            currentElevation = Mathf.Lerp(_summonedElevation, _hiddenElevation, 1 - opacity); //opacity basically acts as a normalized version of the timer

            environmentPhysics.BottomHeight = currentElevation;
            environmentPhysics.TopHeight = currentElevation + objectHeight;

            environmentPhysics.TopSprite.material.SetFloat("_Opacity", opacity);
            environmentPhysics.FrontSprite.material.SetFloat("_Opacity", opacity);

            // top
            environmentPhysics.TopSprite.gameObject.transform.localPosition = Vector2.up * currentElevation;
            // front
            environmentPhysics.FrontSprite.gameObject.transform.localPosition = Vector2.up * (-2 + currentElevation);
            timer += Time.deltaTime;
            yield return null;
        }
        environmentPhysics.BottomHeight = _summonedElevation;
        environmentPhysics.TopHeight = _summonedElevation + objectHeight;

        environmentPhysics.TopSprite.material.SetFloat("_Opacity", 1);
        environmentPhysics.FrontSprite.material.SetFloat("_Opacity", 1);
        // top
        environmentPhysics.TopSprite.gameObject.transform.localPosition = Vector2.up * _summonedElevation;
        // front
        environmentPhysics.FrontSprite.gameObject.transform.localPosition = Vector2.up * (-2 + _summonedElevation);
    }

    public bool IsPlayerHere()
    {
        /*if (GetComponent<BoxCollider2D>().bounds.Contains(_playerPhysics.transform.position))
        {
            return true;
        }
        return false;
        */
        Vector2 centerPos = GetComponent<BoxCollider2D>().bounds.center;
        Vector2 playerPos = _playerPhysics.transform.position;
        if ((centerPos - playerPos).magnitude < 3.0f && _isSummoned)
        {
            return true;
        }
        else return false;
    }
}
