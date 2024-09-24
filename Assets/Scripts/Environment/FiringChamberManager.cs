using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages firing chamber behavior in the Monolith's central area. Keeps anything
/// that should synchronize with the firing of the main cannon synchronized.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FiringChamberManager : MonoBehaviour
{

    [SerializeField] private List<TriggerVolume> ShieldingVolumes;
    [SerializeField] private List<EntityPhysics> KillableThings;
    [SerializeField] private Animator LaserAnimation;
    [SerializeField] private int SecondsBetweenShots = 8;
    private int SecondsUntilNextShot;
    [SerializeField] private SpriteRenderer[] GlowGradients;
    [SerializeField] private SpriteRenderer[] FinalSpikeGlows;
    [SerializeField] private AnimationCurve GradientGlowOverTime;
    [SerializeField] private AnimationCurve SpikeGlowOverTime;
    private AudioSource LaserAudioSource;
    float Timer = 0.0f;
    [SerializeField] MovingEnvironment CannonSyncMovingEnvironment;
    public float GlowScalar = 1.0f;
    [SerializeField] GameObject DamageOrigin;


    // Start is called before the first frame update
    void Start()
    {
        SecondsUntilNextShot = SecondsBetweenShots;
        StartCoroutine(PulseLaserBeam());
        LaserAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Timer += Time.deltaTime;
        foreach (SpriteRenderer renderer in GlowGradients)
        {
            renderer.material.SetFloat("_Opacity", GradientGlowOverTime.Evaluate(Timer/SecondsBetweenShots) * GlowScalar); 
        }
        foreach (SpriteRenderer renderer in FinalSpikeGlows)
        {
            renderer.material.SetFloat("_Opacity", SpikeGlowOverTime.Evaluate(Timer / SecondsBetweenShots) * GlowScalar);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 300, 300), SecondsUntilNextShot.ToString());
    }

    IEnumerator PulseLaserBeam()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            SecondsUntilNextShot -= 1;

            if (SecondsUntilNextShot == 4)
            {
                StartCoroutine(RampGlow(0.0f, 0.3f, 2.0f));
            }

            if (SecondsUntilNextShot == 2)
            {
                LaserAnimation.Play("FiringChamber_WarmUp", -1, 0.0f);
            }

            if (SecondsUntilNextShot == 1)
            {
                if (CannonSyncMovingEnvironment)
                {
                    CannonSyncMovingEnvironment.SetAnimRate(1.25f);
                }
                StartCoroutine(RampGlow(0.3f, 1.0f, 0.9f));
            }

            if (SecondsUntilNextShot == 0)
            {
                KillAllUnshieldedThings();
                SecondsUntilNextShot = SecondsBetweenShots;
                LaserAnimation.Play("FiringChamber_Fire", -1, 0.0f);
                StartCoroutine(RampGlow(1.0f, 0.0f, 2.0f));
                Timer = 0.0f;
                LaserAudioSource.Play();
                if (CannonSyncMovingEnvironment)
                {
                    CannonSyncMovingEnvironment.SetAnimRate(1.0f);
                    CannonSyncMovingEnvironment.PlayAnim();
                }
            }
        }
    }

    IEnumerator RampGlow(float startNormalized, float endNormalized, float duration)
    {
        yield return null;
        /*
        float increment = 0.025f;
        float timer = duration;
        while (timer > 0.0f)
        {
            foreach (SpriteRenderer renderer in GlowGradients)
            {
                renderer.material.SetFloat("_Opacity", Mathf.Lerp(endNormalized, startNormalized, timer / duration)); //timer/duration starts at 1.0f (x/x), ends at 0.0f (0/x)
            }
            timer -= increment;
            yield return new WaitForSeconds(increment);
        }
        foreach (SpriteRenderer renderer in GlowGradients)
        {
            renderer.material.SetFloat("_Opacity", endNormalized);
        }*/
    }


    void KillAllUnshieldedThings()
    {
        // all entities in shielded volumes
        List<EntityPhysics> safeEntities = new List<EntityPhysics>();

        foreach (TriggerVolume volume in ShieldingVolumes)
        {
            foreach (GameObject obj in volume.TouchingObjects)
            {
                if (!obj) continue;
                EntityPhysics physics = obj.GetComponent<EntityPhysics>();
                if (physics && !safeEntities.Contains(physics))
                {
                    safeEntities.Add(physics);
                }
            }
        }
        

        foreach (EntityPhysics entity in GameObject.FindObjectsOfType<EntityPhysics>())
        {
            if (!safeEntities.Contains(entity) && entity)
            {
                entity.Inflict(1000, 0.0f, ElementType.NONE, (entity.transform.position - DamageOrigin.transform.position).normalized * 2);
            }
        }
    }

    public void AddShieldingVolume(TriggerVolume volume)
    {
        ShieldingVolumes.Add(volume);
    }

    public void RemoveShieldingVolume(TriggerVolume volume)
    {
        if (ShieldingVolumes.Contains(volume))
        {
            ShieldingVolumes.Remove(volume);
        }
    }
}
