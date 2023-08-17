﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleAreaController : MonoBehaviour
{
    List<MovingEnvironment> RipplePillars;
    List<float> RipplePillarNormalizedHeights;
    List<Vector2> RipplePillarPositions;
    List<EntityPhysics> EnemyColliders;

    [SerializeField] float ShockwaveDuration = 2.0f;
    [SerializeField] float ShockwaveDistance = 64f; // 3/4'din the Y to suit the "camera angle"
    [SerializeField] AnimationCurve ShockwavePeakOverDuration;
    [SerializeField] AnimationCurve ShockwaveRisePattern;
    [SerializeField] float EnemyRaisedPlatformEffectRadius = 16f; // 3/4'din the Y to suit the "camera angle"
    [SerializeField] AnimationCurve EnemyRaisedPlatformPattern;


    // Start is called before the first frame update
    void Start()
    {
        EnemyColliders = new List<EntityPhysics>();
        RipplePillars = new List<MovingEnvironment>();
        MovingEnvironment[] envts = GetComponentsInChildren<MovingEnvironment>();
        foreach (MovingEnvironment envt in envts)
        {
            RipplePillars.Add(envt);
        }

        RipplePillarNormalizedHeights = new List<float>();
        RipplePillarPositions = new List<Vector2>();
        for (int i = 0; i < RipplePillars.Count; i++)
        {
            RipplePillarNormalizedHeights.Add(0.0f);
            RipplePillarPositions.Add(RipplePillars[i].GetComponent<BoxCollider2D>().bounds.center);
        }
    }

    void Update()
    {
        List<EntityPhysics> removeThese = new List<EntityPhysics>();
        // Check player positions
        foreach (EntityPhysics phys in EnemyColliders)
        {
            if (phys)
            {
                if (phys.GetCurrentHealth() == 0)
                {
                    removeThese.Add(phys);
                }
                RaisePillarsNearEntity(phys);
            }
        }
        foreach (EntityPhysics phys in removeThese)
        {
            EnemyColliders.Remove(phys);
        }


        for (int i = 0; i < RipplePillars.Count; i++)
        {
            RipplePillars[i].SetToElevation(RipplePillarNormalizedHeights[i] * 12.0f - 24);
            RipplePillarNormalizedHeights[i] = Mathf.Lerp(0, RipplePillarNormalizedHeights[i], 0.9f);
        }
    }

    public void TriggerShockwave(Vector2 Origin)
    {
        StartCoroutine(Shockwave(Origin));
    }

    void RaisePillarsNearEntity(EntityPhysics entity)
    {
        Vector2 entityLocation = entity.transform.position;
        for (int i = 0; i < RipplePillarPositions.Count; i++)
        {
            float NormalizedDistance = Mathf.Clamp((entityLocation - RipplePillarPositions[i]).magnitude / EnemyRaisedPlatformEffectRadius, 0, 1);
            float heightItShouldBe = EnemyRaisedPlatformPattern.Evaluate(NormalizedDistance);
            if (heightItShouldBe > RipplePillarNormalizedHeights[i])
            {
                RipplePillarNormalizedHeights[i] = heightItShouldBe;
            }
        }
    }

    IEnumerator Shockwave(Vector2 OriginPoint)
    {
        // modulate the RipplePillarNormalizedHeights based on RipplePillarPosition over the duration of a shockwave.
        float timer = 0.0f;

        while (timer < ShockwaveDuration)
        {
            for (int i = 0; i < RipplePillarPositions.Count; i++)
            {
                float NormalizedDistance = Mathf.Clamp((OriginPoint - RipplePillarPositions[i]).magnitude / ShockwaveDistance, 0, 1);
                float heightItShouldBe = ShockwaveRisePattern.Evaluate(((timer / ShockwaveDuration) - NormalizedDistance) * 2) * ShockwavePeakOverDuration.Evaluate(NormalizedDistance);
                if (heightItShouldBe > RipplePillarNormalizedHeights[i])
                {
                    RipplePillarNormalizedHeights[i] = heightItShouldBe;
                }
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void AddTrackedEnemy(GameObject enemy)
    {
        EnemyColliders.Add(enemy.GetComponentInChildren<EntityPhysics>());
    }
}