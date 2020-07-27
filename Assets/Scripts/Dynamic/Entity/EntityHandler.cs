using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class provides methods and fields for all entities in the game. 
/// Entities, in this case (and in most cases in this project) encompass
/// the player, enemies, and other agents
/// </summary>
public abstract class EntityHandler : MonoBehaviour
{
    [SerializeField] protected AudioSource _primeAudioSource;
    [SerializeField] protected EntityPhysics entityPhysics;
    protected enum FaceDirection {NORTH, WEST, SOUTH, EAST }

    // prime / detonation fields
    protected bool _isPrimed_Void = false;
    protected GameObject _voidPrimeVfx;
    protected bool _isPrimed_Zap = false;
    protected GameObject _zapPrimeVfx;
    protected bool _isPrimed_Fire = false;
    protected GameObject _firePrimeVfx;
    protected List<ElementType> currentPrimes;
    protected bool _isDetonating = false;

    protected ElementType _shieldType = ElementType.NONE;

    protected Vector2 DeathVector = Vector2.zero;

    public void SetDeathVector(Vector2 value)
    {
        DeathVector = value;
    }


    public static Color GetElementColor(ElementType type)
    {
        switch (type)
        {
            case ElementType.FIRE:
                return new Color(1f, 0.5f, 0f, 1f);
            case ElementType.VOID:
                return new Color(0.5f, 0.0f, 1.0f, 1f);
            case ElementType.ZAP:
                return new Color(0.0f, 1.0f, 0.5f, 1f);
            default:
                Debug.LogWarning("GetElementColor() called with type NONE");
                return new Color(1f, 0.2f, 0.1f, 1f);
        }
    }



    /// <summary>
    /// Contains the state machine switch statement and calls state methods
    /// </summary>
    protected abstract void ExecuteState();
    public abstract void SetXYAnalogInput(float x, float y);
    public EntityPhysics GetEntityPhysics()
    {
        return entityPhysics;
    }

    public abstract void JustGotHit(Vector2 hitDirection);
    public virtual void OnDeath()
    {
        GameObject.Destroy(transform.parent);
    }

    public void PerformDetonations(ElementType elementOfAttack)
    {
        if (!(_isPrimed_Fire || _isPrimed_Void || _isPrimed_Zap) || elementOfAttack == ElementType.NONE) return;

        // decided against adding detonating attack to prime stack
        /*
        if (!currentPrimes.Contains(elementOfAttack))
        {
            PrimeEnemy(elementOfAttack);
        }*/

        List<ElementType> detonations = new List<ElementType>();

        foreach (ElementType element in currentPrimes)
        {
            switch (element)
            {
                case ElementType.FIRE:
                    Debug.Log("Fire Detonation");
                    _isPrimed_Fire = false;
                    //Destroy(_firePrimeVfx);
                    detonations.Add(ElementType.FIRE);
                    break;
                case ElementType.ZAP:
                    Debug.Log("Zap Detonation");
                    _isPrimed_Zap = false;
                    //Destroy(_zapPrimeVfx);
                    detonations.Add(ElementType.ZAP);
                    break;
                case ElementType.VOID:
                    Debug.Log("Void Detonation");
                    _isPrimed_Void = false;
                    //Destroy(_voidPrimeVfx);
                    detonations.Add(ElementType.VOID);
                    break;
            }
        }
        currentPrimes = new List<ElementType>();

        StartCoroutine(ExecuteDetonations(detonations));


        //Debug.Log("Blam!");
    }

