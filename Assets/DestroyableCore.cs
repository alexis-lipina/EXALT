using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyableCore : MonoBehaviour
{
    [SerializeField] SwitchPhysics HittableSwitch;
    [SerializeField] PlayerHandler playerHandler;
    [SerializeField] ToggleSwitch[] Switches;
    [SerializeField] ForceFieldFX ShieldFX;
    [SerializeField] SpriteRenderer CrystalSpriteRenderer;
    [SerializeField] Sprite ShatteredSprite_Right;
    [SerializeField] Sprite ShatteredSprite_Left;
    bool IsShieldDisabled;

    public UnityEvent OnCoreDestroyed;


    // Start is called before the first frame update
    void Start()
    {
        HittableSwitch.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsShieldDisabled)
        {
            if (HittableSwitch.IsOn)
            {
                OnShatter(playerHandler.GetLookDirection().x > 0.0f);
            }
        }
        else
        {
            CheckIsVulnerable();
        }
    }

    void OnShatter(bool FaceRight)
    {
        playerHandler.ShatterHealth();
        Destroy(gameObject);
        if (FaceRight)
        {
            CrystalSpriteRenderer.sprite = ShatteredSprite_Right;
            //CrystalSpriteRenderer.size = CrystalSpriteRenderer.size * new Vector2(4.0f, 1.0f);
            CrystalSpriteRenderer.gameObject.transform.position += new Vector3(2.0f, 0.0f, 0.0f);
        }
        else
        {
            CrystalSpriteRenderer.sprite = ShatteredSprite_Left;
            //CrystalSpriteRenderer.size = CrystalSpriteRenderer.size * new Vector2(4.0f, 1.0f);
            CrystalSpriteRenderer.gameObject.transform.position += new Vector3(-2.0f, 0.0f, 0.0f);
        }
        OnCoreDestroyed.Invoke();
    }

    void CheckIsVulnerable()
    {
        foreach (ToggleSwitch powerswitch in Switches)
        {
            if (powerswitch.IsToggledOn)
            {
                return;
            }
        }
        IsShieldDisabled = true;
        ShieldFX.GetComponent<SpriteRenderer>().enabled = false;
        HittableSwitch.gameObject.SetActive(true);
    }
}
