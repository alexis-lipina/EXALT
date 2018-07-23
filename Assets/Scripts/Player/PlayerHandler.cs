using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// This class controls player state and contains methods for each state. It also receives input from the InputHandler and acts in accordance with said input.
/// In addition, it handles sprites, shadows, and player height
/// </summary>
public class PlayerHandler : EntityHandler
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private GameObject characterSprite;
    [SerializeField] private GameObject FollowingCamera;
    private Animator characterAnimator;
    private PlayerInventory inventory;

    [SerializeField] private UIHealthBar _healthBar;

    enum PlayerState {IDLE, RUN, JUMP, LIGHT_STAB, HEAVY_STAB, LIGHT_SWING, HEAVY_SWING};

    const string IDLE_EAST_Anim = "Anim_PlayerIdleEast";
    const string IDLE_WEST_Anim = "Anim_PlayerIdleWest";
    const string RUN_EAST_Anim = "Anim_PlayerRunEast";
    const string RUN_WEST_Anim = "Anim_PlayerRunWest";
    const string JUMP_EAST_Anim = "Anim_PlayerJumpEast";
    const string JUMP_WEST_Anim = "Anim_PlayerJumpWest";
    const string FALL_EAST_Anim = "Anim_PlayerFallEast";
    const string FALL_WEST_Anim = "Anim_PlayerFallWest";
    const string SWING_NORTH_Anim = "Anim_PlayerSwingNorth";
    const string SWING_SOUTH_Anim = "Anim_PlayerSwingSouth";
    const string SWING_EAST_Anim = "Anim_PlayerSwingEast";
    const string SWING_WEST_Anim = "Anim_PlayerSwingWest";


    private const float AttackMovementSpeed = 0.3f;

    private Weapon _equippedWeapon;

    private PlayerState CurrentState;
    private PlayerState PreviousState;
    private FaceDirection currentFaceDirection;
    private bool UpPressed;
    private bool DownPressed;
    private bool LeftPressed;
    private bool RightPressed;
    private bool JumpPressed;
    private bool AttackPressed;
    private bool hasSwung;
    


    
    private float PlayerRunSpeed;
    private float xInput; 
    private float yInput;   
    private float JumpImpulse;
    private float StateTimer;
    private bool isFlipped;
    private List<int> hitEnemies;

    void Awake()
    {
        //this.entityPhysics.GetComponent<Rigidbody2D>().MovePosition(TemporaryPersistentDataScript.getDestinationPosition());
        inventory = gameObject.GetComponent<PlayerInventory>();
        
    }

	void Start ()
    {
        SwapWeapon("NORTH"); //Debug
        //Debug.Log(_equippedWeapon);
        CurrentState = PlayerState.IDLE;
        StateTimer = 0;
        JumpImpulse = 0.6f;
        //playerRigidBody = PhysicsObject.GetComponent<Rigidbody2D>();
        
        //TerrainTouched.Add(666, new KeyValuePair<float, float>(0.0f, -20.0f));
        characterAnimator = characterSprite.GetComponent<Animator>();
        hasSwung = false;
        hitEnemies = new List<int>();
    }


    void Update ()
    {
        //---------------------------| Manage State Machine |
        this.ExecuteState();
        //updateHeight();
        //moveCharacterPosition();
        //reset button presses
        JumpPressed = false;
        AttackPressed = false;
        PreviousState = CurrentState;
        //FollowingCamera.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, -100);

        if (_inputHandler.DPadNorth > 0)
        {
            SwapWeapon("NORTH");
        }
        else if (_inputHandler.DPadSouth > 0)
        {
            SwapWeapon("SOUTH");
        }
        else if (_inputHandler.DPadWest > 0)
        {
            SwapWeapon("WEST");
        }
        else if (_inputHandler.DPadEast > 0)
        {
            SwapWeapon("EAST");
        }

        //TODO : Temporary gun testing
        if (_inputHandler.RightTrigger > 0.2)
        {
            if (_equippedWeapon.CanFireBullet())
            {
                FireBullet();
            }

        }

        if (_inputHandler.RightBumper > 0.2)
        {
            ThrowGrenade();
        }
        
    }

    protected override void ExecuteState()
    {
        switch (CurrentState)
        {
            case (PlayerState.IDLE):
                PlayerIdle();
                break;
            case (PlayerState.RUN):
                PlayerRun();
                break;
            case (PlayerState.JUMP):
                //characterAnimator.Play(JUMP_Anim);
                PlayerJump();
                break;
            case (PlayerState.LIGHT_STAB):
                if (isFlipped)
                {
                    FlipCharacterSprite();
                    isFlipped = false;
                }
                PlayerLightStab();
                break;
            case (PlayerState.HEAVY_STAB):
                PlayerHeavyStab();
                break;
            case (PlayerState.LIGHT_SWING):
                PlayerLightSwing();
                break;
            case (PlayerState.HEAVY_SWING):
                PlayerHeavySwing();
                break;
        }
    }
    
    private void FlipCharacterSprite()
    {
        Vector3 theScale = characterSprite.transform.localScale;
        theScale.x *= -1;
        characterSprite.transform.localScale = theScale;
    }

    //================================================================================| STATE METHODS |
    #region State Methods
    private void PlayerIdle()
    {
        //Draw
        if (currentFaceDirection == FaceDirection.EAST)
        {
            characterAnimator.Play(IDLE_EAST_Anim);
        }
        else
        {
            characterAnimator.Play(IDLE_WEST_Anim);
        }
        
        //do nothing, maybe later have them breathing or getting bored, sitting down
        entityPhysics.MoveCharacterPositionPhysics(0, 0);
        //Debug.Log("Player Idle");
        //------------------------------------------------| STATE CHANGE
        if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2) 
        {
            //Debug.Log("IDLE -> RUN");
            CurrentState = PlayerState.RUN;
        }
        if (JumpPressed)
        {
            //Debug.Log("IDLE -> JUMP");
            entityPhysics.ZVelocity = JumpImpulse;
            JumpPressed = false;
            CurrentState = PlayerState.JUMP;
        }

        if (AttackPressed)
        {
            hasSwung = false;
            //Debug.Log("IDLE -> ATTACK");
            StateTimer = 0.25f;
            CurrentState = PlayerState.LIGHT_STAB;
        }

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }
        
    }

    private void PlayerRun()
    {
        //Face Direction Determination
        Vector2 direction = new Vector2(xInput, yInput);
        if (Vector2.Angle(new Vector2(1, 0), direction) < 45)
        {
            currentFaceDirection = FaceDirection.EAST;
        }
        else if (Vector2.Angle(new Vector2(0, 1), direction) < 45)
        {
            currentFaceDirection = FaceDirection.NORTH;
        }
        else if (Vector2.Angle(new Vector2(0, -1), direction) < 45)
        {
            currentFaceDirection = FaceDirection.SOUTH;
        }
        else if (Vector2.Angle(new Vector2(-1, 0), direction) < 45)
        {
            currentFaceDirection = FaceDirection.WEST;
        }
        //Draw
        if (xInput > 0)
        {
            characterAnimator.Play(RUN_EAST_Anim);
        }
        else
        {
            characterAnimator.Play(RUN_WEST_Anim);
        }


        //Debug.Log("Player Running");
        //------------------------------------------------| MOVE
        entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        //face direction determination
        
        
        //-------| Z Azis Traversal 
        // handles falling if player is above ground
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }
        //------------------------------------------------| STATE CHANGE
        //Debug.Log("X:" + xInput + "Y:" + yInput);
        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            //Debug.Log("RUN -> IDLE");
            CurrentState = PlayerState.IDLE;
        }
        if (JumpPressed)
        {
            entityPhysics.SavePosition();
            //Debug.Log("RUN -> JUMP");
            entityPhysics.ZVelocity = JumpImpulse;
            JumpPressed = false;
            CurrentState = PlayerState.JUMP;
        }
        if (AttackPressed)
        {
            //Debug.Log("RUN -> ATTACK");
            hasSwung = false;
            StateTimer = 0.25f;
            CurrentState = PlayerState.LIGHT_STAB;
        }


        if (CurrentState == PlayerState.RUN)
        {
            entityPhysics.SavePosition();
        }
    }

    private void PlayerJump()
    {
        //Debug.Log("Player Jumping");
        //Facing Determination

        Vector2 direction = new Vector2(xInput, yInput);
        if (Vector2.Angle(new Vector2(1, 0), direction) < 45)
        {
            currentFaceDirection = FaceDirection.EAST;
        }
        else if (Vector2.Angle(new Vector2(0, 1), direction) < 45)
        {
            currentFaceDirection = FaceDirection.NORTH;
        }
        else if (Vector2.Angle(new Vector2(0, -1), direction) < 45)
        {
            currentFaceDirection = FaceDirection.SOUTH;
        }
        else if (Vector2.Angle(new Vector2(-1, 0), direction) < 45)
        {
            currentFaceDirection = FaceDirection.WEST;
        }


        //DRAW

        if (entityPhysics.ZVelocity > 0 && currentFaceDirection == FaceDirection.EAST)
        {
            characterAnimator.Play(JUMP_EAST_Anim);
        }
        else if (entityPhysics.ZVelocity < 0 && currentFaceDirection == FaceDirection.EAST)
        {
            characterAnimator.Play(FALL_EAST_Anim);
        }
        else if (entityPhysics.ZVelocity > 0 )
        {
            characterAnimator.Play(JUMP_WEST_Anim);
        }
        else if (entityPhysics.ZVelocity < 0)
        {
            characterAnimator.Play(FALL_WEST_Anim);
        }
        //------------------------------| MOVE

        entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        entityPhysics.FreeFall();
        /*
        EntityPhysics.SetObjectElevation(EntityPhysics.GetObjectElevation() + EntityPhysics.ZVelocity);
        
        EntityPhysics.ZVelocity -= 0.03f;
        */
        //------------------------------| STATE CHANGE

        //Check for foot collision

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        //EntityPhysics.CheckHitHeadOnCeiling();
        //if (entityPhysics.TestFeetCollision())


        if (entityPhysics.GetObjectElevation() <= maxheight)
        {
            entityPhysics.SetObjectElevation(maxheight);
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                entityPhysics.SavePosition();
                //Debug.Log("JUMP -> IDLE");
                CurrentState = PlayerState.IDLE;
            }
            else
            {
                //Debug.Log("JUMP -> RUN");
                CurrentState = PlayerState.RUN;
            }
        }
    }

    private void PlayerLightStab()//====================| ATTACK 
    {
        entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        Vector2 swingboxpos = Vector2.zero;
        Vector2 thrustdirection = Vector2.zero;
        switch (currentFaceDirection)
        {
            case FaceDirection.EAST:
                characterAnimator.Play(SWING_EAST_Anim);
                thrustdirection = new Vector2(1, 0);
                //entityPhysics.MoveWithCollision(AttackMovementSpeed, 0);
                swingboxpos = new Vector2(entityPhysics.transform.position.x + 2, entityPhysics.transform.position.y);
                break;
            case FaceDirection.WEST:
                characterAnimator.Play(SWING_WEST_Anim);
                thrustdirection = new Vector2(-1, 0);
                //entityPhysics.MoveWithCollision(-AttackMovementSpeed, 0);
                swingboxpos = new Vector2(entityPhysics.transform.position.x - 2, entityPhysics.transform.position.y);
                break;
            case FaceDirection.NORTH:
                characterAnimator.Play(SWING_NORTH_Anim);
                thrustdirection = new Vector2(0, 1);
                //entityPhysics.MoveWithCollision(0, AttackMovementSpeed);
                swingboxpos = new Vector2(entityPhysics.transform.position.x, entityPhysics.transform.position.y + 2);
                break;
            case FaceDirection.SOUTH:
                characterAnimator.Play(SWING_SOUTH_Anim);
                thrustdirection = new Vector2(0, -1);
                //entityPhysics.MoveWithCollision(0, -AttackMovementSpeed);
                swingboxpos = new Vector2(entityPhysics.transform.position.x, entityPhysics.transform.position.y - 2);
                break;
        }
        entityPhysics.MoveCharacterPositionPhysics(thrustdirection.x*AttackMovementSpeed, thrustdirection.y*AttackMovementSpeed);   
        //-----| Hitbox - the one directly below only flashes for one frame 
        /*
        if (!hasSwung)
        {
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(swingboxpos, new Vector2(4, 3), 0);
            foreach (Collider2D hit in hitobjects)
            {
                if (hit.tag == "Enemy")
                {
                    hit.gameObject.GetComponent<EntityColliderScript>().Inflict(1.0f);
                }
            }
            hasSwung = true;
        }
        */
        //-----| Hitbox - This one is active for the entire time and only should deal damage to a given enemy once.
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(swingboxpos, new Vector2(4, 3), 0);
        foreach (Collider2D hit in hitobjects)
        {
            if (hit.tag == "Enemy")
            {
                int temp = hit.GetComponent<EntityPhysics>().GetInstanceID();

                if (!hitEnemies.Contains(temp) && 
                    hit.gameObject.GetComponent<PhysicsObject>().GetBottomHeight() < entityPhysics.GetTopHeight() && hit.gameObject.GetComponent<PhysicsObject>().GetTopHeight() > entityPhysics.GetBottomHeight())
                {
                    Debug.Log("thrustdirection:" + thrustdirection);
                    hit.GetComponent<EntityPhysics>().Inflict(1.0f, thrustdirection, 2f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 6, 0.01f);
                    //FollowingCamera.GetComponent<CameraScript>().Jolt(0.5f, new Vector2(xInput, yInput));
                    hitEnemies.Add(temp);

                }
            }
        }
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }

        StateTimer -= Time.deltaTime;
        if (StateTimer < 0)
        {
            CurrentState = PlayerState.RUN;
            hitEnemies.Clear();
        }
    }
    private void PlayerHeavyStab()
    {
        //todo
    }
    private void PlayerLightSwing()
    {
        //todo
    }
    private void PlayerHeavySwing()
    {
        //todo
    }
    #endregion

    //================================================================================| FIRE BULLETS

    /// <summary>
    /// Fires a bullet
    /// </summary>
    private void FireBullet()
    {
        Vector2 _tempRightAnalogDirection = Vector2.zero;
        if (_inputHandler.RightAnalog.magnitude <= 0.2)
        {
            //_tempRightAnalogDirection = _tempRightAnalogDirection.normalized;
            if (_inputHandler.LeftAnalog.magnitude >= 0.2)
            {
                _tempRightAnalogDirection = _inputHandler.LeftAnalog;
            }
            else
            {
                _tempRightAnalogDirection = _tempRightAnalogDirection.normalized * 0.2f;
            }
        }
        else
        {
            _tempRightAnalogDirection = _inputHandler.RightAnalog;
        }

        GameObject tempBullet = _equippedWeapon.FireBullet(_tempRightAnalogDirection);
        //tempBullet.GetComponentInChildren<EntityPhysics>().NavManager = entityPhysics.NavManager;
        tempBullet.GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(entityPhysics.GetObjectElevation() + 2.0f);
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<Rigidbody2D>().position = (entityPhysics.GetComponent<Rigidbody2D>().position);
    }
    
    /// <summary>
    /// Swaps weapon with one from your inventory given a d-pad direction
    /// </summary>
    /// <param name="cardinal"></param>
    private void SwapWeapon(string cardinal)
    {
        Weapon temp = inventory.GetWeapon(cardinal);
        if (temp) //not null
        {
            Debug.Log("Equipping " + temp);
            _equippedWeapon = inventory.GetWeapon(cardinal);
            _equippedWeapon.PopulateBulletPool();
        }
    }

    /// <summary>
    /// Throws a grenade in the direction of aim.
    /// </summary>
    private void ThrowGrenade()
    {
        GameObject tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/TestGrenade")) as GameObject;
        tempBullet.GetComponentInChildren<GrenadeHandler>().MoveDirection = Vector2.right;
        tempBullet.SetActive(true);
        tempBullet.GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(entityPhysics.GetObjectElevation());
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<Transform>().position = (entityPhysics.GetComponent<Rigidbody2D>().position);
    }

    //================================================================================| SETTERS FOR INPUT |
    
    public void SetUpPressed(bool isPressed)
    {
        UpPressed = isPressed;
    }
    public void SetDownPressed(bool isPressed)
    {
        DownPressed = isPressed;
    }
    public void SetLeftPressed(bool isPressed)
    {
        LeftPressed = isPressed;
    }
    public void SetRightPressed(bool isPressed)
    {
        RightPressed = isPressed;
    }
    public void SetJumpPressed(bool isPressed)
    {
        JumpPressed = isPressed;
    }
    public void SetAttackPressed(bool isPressed)
    {
        AttackPressed = isPressed;
    }
    



    public override void SetXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    public override void JustGotHit()
    {
        Debug.Log("Player: Ow!");
        FollowingCamera.GetComponent<CameraScript>().Shake(1f, 6, 0.01f);
        _healthBar.UpdateBar((int)entityPhysics.GetCurrentHealth());
    }


    void OnDestroy()
    {
        Debug.Log("Reload");
        //SceneManager.LoadScene("MainMenu2");
        SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);

    }


    // I think I changed my mind on having this in the player class, I kinda want it in the Reticle's code to avoid clutter here
    /*
    /// <summary>
    /// Updates the targeting reticle's position based on player input and environment 
    /// 
    ///
    /// </summary>
    private void UpdateReticle()
    {
        Vector2 reticlevector = _inputHandler.RightAnalog;

        if (reticlevector.magnitude == 0)
        {
            reticlevector = _inputHandler.LeftAnalog;
            if (reticlevector.magnitude == 0 )
            {
                //idk what to do here, maybe a private field just for the cases where this happens?
            }
        }
        RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, this.GetComponent<BoxCollider2D>().size, 0f, new Vector2(velocityX, velocityY), distance: boxCastDistance);

    }
    */
}