using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this component forces this animator to sync with TargetAnimator
[RequireComponent(typeof(Animator))]
public class AnimationMirrorer : MonoBehaviour
{
    [SerializeField] Animator TargetAnimator;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().runtimeAnimatorController = TargetAnimator.runtimeAnimatorController;
        //GetComponent<Animator>().Play(TargetAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
