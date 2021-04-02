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


    // Start is called before the first frame update
    void Start()
    {
        _summonedElevation = GetComponent<EnvironmentPhysics>().GetBottomHeight();
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
        GetComponent<EnvironmentPhysics>()._isCollapsed = true;
        float timer = 0.0f;
        float currentElevation = _summonedElevation;
        float objectHeight = GetComponent<EnvironmentPhysics>().ObjectHeight;

        while (timer < _animationDuration)
        {
            opacity = _interpolateCurve.Evaluate( (_animationDuration - timer) / _animationDuration);
            currentElevation = Mathf.Lerp(_summonedElevation, _hiddenElevation, 1 - opacity); //opacity basically acts as a normalized version of the timer

            GetComponent<EnvironmentPhysics>().BottomHeight = currentElevation;
            GetComponent<EnvironmentPhysics>().TopHeight =  currentElevation + objectHeight;

            GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", opacity);
            GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", opacity);


            // top
            GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition = Vector2.up * currentElevation;
            // front
            GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition = Vector2.up * (-2 + currentElevation);
            timer += Time.deltaTime;
            yield return null;
        }
        GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", 0);
        GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", 0);
    }

    private IEnumerator SummonCoroutine()
    {
        //Drop
        Debug.Log("Raising!");
        float opacity = 0.0f;
        GetComponent<EnvironmentPhysics>()._isCollapsed = true;
        float timer = 0.0f;
        float currentElevation = _summonedElevation;
        float objectHeight = GetComponent<EnvironmentPhysics>().ObjectHeight;

        while (timer < _animationDuration)
        {
            opacity = _interpolateCurve.Evaluate( timer / _animationDuration);
            currentElevation = Mathf.Lerp(_summonedElevation, _hiddenElevation, 1 - opacity); //opacity basically acts as a normalized version of the timer

            GetComponent<EnvironmentPhysics>().BottomHeight = currentElevation;
            GetComponent<EnvironmentPhysics>().TopHeight = currentElevation + objectHeight;

            GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", opacity);
            GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", opacity);

            // top
            GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition = Vector2.up * currentElevation;
            // front
            GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition = Vector2.up * (-2 + currentElevation);
            timer += Time.deltaTime;
            yield return null;
        }
        GetComponent<EnvironmentPhysics>().BottomHeight = _summonedElevation;
        GetComponent<EnvironmentPhysics>().TopHeight = _summonedElevation + objectHeight;

        GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", 1);
        GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", 1);
        // top
        GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition = Vector2.up * _summonedElevation;
        // front
        GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition = Vector2.up * (-2 + _summonedElevation);
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
