using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DropPylonOnHit : MonoBehaviour
{
    [SerializeField] ToggleSwitch HitSwitch;
    [SerializeField] MovingEnvironment EnvtAnimator;
    [SerializeField] EntityPhysics SwitchEntity;

    public UnityEvent OnPylonDroppedEvent;


    bool hasRun = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasRun) return;

        if (!HitSwitch.IsToggledOn)
        {
            EnvtAnimator.PlayAnim();
            SwitchEntity.SetElevation(-20);
            OnPylonDroppedEvent.Invoke();
            hasRun = true;
        }
    }
}
