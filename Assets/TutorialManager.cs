using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Directs the operation of the combat tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlayerHandler _playerHandler;
    [SerializeField] private TriggerVolume _startTrigger;

    [SerializeField] private EnvironmentPhysics _entranceBridge;
    [SerializeField] private EnvironmentPhysics _exitBridge;

    [SerializeField] private List<EnemySpawner> _spawners_West;
    [SerializeField] private List<EnemySpawner> _spawners_East;
    [SerializeField] private List<EnemySpawner> _spawners_North;
    [SerializeField] private List<EnemySpawner> _spawners_South;
    [SerializeField] private EnemySpawner _spawnerCenter;
    [SerializeField] private List<EnemySpawner> _spawners_Corners;

    private static readonly string[] STEP_FireTransition = { "Hold <ChangeStyle_Fire> to equip |fire|Flame|white| element", "" };
    private static readonly string[] STEP_VoidTransition = { "Hold <ChangeStyle_Void> to equip |void|void|white| element", "" };
    private static readonly string[] STEP_ZapTransition = { "Hold <ChangeStyle_Zap> to equip |zap|storm|white| element", "" };

    private static readonly string[] STEP_Energy_Explanation = { "Energy is spent to cast elemental magic", "" };
    private static readonly string[] STEP_Energy_Recovery = { "Recover energy by meleeing enemies", "or press <Rest> to rest" };



    private static readonly string[] STEP_SimpleMelee = { "<Melee> to melee attack", "" };

    private static readonly string[] STEP_ElementalMelee_Intro = { "Melee 3 times to perform an", "elemental melee" };
    private static readonly string[] STEP_ElementalMelee_ZapAttack = { "|zap|Zap|white| elemental melee causes", "chain lightning" };
    private static readonly string[] STEP_ElementalMelee_VoidAttack = { "|void|Void|white| elemental melee", "pushes enemies" };
    private static readonly string[] STEP_ElementalMelee_FireAttack = { "|fire|Flame|white| elemental melee", "burns enemies" };

    private static readonly string[] STEP_ElementalRanged_Intro = { "Use <RangedAttack> to throw", "elemental magic" };
    private static readonly string[] STEP_ElementalRanged_Aim_KBM = { "Use mouse cursor to aim", "" };
    private static readonly string[] STEP_ElementalRanged_Aim_Gamepad = { "Use <STICK_R> to aim", "" };
    private static readonly string[] STEP_ElementalRanged_FireAttack = { "Cast |fire|FIRE|white| elemental magic to throw", "a bouncing fireball" };
    private static readonly string[] STEP_ElementalRanged_VoidAttack = { "cast |void|Void|white| elemental magic to throw", "a seeking missile" };
    private static readonly string[] STEP_ElementalRanged_ZapAttack = { "Cast |zap|Zap|white| elemental magic to throw", "a lightning bolt" };

    private static readonly string[] STEP_ElementalPrime_Intro = { "Blink <Blink> through an enemy to place", "an elemental curse on them" }; //Prime = Curse, Detonation = Trigger
    private static readonly string[] STEP_ElementalDetonation_Intro1 = { "CURSES have no effect", "until they are TRIGGERED" };
    private static readonly string[] STEP_ElementalDetonation_Intro2 = { "Use ELEMENTAL MELEE or MAGIC", "to TRIGGER all CURSES on an enemy" };

    private static readonly string[] STEP_ElementalDetonation_Zap = { "|zap|Zap|white| Curses strike the cursed enemy", "with a damaging lightning bolt" };
    private static readonly string[] STEP_ElementalDetonation_Fire = { "|fire|FIRE|white| Curses cause a fiery explosion", "" };
    private static readonly string[] STEP_ElementalDetonation_Void = { "|void|Void|white| Curses pull nearby enemies closer", "" };


    private static readonly string[] STEP_ElementalDetonation_All1 = { "Many curses can be stacked", "on one enemy" };
    private static readonly string[] STEP_ElementalDetonation_All2 = { "|void|Stack |fire|many |zap|curses |white|on many enemies", "for widespread destruction" };

    private static readonly string[] STEP_ElementalShield_Intro1 = { "Some enemies have", "elemental shields" };
    private static readonly string[] STEP_ElementalShield_Intro2 = { "Shields can only be broken by", "matching elemental curses" };

    private static readonly string[] STEP_Heal1 = { "Hold <Heal> to spend 4 energy", "to quickly heal" };
    private static readonly string[] STEP_Heal2 = { "Or press <Rest> to rest", "" };

    private static readonly string[] STEP_Deflect = { "Melee enemy projectiles", "to deflect them" };

    private HintTextManager hintTextManager;
    private bool _playerHasBegunTutorial = false;

    [SerializeField] private Color _levelHighColor;
    [SerializeField] private Color _levelLowColor;

    [SerializeField] Material _platformMaterial;
    [SerializeField] Material _wallMaterial;

    // Start is called before the first frame update
    void Start()
    {
        _exitBridge.gameObject.SetActive(false);
        hintTextManager = HintTextManager.GetInstanceOf();

        // This functionality has been moved to LevelManager
        //Shader.SetGlobalColor("_HighColor", _levelHighColor);
        //Shader.SetGlobalColor("_LowColor", _levelLowColor);
        //Shader.SetGlobalColor("_HighColor", _levelHighColor);
        //Shader.SetGlobalColor("_LowColor", _levelLowColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (_startTrigger.IsTriggered && !_playerHasBegunTutorial)
        {
            _playerHasBegunTutorial = true;
            StartCoroutine(PlayTutorialSequence());
        }
    }

    private IEnumerator PlayTutorialSequence()
    {
        // hide the bridge! TODO : Probably should do this in a more dignified way, like have it lerp away or something
        _entranceBridge.gameObject.SetActive(false);

        hintTextManager.ShowHintText(STEP_SimpleMelee[0], STEP_SimpleMelee[1]);

        EntityPhysics enemy = _spawners_North[0].SpawnEnemy(shieldType: ElementType.NONE, isHostile: false).GetComponentInChildren<EntityPhysics>();
        bool readyToProgress = false;
        bool hasExplainedEnergy = false;


        while (!readyToProgress)
        {
            yield return new WaitForSeconds(0.5f);
            readyToProgress = enemy.GetCurrentHealth() <= 0;
        }
        readyToProgress = false;

        //======================================================| ELEMENTAL MELEE TEST - ZAP

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalMelee_Intro[0], STEP_ElementalMelee_Intro[1]);

        yield return new WaitForSeconds(5.0f);

        #region Zap Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.ZAP)
        {
            hintTextManager.HideHintText();
            
            hintTextManager.ShowHintText(STEP_ZapTransition[0], STEP_ZapTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.ZAP;
                yield return new WaitForSeconds(0.5f);
            }
        }
        readyToProgress = false;
        #endregion

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalMelee_ZapAttack[0], STEP_ElementalMelee_ZapAttack[1]);

        yield return new WaitForSeconds(3.0f);

        List<EntityPhysics> enemies = new List<EntityPhysics>();

        for (int i = 0; i < _spawners_North.Count; i++)
        {
            enemies.Add(_spawners_North[i].SpawnEnemy(isHostile:false).GetComponentInChildren<EntityPhysics>());
        }

        while (!readyToProgress)
        {
            #region Zap Element Test
            if (_playerHandler.GetElementalAttunement() != ElementType.ZAP)
            {
                hintTextManager.HideHintText();
                
                hintTextManager.ShowHintText(STEP_ZapTransition[0], STEP_ZapTransition[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.ZAP;
                    yield return new WaitForSeconds(0.5f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ElementalMelee_ZapAttack[0], STEP_ElementalMelee_ZapAttack[1]);
            }
            #endregion
            yield return new WaitForSeconds(0.5f);
            ClearDeadEnemies(ref enemies);
            readyToProgress = (enemies.Count == 0);
        }
        readyToProgress = false;
        
        //======================================================| ELEMENTAL MELEE TEST - VOID

        #region Void Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.VOID)
        {
            hintTextManager.HideHintText();
            
            hintTextManager.ShowHintText(STEP_VoidTransition[0], STEP_VoidTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.VOID;
                yield return new WaitForSeconds(0.5f);
            }
        }
        readyToProgress = false;
        #endregion

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalMelee_VoidAttack[0], STEP_ElementalMelee_VoidAttack[1]);
        
        // void melee enemies
        yield return new WaitForSeconds(3.0f);

        enemies = new List<EntityPhysics>();
        enemies.Add(_spawners_East[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[5].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_West[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_West[5].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());

        while (!readyToProgress)
        {

            #region Void Element Test
            if (_playerHandler.GetElementalAttunement() != ElementType.VOID)
            {
                hintTextManager.HideHintText();

                hintTextManager.ShowHintText(STEP_VoidTransition[0], STEP_VoidTransition[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.VOID;
                    yield return new WaitForSeconds(0.5f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ElementalMelee_VoidAttack[0], STEP_ElementalMelee_VoidAttack[1]);
            }
            #endregion
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                ClearDeadEnemies(ref enemies);
            }

            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.5f);
        }
        readyToProgress = false;

        //=======================================================| ELEMENTAL MELEE TEST - FIRE
        #region Fire Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.FIRE)
        {
            hintTextManager.HideHintText();
            
            hintTextManager.ShowHintText(STEP_FireTransition[0], STEP_FireTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.FIRE;
                yield return new WaitForSeconds(0.5f);
            }
        }
        readyToProgress = false;
        #endregion
        
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalMelee_FireAttack[0], STEP_ElementalMelee_FireAttack[1]);

        yield return new WaitForSeconds(3.0f);
        enemies.Add(_spawners_East[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_West[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_West[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());

        while (!readyToProgress)
        {
            #region Fire Element Test
            if (_playerHandler.GetElementalAttunement() != ElementType.FIRE)
            {
                hintTextManager.HideHintText();
                
                hintTextManager.ShowHintText(STEP_FireTransition[0], STEP_FireTransition[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.FIRE;
                    yield return new WaitForSeconds(0.5f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ElementalMelee_FireAttack[0], STEP_ElementalMelee_FireAttack[1]);
            }
            #endregion
            ClearDeadEnemies(ref enemies);

            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.5f);
        }
        readyToProgress = false;

        // ====================================================| ELEMENTAL RANGED TEST - FIRE
        hintTextManager.HideHintText();

        hintTextManager.ShowHintText(STEP_ElementalRanged_Intro[0], STEP_ElementalRanged_Intro[1]);

        yield return new WaitForSeconds(3.0f);

        hintTextManager.HideHintText();
        if (_playerHandler.IsUsingMouse)
        {
            hintTextManager.ShowHintText(STEP_ElementalRanged_Aim_KBM[0], STEP_ElementalRanged_Aim_KBM[1]);
        }
        else
        {
            hintTextManager.ShowHintText(STEP_ElementalRanged_Aim_Gamepad[0], STEP_ElementalRanged_Aim_Gamepad[1]);
        }
        yield return new WaitForSeconds(4.0f);

        #region Fire Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.FIRE)
        {
            hintTextManager.HideHintText();

            hintTextManager.ShowHintText(STEP_FireTransition[0], STEP_FireTransition[1]);

            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.FIRE;
                yield return new WaitForSeconds(0.5f);
            }
        }
        readyToProgress = false;
        #endregion
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalRanged_FireAttack[0], STEP_ElementalRanged_FireAttack[1]);

        yield return new WaitForSeconds(3.0f);
        enemies.Add(_spawners_East[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        while (!readyToProgress)
        {
            #region Energy Level Test
            if (_playerHandler.CurrentEnergy < 2)
            {
                if (!hasExplainedEnergy)
                {
                    hasExplainedEnergy = true;
                    hintTextManager.HideHintText();
                    hintTextManager.ShowHintText(STEP_Energy_Explanation[0], STEP_Energy_Explanation[1]);
                    yield return new WaitForSeconds(4.0f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_Energy_Recovery[0], STEP_Energy_Recovery[1]);
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.CurrentEnergy > 4;
                    yield return new WaitForSeconds(0.5f);
                }
            }
            #endregion
            #region Fire Element Test
            if (_playerHandler.GetElementalAttunement() != ElementType.FIRE)
            {
                hintTextManager.HideHintText();

                hintTextManager.ShowHintText(STEP_FireTransition[0], STEP_FireTransition[1]);

                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.FIRE;
                    yield return new WaitForSeconds(0.5f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ElementalRanged_FireAttack[0], STEP_ElementalRanged_FireAttack[1]);
            }
            #endregion

            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.5f);
        }
        readyToProgress = false;

        #region Energy Level Test
        if (_playerHandler.CurrentEnergy < 2)
        {
            if (!hasExplainedEnergy)
            {
                hasExplainedEnergy = true;
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_Energy_Explanation[0], STEP_Energy_Explanation[1]);
                yield return new WaitForSeconds(4.0f);
            }
            hintTextManager.HideHintText();
            
            hintTextManager.ShowHintText(STEP_Energy_Recovery[0], STEP_Energy_Recovery[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.CurrentEnergy > 4;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion


        //======================================================| ELEMENT RANGED TEST - VOID

        #region Void Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.VOID)
        {
            hintTextManager.HideHintText();
            hintTextManager.ShowHintText(STEP_VoidTransition[0], STEP_VoidTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.VOID;
                yield return new WaitForSeconds(0.5f);
            }
        }
        readyToProgress = false;
        #endregion
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalRanged_VoidAttack[0], STEP_ElementalRanged_VoidAttack[1]);

        yield return new WaitForSeconds(3.0f);
        enemies.Add(_spawners_West[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        while (!readyToProgress)
        {
            #region Energy Level Test
            if (_playerHandler.CurrentEnergy < 2)
            {
                if (!hasExplainedEnergy)
                {
                    hasExplainedEnergy = true;
                    hintTextManager.HideHintText();
                    hintTextManager.ShowHintText(STEP_Energy_Explanation[0], STEP_Energy_Explanation[1]);
                    yield return new WaitForSeconds(4.0f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_Energy_Recovery[0], STEP_Energy_Recovery[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.CurrentEnergy > 4;
                    yield return new WaitForSeconds(0.5f);
                }
            }
            #endregion
            #region Void Element Test
            if (_playerHandler.GetElementalAttunement() != ElementType.VOID)
            {
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_VoidTransition[0], STEP_VoidTransition[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.VOID;
                    yield return new WaitForSeconds(0.5f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ElementalRanged_VoidAttack[0], STEP_ElementalRanged_VoidAttack[1]);
            }
            #endregion

            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.5f);
        }
        readyToProgress = false;

        //======================================================| ELEMENT RANGED TEST - ZAP
        #region Zap Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.ZAP)
        {
            hintTextManager.HideHintText();
            hintTextManager.ShowHintText(STEP_ZapTransition[0], STEP_ZapTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.ZAP;
                yield return new WaitForSeconds(0.5f);
            }
        }
        readyToProgress = false;
        #endregion
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalRanged_ZapAttack[0], STEP_ElementalRanged_ZapAttack[1]);

        yield return new WaitForSeconds(3.0f);
        enemies.Add(_spawners_West[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        while (!readyToProgress)
        {
            #region Energy Level Test
            if (_playerHandler.CurrentEnergy < 2)
            {
                if (!hasExplainedEnergy)
                {
                    hasExplainedEnergy = true;
                    hintTextManager.HideHintText();
                    hintTextManager.ShowHintText(STEP_Energy_Explanation[0], STEP_Energy_Explanation[1]);
                    yield return new WaitForSeconds(4.0f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_Energy_Recovery[0], STEP_Energy_Recovery[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.CurrentEnergy > 4;
                    yield return new WaitForSeconds(0.5f);
                }
            }
            #endregion
            #region Zap Element Test
            if (_playerHandler.GetElementalAttunement() != ElementType.ZAP)
            {
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ZapTransition[0], STEP_ZapTransition[1]);
                
                while (!readyToProgress)
                {
                    readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.ZAP;
                    yield return new WaitForSeconds(0.5f);
                }
                hintTextManager.HideHintText();
                hintTextManager.ShowHintText(STEP_ElementalRanged_ZapAttack[0], STEP_ElementalRanged_ZapAttack[1]);
            }
            #endregion
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                ClearDeadEnemies(ref enemies);
            }

            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.5f);
        }
        readyToProgress = false;
        

        //============================================| PRIMING DEMO

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalPrime_Intro[0], STEP_ElementalPrime_Intro[1]);
        
        yield return new WaitForSeconds(4.0f);
        enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        while (!readyToProgress)
        {
            Collider2D[] allObjects = Physics2D.OverlapBoxAll(transform.position, new Vector2(30, 30), 0f);
            foreach(Collider2D collider in allObjects)
            {
                if (collider.GetComponent<PlayerProjection>())
                {
                    readyToProgress = true;
                }
            }
            ClearDeadEnemies(ref enemies);
            if (enemies.Count == 0)
            {
                enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            }
            yield return new WaitForSeconds(0.5f);
        }
        readyToProgress = false;
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_Intro1[0], STEP_ElementalDetonation_Intro1[1]);

        yield return new WaitForSeconds(4.0f);

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_Intro2[0], STEP_ElementalDetonation_Intro2[1]);

        ClearDeadEnemies(ref enemies);
        if (enemies.Count == 0)
        {
            enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        }

        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = (enemies.Count == 0);
            yield return new WaitForSeconds(0.2f);
        }
        //=====================================================| ZAP DETONATION

        #region Zap Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.ZAP)
        {
            hintTextManager.HideHintText();
            hintTextManager.ShowHintText(STEP_ZapTransition[0], STEP_ZapTransition[1]);
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.ZAP;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_Zap[0], STEP_ElementalDetonation_Zap[1]);

        //waits for fire projection, then waits for enemy to die
        yield return new WaitForSeconds(5.0f);
        ClearDeadEnemies(ref enemies);
        if (enemies.Count == 0)
        {
            enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        }
        while (!readyToProgress)
        {
            Collider2D[] allObjects = Physics2D.OverlapBoxAll(transform.position, new Vector2(30, 30), 0f);
            foreach (Collider2D collider in allObjects)
            {
                if (collider.GetComponent<PlayerProjection>())
                {
                    readyToProgress = true;
                }
            }
            yield return new WaitForSeconds(0.5f);
            ClearDeadEnemies(ref enemies);
            if (enemies.Count == 0)
            {
                enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            }
        }
        readyToProgress = false;
        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.2f);
        }
        readyToProgress = false;


        //=====================================================| FIRE DETONATION

        #region Fire Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.FIRE)
        {
            hintTextManager.HideHintText();
            hintTextManager.ShowHintText(STEP_FireTransition[0], STEP_FireTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.FIRE;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_Fire[0], STEP_ElementalDetonation_Fire[1]);

        //waits for fire projection, then waits for enemy to die
        yield return new WaitForSeconds(3.0f);
        ClearDeadEnemies(ref enemies);
        if (enemies.Count == 0)
        {
            enemies.Add(_spawners_South[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_South[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_South[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_South[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        }
        while (!readyToProgress)
        {
            Collider2D[] allObjects = Physics2D.OverlapBoxAll(transform.position, new Vector2(30, 30), 0f);
            foreach (Collider2D collider in allObjects)
            {
                if (collider.GetComponent<PlayerProjection>())
                {
                    readyToProgress = true;
                }
            }
            yield return new WaitForSeconds(0.5f);
            ClearDeadEnemies(ref enemies);
            if (enemies.Count == 0)
            {
                enemies.Add(_spawners_South[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_South[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_South[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_South[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            }
        }
        readyToProgress = false;
        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.2f);
        }
        readyToProgress = false;

        //====================================================| VOID DETONATION
        
        #region Void Element Test
        if (_playerHandler.GetElementalAttunement() != ElementType.VOID)
        {
            hintTextManager.HideHintText();
            hintTextManager.ShowHintText(STEP_VoidTransition[0], STEP_VoidTransition[1]);
            
            while (!readyToProgress)
            {
                readyToProgress = _playerHandler.GetElementalAttunement() == ElementType.VOID;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_Void[0], STEP_ElementalDetonation_Void[1]);

        // waits for fire projection
        yield return new WaitForSeconds(3.0f);
        ClearDeadEnemies(ref enemies);
        if (enemies.Count == 0)
        {
            enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_East[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_East[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_East[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_North[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_North[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_North[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_South[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_South[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_South[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_West[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_West[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            enemies.Add(_spawners_West[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());

        }
        while (!readyToProgress)
        {
            Collider2D[] allObjects = Physics2D.OverlapBoxAll(transform.position, new Vector2(30, 30), 0f);
            foreach (Collider2D collider in allObjects)
            {
                if (collider.GetComponent<PlayerProjection>())
                {
                    readyToProgress = true;
                }
            }
            yield return new WaitForSeconds(0.5f);
            ClearDeadEnemies(ref enemies);
            if (enemies.Count == 0)
            {
                enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_East[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_East[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_North[5].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
                enemies.Add(_spawners_North[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
            }
        }
        readyToProgress = false;
        // waits for enemy to die
        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.2f);
        }
        readyToProgress = false;

        //=====================================================| All detonations fun time!
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_All1[0], STEP_ElementalDetonation_All1[1]);
        yield return new WaitForSeconds(5.0f);
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalDetonation_All2[0], STEP_ElementalDetonation_All2[1]);
        yield return new WaitForSeconds(4.0f);

        enemies.Add(_spawners_West[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_West[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_West[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_East[5].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_North[0].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_North[2].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_North[4].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_South[1].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_South[3].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());
        enemies.Add(_spawners_South[5].SpawnEnemy(isHostile: false).GetComponentInChildren<EntityPhysics>());

        while (!readyToProgress)
        {
            yield return new WaitForSeconds(0.2f);
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
        }
        readyToProgress = false;
        

        //===============================================| SHIELD INSTRUCTION
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalShield_Intro1[0], STEP_ElementalShield_Intro1[1]);
        yield return new WaitForSeconds(3.0f);
        enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false, shieldType:ElementType.ZAP).GetComponentInChildren<EntityPhysics>());
        yield return new WaitForSeconds(2.0f);
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_ElementalShield_Intro2[0], STEP_ElementalShield_Intro2[1]); //leaving this up to the player is a bit risky, might want to prompt them to change elements?
        float delayTimer = 5.0f;
        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.2f);
            delayTimer -= 0.2f;
        }
        readyToProgress = false;

        yield return new WaitForSeconds(1.0f);

        enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false, shieldType: ElementType.VOID).GetComponentInChildren<EntityPhysics>());
        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.2f);
            delayTimer -= 0.2f;
        }
        readyToProgress = false;

        enemies.Add(_spawnerCenter.SpawnEnemy(isHostile: false, shieldType: ElementType.FIRE).GetComponentInChildren<EntityPhysics>());
        while (!readyToProgress)
        {
            ClearDeadEnemies(ref enemies);
            readyToProgress = enemies.Count == 0;
            yield return new WaitForSeconds(0.2f);
            delayTimer -= 0.2f;
        }
        readyToProgress = false;

        //==============================================| HEALING INSTRUCTION
        hintTextManager.HideHintText();
        yield return new WaitForSeconds(4.0f);
        //force inflict the player 
        for (int i = 0; i < 4; i++)
        {
            _playerHandler.GetEntityPhysics().Inflict(1);
            yield return new WaitForSeconds(0.5f);
        }

        int playerHealth = _playerHandler.GetEntityPhysics().GetCurrentHealth();
        hintTextManager.ShowHintText(STEP_Heal1[0], STEP_Heal1[1]);
        
        while (!readyToProgress)
        {
            //give player enough energy to heal
            if (_playerHandler.CurrentEnergy < 4)
            {
                _playerHandler.ChangeEnergy(4 - _playerHandler.CurrentEnergy);
            }
            yield return new WaitForSeconds(0.2f);
            //check if player has recovered at all
            readyToProgress = playerHealth < _playerHandler.GetEntityPhysics().GetCurrentHealth();
        }
        readyToProgress = false;

        hintTextManager.HideHintText();
        
        hintTextManager.ShowHintText(STEP_Heal2[0], STEP_Heal2[1]);
        
        while (!readyToProgress)
        {
            yield return new WaitForSeconds(0.2f);
            readyToProgress = _playerHandler.GetEntityPhysics().GetCurrentHealth() == _playerHandler.GetEntityPhysics().GetMaxHealth();
        }
        readyToProgress = false;

        //===================================================| DEFLECT INSTRUCTION

        hintTextManager.HideHintText();
        hintTextManager.ShowHintText(STEP_Deflect[0], STEP_Deflect[1]);
        yield return new WaitForSeconds(7.0f);
        bool deflectionExists = false;
        while (!readyToProgress)
        {
            //look for reflected projectiles
            Collider2D[] allObjects = Physics2D.OverlapBoxAll(transform.position, new Vector2(30, 30), 0f);
            foreach (Collider2D collider in allObjects)
            {
                if (collider.GetComponent<ProjectilePhysics>())
                {
                    if (collider.GetComponent<ProjectilePhysics>().GetElement() == ElementType.NONE && collider.GetComponent<ProjectilePhysics>().GetWhoToHurt() == "ENEMY")
                    {
                        deflectionExists = true;
                    }
                }
            }
            //ensure player doesnt die
            if (_playerHandler.GetEntityPhysics().GetCurrentHealth() < 2)
            {
                yield return new WaitForSeconds(0.2f);
                _playerHandler.GetEntityPhysics().Heal(1);
            }

            // respawn enemies
            ClearDeadEnemies(ref enemies);
            if (enemies.Count == 0)
            {
                if (deflectionExists)
                {
                    readyToProgress = true;
                }
                else
                {
                    enemies.Add(_spawners_Corners[Random.Range(0, 4)].SpawnEnemy(isHostile: true).GetComponentInChildren<EntityPhysics>());
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
        readyToProgress = false;
        hintTextManager.HideHintText();
        hintTextManager.ShowHintText("DONE!", "You now know everything!");
        _exitBridge.gameObject.SetActive(true);

    }


    private void ClearDeadEnemies(ref List<EntityPhysics> enemies)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i].GetCurrentHealth() <= 0)
            {
                enemies.Remove(enemies[i]);
            }
        }
        return;
    }
}
