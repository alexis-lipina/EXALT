using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftProjectileHandler : BulletHandler
{
    EntityPhysics TargetedEnemy;
    public Vector2 targetingArea;

    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(_projectilePhysics.transform.position, targetingArea, 0.0f);
        foreach (Collider2D obj in hitobjects)
        {
            if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
            {
                if (TargetedEnemy == null)
                {
                    TargetedEnemy = obj.GetComponent<EntityPhysics>();
                }
                if ((TargetedEnemy.transform.position - _projectilePhysics.transform.position).sqrMagnitude > (obj.transform.position - _projectilePhysics.transform.position).sqrMagnitude)
                {
                    TargetedEnemy = obj.GetComponent<EntityPhysics>();
                }
            }
        }
    }
}