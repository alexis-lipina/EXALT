using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sol projectiles radiate heat over an area
/// </summary>
public class SolProjectileHandler : BulletHandler
{
    enum SolProjectileState { PREIGNITE, IGNITE, DESPAWN };
    private SolProjectileState _currentSolProjectileState;

    private float _stateTimer = 0.0f;

    // INITIAL LAUNCH STATE
    public float _timeUntilIgnite = 1.0f;
    public float _speedUntilIgnite = 30.0f;
    private string _preIgniteAnimationState = "SolProjectile_PreIgnite";
    public float _preIgniteGlowBrightness = 1.0f;
    public float _preIgniteGlowScale = 1.0f; // uniform scale

    // LATER IGNITE STATE
    public float _durationOfIgnite = 6.5f;
    public float _igniteShockwaveInterval = 1.0f;
    private float _igniteShockwaveTimer = 0.0f;
    private string _igniteAnimationState = "SolProjectile_Ignite";
    private Vector2 _igniteShockwaveArea = new Vector2(8, 6);
    public AnimationCurve _igniteGlowBrightnessCurve;
    public AnimationCurve _igniteGlowScaleCurve;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentSolProjectileState)
        {
            case SolProjectileState.PREIGNITE:
                PreIgniteState();
                break;
            case SolProjectileState.IGNITE:
                IgniteState();
                break;
            case SolProjectileState.DESPAWN:
                break;
        }
    }

    private void PreIgniteState()
    {
        if (_stateTimer < 0.2)
        {
            _projectilePhysics.ObjectSprite.GetComponent<TrailRenderer>().Clear();

        }
        else
        {
            _projectilePhysics.ObjectSprite.GetComponent<TrailRenderer>().emitting = true;
        }
        _stateTimer += Time.deltaTime;
        _projectilePhysics.ObjectSprite.GetComponent<Animator>().Play(_preIgniteAnimationState);
        _projectilePhysics.GlowSprite.material.SetFloat("_Opacity", _preIgniteGlowBrightness);
        _projectilePhysics.GlowSprite.transform.localScale = Vector3.one * _preIgniteGlowScale;

        if (_stateTimer > _timeUntilIgnite || _projectilePhysics.HasHitEnemy)
        {
            _currentSolProjectileState = SolProjectileState.IGNITE;
            _stateTimer = 0.0f;
            _projectilePhysics.Speed = 0.0f;
            _projectilePhysics.Gravity = 0.0f;
            _projectilePhysics.ZVelocity = 0.0f;

            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(_projectilePhysics.transform.position, _igniteShockwaveArea, 0.0f);
            foreach (Collider2D obj in hitobjects)
            {
                if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
                {
                    if (obj.GetComponent<EntityPhysics>().GetTopHeight() > _projectilePhysics.GetBottomHeight() - 2.0f && obj.GetComponent<EntityPhysics>().GetBottomHeight() < _projectilePhysics.GetTopHeight() + 2.0f)
                    {
                        //FollowingCamera.GetComponent<CameraScript>().Jolt(0.2f, aimDirection);
                        //FollowingCamera.GetComponent<CameraScript>().Shake(0.5f, 10, 0.01f);
                        obj.GetComponent<EntityPhysics>().Inflict(1, force: Vector2.zero, type: ElementType.FIRE);
                        obj.GetComponent<EntityPhysics>().Burn();
                        obj.GetComponent<EntityPhysics>().Stagger();
                    }
                }
            }
        }
    }

    private void IgniteState()
    {
        _stateTimer += Time.deltaTime;
        _igniteShockwaveTimer += Time.deltaTime;
        _projectilePhysics.ObjectSprite.GetComponent<Animator>().Play(_igniteAnimationState);

        _projectilePhysics.GlowSprite.material.SetFloat("_Opacity", _igniteGlowBrightnessCurve.Evaluate(_igniteShockwaveTimer / _igniteShockwaveInterval));
        _projectilePhysics.GlowSprite.transform.localScale = Vector3.one * _igniteGlowScaleCurve.Evaluate(_igniteShockwaveTimer / _igniteShockwaveInterval);


        if (_igniteShockwaveTimer > _igniteShockwaveInterval)
        {
            _igniteShockwaveTimer -= _igniteShockwaveInterval;
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(_projectilePhysics.transform.position, _igniteShockwaveArea, 0.0f);
            foreach (Collider2D obj in hitobjects)
            {
                if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
                {
                    if (obj.GetComponent<EntityPhysics>().GetTopHeight() > _projectilePhysics.GetBottomHeight() - 2.0f && obj.GetComponent<EntityPhysics>().GetBottomHeight() < _projectilePhysics.GetTopHeight() + 2.0f)
                    {
                        //FollowingCamera.GetComponent<CameraScript>().Jolt(0.2f, aimDirection);
                        //FollowingCamera.GetComponent<CameraScript>().Shake(0.5f, 10, 0.01f);
                        obj.GetComponent<EntityPhysics>().Inflict(1, force: Vector2.zero, type: ElementType.FIRE);
                        obj.GetComponent<EntityPhysics>().Burn();
                        //obj.GetComponent<EntityPhysics>().Stagger();
                    }
                }
            }
        }

        if (_stateTimer > _durationOfIgnite)
        {
            _projectilePhysics.Despawn();
            _currentSolProjectileState = SolProjectileState.DESPAWN;
        }
    }


    private void OnEnable()
    {
        _projectilePhysics.Velocity = MoveDirection;
        _projectilePhysics.Speed = _speedUntilIgnite;
        _currentSolProjectileState = SolProjectileState.PREIGNITE;
        _igniteShockwaveTimer = 0.0f;
        _stateTimer = 0.0f;
    }
}
