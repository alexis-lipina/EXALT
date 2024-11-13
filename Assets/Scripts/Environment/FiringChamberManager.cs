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
    [SerializeField] private SpriteRenderer[] GlowGradients;
    [SerializeField] private SpriteRenderer[] FinalSpikeGlows;
    [SerializeField] private AnimationCurve GradientGlowOverTime;
    [SerializeField] private AnimationCurve SpikeGlowOverTime;
    private AudioSource LaserAudioSource;
    float Timer = 0.0f;
    [SerializeField] MovingEnvironment CannonSyncMovingEnvironment;
    public float GlowScalar = 1.0f;
    [SerializeField] GameObject DamageOrigin;
    [SerializeField] Vector2 OverrideDamageDirection = Vector2.zero;


    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(PulseLaserBeam());
        LaserAudioSource = GetComponent<AudioSource>();
        Timer = MonolithPersistent.GetInstance().SuperweaponTimer;
        if (Timer != 0.0f && Timer != 0.5f) // either of these values being exact is unlikely except for at startup of the persistent cannon, and I dunno if OnSceneLoad will have run so
        {
            StartCoroutine(PulseLaserBeam(Timer));
        }

    }

    private void Update()
    {
        Timer = MonolithPersistent.GetInstance().SuperweaponTimer;
        foreach (SpriteRenderer renderer in GlowGradients)
        {
            renderer.material.SetFloat("_Opacity", GradientGlowOverTime.Evaluate(Timer/SecondsBetweenShots) * GlowScalar); 
        }
        foreach (SpriteRenderer renderer in FinalSpikeGlows)
        {
            renderer.material.SetFloat("_Opacity", SpikeGlowOverTime.Evaluate(Timer / SecondsBetweenShots) * GlowScalar);
        }
    }

//    void OnGUI()
//    {
//        GUI.Label(new Rect(0, 0, 300, 300), Timer.ToString());
//    }

    IEnumerator PulseLaserBeam(float startTime = 0.0f)
    {
        if (startTime < 6)
        {
            yield return new WaitForSeconds(6.0f - startTime);

        }

        if (startTime > 6 && startTime < 8)
        {
            LaserAnimation.Play("FiringChamber_WarmUp", -1, (startTime - 6.0f) / 2.0f); // 2.0 = duration of animation
        }
        else
        {
            LaserAnimation.Play("FiringChamber_WarmUp", -1, 0.0f);
        }
        yield return new WaitForSeconds(1.0f);

        if (CannonSyncMovingEnvironment)
        {
            CannonSyncMovingEnvironment.SetAnimRate(1.25f); // this may need to be adjusted
        }
    }


    public void FireCannon()
    {
        KillAllUnshieldedThings();
        LaserAnimation.Play("FiringChamber_Fire", -1, 0.0f);
        Timer = 0.0f;
        //LaserAudioSource.Play();
        if (CannonSyncMovingEnvironment)
        {
            CannonSyncMovingEnvironment.SetAnimRate(1.0f);
            CannonSyncMovingEnvironment.PlayAnim();
        }
        StartCoroutine(PulseLaserBeam());
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
                Vector2 direction = OverrideDamageDirection.sqrMagnitude > 0 ? OverrideDamageDirection : (Vector2)(entity.transform.position - DamageOrigin.transform.position).normalized;
                entity.Inflict(1000, 0.0f, ElementType.ICHOR, direction * 2);
                if (entity.Handler is PlayerHandler)
                {
                    PlayerHandler playerHandler = (PlayerHandler)entity.Handler;
                    playerHandler.DeathFreezeDuration = 0.1f;
                }
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