    public void PrimeEnemy(ElementType type)
    {
        if (_isPrimed_Void && type == ElementType.VOID || _isPrimed_Fire && type == ElementType.FIRE || _isPrimed_Zap && type == ElementType.ZAP) return;
        _primeAudioSource.Play();
        switch (type)
        {
            case ElementType.FIRE:
                _isPrimed_Fire = true;
                //_firePrimeVfx = Instantiate(Resources.Load("Prefabs/VFX/PrimedParticles_Fire", typeof(GameObject)) as GameObject, entityPhysics.ObjectSprite.transform);
                _firePrimeVfx = FireDetonationHandler.DeployFromPool(entityPhysics);
                currentPrimes.Add(type);
                StartCoroutine(PrimeFlash(type));
                return;
            case ElementType.VOID:
                _isPrimed_Void = true;
                //_voidPrimeVfx = Instantiate(Resources.Load("Prefabs/VFX/PrimedParticles_Void", typeof(GameObject)) as GameObject, entityPhysics.ObjectSprite.transform);
                _voidPrimeVfx = VoidDetonationHandler.DeployFromPool(entityPhysics);
                currentPrimes.Add(type);
                StartCoroutine(PrimeFlash(type));
                return;
            case ElementType.ZAP:
                _isPrimed_Zap = true;
                //_zapPrimeVfx = Instantiate(Resources.Load("Prefabs/VFX/PrimedParticles_Zap", typeof(GameObject)) as GameObject, entityPhysics.ObjectSprite.transform);
                _zapPrimeVfx = ZapDetonationHandler.DeployFromPool(entityPhysics);
                currentPrimes.Add(type);
                StartCoroutine(PrimeFlash(type));
                return;
        }
    }

    IEnumerator ExecuteDetonations(List<ElementType> detonations)
    {
        Debug.Log("DETONATING?????");
        _isDetonating = true;
        yield return new WaitForSeconds(0.3f);
        if (detonations.Contains(_shieldType)) {  BreakShield();  } //Shield break on detonate

        List<ElementType> orderedDetonations = new List<ElementType>();

        if (detonations.Contains(ElementType.VOID)) orderedDetonations.Add(ElementType.VOID);
        if (detonations.Contains(ElementType.FIRE)) orderedDetonations.Add(ElementType.FIRE);
        if (detonations.Contains(ElementType.ZAP)) orderedDetonations.Add(ElementType.ZAP);

        foreach (ElementType element in orderedDetonations)
        {
            switch (element)
            {
                case ElementType.FIRE:
                    //FireDetonationHandler.DeployFromPool(entityPhysics);
                    Debug.Log("DETONATING FIRE!!!");
                    _firePrimeVfx.GetComponent<FireDetonationHandler>().Detonate();
                    _firePrimeVfx = null;
                    break;
                case ElementType.VOID:
                    //VoidDetonationHandler.DeployFromPool(entityPhysics);
                    _voidPrimeVfx.GetComponent<VoidDetonationHandler>().Detonate();
                    _voidPrimeVfx = null;
                    break;
                case ElementType.ZAP:
                    _zapPrimeVfx.GetComponent<ZapDetonationHandler>().Detonate();
                    _zapPrimeVfx = null;
                    break;
            }
            //yield return new WaitForSeconds(0.3f); //TIME BETWEEN DETONATIONS
            yield return new WaitForSeconds(Random.Range(0.22f, 0.38f)); //TIME BETWEEN DETONATIONS
        }
        _isDetonating = false;
        if (entityPhysics.GetCurrentHealth() <= 0) OnDeath();
    }

    IEnumerator PrimeFlash(ElementType type)
    {
        //Debug.Log("TakeDamageFlash entered");
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 1);
        for (float i = 0; i < 1; i++)
        {
            //characterSprite.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", new Color(0, 0, 0, 1));
            yield return new WaitForSeconds(0.08f);
            //characterSprite.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
            entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", GetElementColor(type));
            
            yield return new WaitForSeconds(0.08f);
            
        }

        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 0);
    }

    /// <summary>
    /// Give the entity a shield of a certain elemental type
    /// </summary>
    /// <param name="elementToMakeShield"></param>
    public virtual void ActivateShield(ElementType elementToMakeShield)
    {
        _shieldType = elementToMakeShield;
    }

    public ElementType GetShield()
    {
        return _shieldType;
    }

    public virtual void BreakShield()
    {
        Debug.Log("CRACK! Shield broken!");
        _shieldType = ElementType.NONE;
    }
}
