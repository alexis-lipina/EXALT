using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBlockHandler : EntityHandler
{
    



    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector2 v = entityPhysics.MoveAvoidEntities(Vector2.up * 0.01f);
        entityPhysics.MoveWithCollision(v.x, v.y);
	}




    public override void JustGotHit()
    {
        throw new NotImplementedException();
    }

    public override void SetXYAnalogInput(float x, float y)
    {
        throw new NotImplementedException();
    }

    protected override void ExecuteState()
    {
        throw new NotImplementedException();
    }

    public override void OnDeath()
    {
        return;
    }
}
