using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum AttackType
{
    Enemy, Boss, Tower
}

public class PlayerControl : MonoBehaviour
{
    public float forwardSpeed = 6f, attackSpeed = 5f, swipeSensitivity = 0.01f, maxX = 5f;

    [SerializeField] PlayerSpawner playerSpawner;
    [SerializeField] Transform followerParent;

    
    public static PlayerControl playerControlInstance;
    public static bool gamestate;
    public static bool inFinishZone;
    public static float progress = 0f;
    public static float characterSizeScaleModifier;

    [Header("User Settings")] 
    public static float userSensitivity;

    [Header("Camera")] public Camera mainCamera;

    [Header("Additional")] [SerializeField]
    private GameObject stickmansCounters;

    [Header("Attack")]
    public AttackType attackType = AttackType.Enemy;
    
    private float currentX = 0f;
    private Vector2 lastTouchPosition;
    private bool isTouching = false;
    public static bool allowMovement = true;
    private bool attack;
    private bool attackBoss;
    private Transform enemy;
    private float currentForwardSpeed;

    // Cannon Finish
    private bool moveTowardsObject;
    private Transform destinationObject;
    
    private int inBossCloseRange = 0;
    

    void Awake()
    {
        playerControlInstance = this;
        inFinishZone = false;
        allowMovement = true;
        moveTowardsObject = false;
    }
    void Start()
    { 
        characterSizeScaleModifier = playerSpawner.followerPrefab.transform.localScale.x;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        if (!gamestate) return;
        if (followerParent.childCount != 0 || inFinishZone)
        {
            if (attack)
            {
                Attack();
            }
            else if (moveTowardsObject)
            {
                MoveStickMansTowards();
            }
            else if(allowMovement)
                Move();

            progress = transform.position.z / (LevelGenerator.pathDistance + 12f);
            UIManager.UIManagerInstance.progressBar.fillAmount = progress;
        }
        else
        {
            Debug.Log("Game Over");
            if (attackBoss)
            {
                enemy.GetComponent<BossScript>().unsetAnimationPunch();
                attackBoss = false;
            }
            attack = false;
            gamestate = false;
            transform.GetChild(1).gameObject.SetActive(false);
            UIManager.UIManagerInstance.OpenLoseScreen();
        }
    }
    

    void Move(Vector3 direction = default, float overrideSpeed = 0)
    {
        direction = direction == default ? Vector3.forward : direction.normalized;
        if (inFinishZone) direction.x = 0;
        
        currentForwardSpeed = (attack || moveTowardsObject) ? attackSpeed : forwardSpeed;
        currentForwardSpeed = overrideSpeed == 0 ? currentForwardSpeed: overrideSpeed;
        transform.Translate(direction * (currentForwardSpeed * Time.deltaTime));
        if (inFinishZone)
        {
            transform.DOMoveX(0, 2f).SetEase(Ease.Linear);
            return;
        }
        if (attack || moveTowardsObject) return;


#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            lastTouchPosition = Input.mousePosition;
            isTouching = true;
        }
        else if (Input.GetMouseButton(0) && isTouching)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPosition;
            lastTouchPosition = Input.mousePosition;
        
            currentX += delta.x * swipeSensitivity * userSensitivity;
            CalculateMaxX();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isTouching = false;
        }
#else
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began){
                lastTouchPosition = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching){
                Vector2 delta = touch.position - lastTouchPosition;
                lastTouchPosition = touch.position;

                currentX += delta.x * swipeSensitivity * userSensitivity;
                CalculateMaxX();
            }
            else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
                isTouching = false;
            }

        }
#endif


        void CalculateMaxX()
        {
            if (playerSpawner.followers.Count < 50 / characterSizeScaleModifier)
            {
                currentX = Mathf.Clamp(currentX, -maxX, maxX);
            }
            else
            {
                float clampSizeModifier = 0.6f / characterSizeScaleModifier;
                currentX = Mathf.Clamp(currentX, -maxX * clampSizeModifier, maxX * clampSizeModifier);
            }
        }

        Vector3 newPosition = transform.position;
        newPosition.x = currentX;
        transform.position = newPosition;
    }

    public void PlayerWin()
    {
        UIManager.UIManagerInstance.OpenWinScreen();
    }

    void Attack()
    {
        if (enemy)
        {
           
            Vector3 EnemyDirection = new Vector3(enemy.position.x, transform.position.y, enemy.position.z) -
                                     transform.position;
            
            MoveStickMansTowards(EnemyDirection);

            bool moveTowardsEnemy = false;
            Transform target = null;
            
            if (attackType == AttackType.Enemy)
            {
                Transform enemyParent = enemy.GetChild(1);
                moveTowardsEnemy = enemyParent.childCount > 1;
                if (moveTowardsEnemy) target = enemyParent.GetChild(0);
                
            }
            else if (attackType == AttackType.Tower)
            {
                moveTowardsEnemy = true;
                target = enemy;
            }
            if (moveTowardsEnemy)
            {
                for (int i = 0; i < followerParent.childCount; i++)
                {
                    Vector3 Distance = target.position - followerParent.GetChild(i).position;

                    if (Distance.magnitude < 8f)
                    {
                        Transform follower = followerParent.GetChild(i);
                        follower.position =
                            Vector3.Lerp(follower.position, new Vector3(target.position.x,
                                follower.position.y,
                                target.position.z), Time.deltaTime * 1f);
                    }
                    
                    if (enemy.GetChild(1).childCount == 0)
                    {
                        attack = false;
                        enemy.gameObject.SetActive(false);
                        playerSpawner.FormatStickMan();
                    }
                }
            }
            Move(EnemyDirection);

            if (attackType == AttackType.Enemy)
            {
                if (enemy.GetChild(1).childCount == 0)
                {
                    attack = false;
                    enemy.gameObject.SetActive(false);
                    playerSpawner.FormatStickMan();
                }
            }

            if (attackType == AttackType.Tower)
            {
                // Do nothing   
            }
        }
    }

    void MoveStickMansTowards()
    {
        if (!destinationObject) return;
        Vector3 TargetDirection = new Vector3(destinationObject.position.x, transform.position.y, destinationObject.position.z) -
                                 transform.position;
            

        for (int i = 0; i < followerParent.childCount; i++)
        {
            Transform follower = followerParent.GetChild(i);
            
            follower.rotation =
                Quaternion.Slerp(follower.rotation, Quaternion.LookRotation(TargetDirection, Vector3.up),
                    Time.deltaTime * 3f);
            
            Vector3 Distance = destinationObject.position - follower.position;
            
                follower.position =
                    Vector3.Lerp(follower.position, new Vector3(destinationObject.position.x,
                        follower.position.y,
                        destinationObject.position.z), Time.deltaTime * 1f);
        }
        
        Move(TargetDirection);
    }
    
    void MoveStickMansTowards(Vector3 TargetDirection)
    {

        for (int i = 0; i < followerParent.childCount; i++)
        {
            Transform follower = followerParent.GetChild(i);
            
            follower.rotation =
                Quaternion.Slerp(follower.rotation, Quaternion.LookRotation(TargetDirection, Vector3.up),
                    Time.deltaTime * 3f);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            GateControl gateControl = other.GetComponent<GateControl>();
            gateControl.Activate(playerSpawner);
            AudioManager.instance.Play("GatePassed");
            playerSpawner.StickmansSetAnimRun();
        }
        else if (other.CompareTag("EnemyZone"))
        {
            attackType = AttackType.Enemy;
            enemy = other.transform;
            attack = true;

            other.transform.GetChild(1).GetComponent<EnemyManager>().AttackThem(transform);
        }
        else if (other.CompareTag("EnemyZoneShooting"))
        {
            attackType = AttackType.Enemy;
            EnemyShootingPrefabManager enemyShootingPrefabManager = other.GetComponent<EnemyShootingPrefabManager>();
            enemy = enemyShootingPrefabManager.enemyShootingManager.transform.parent;
            // attack = true;
            
            enemyShootingPrefabManager.enemyShootingManager.GetComponent<EnemyShootingManager>().AttackThem(transform);
        }
        else if (other.CompareTag("EnemyZoneShootingAttack"))
        {
            attack = true;
            
            other.transform.GetChild(1).GetComponent<EnemyShootingManager>().StopAutoShooting();
        }
        else if (other.CompareTag("EnemyDefenceTowerZone"))
        {
            attackType = AttackType.Tower;
            EnemyDefenceTowerZoneManager defenceTowerZoneManager= other.GetComponent<EnemyDefenceTowerZoneManager>();

            attack = true;
            enemy = defenceTowerZoneManager.enemyDefenceTower;
            defenceTowerZoneManager.enemyDefenceTowerManager.AttackThem(transform);
        }
        else if (other.CompareTag("HorizontalObstacleButton"))
        {
            TrapButton trapButton = other.GetComponent<TrapButton>();
            trapButton.ActivateButton();
        }
        else if (other.CompareTag("FinishStairs"))
        {
            playerSpawner.StickmansBuildPyramid();
            inFinishZone = true;
            stickmansCounters.SetActive(false);
            // UIManager.UIManagerInstance.OpenWinScreen();
            // gamestate = false;
            // playerSpawner.StickmansSetAnimDance();
            //
            // Debug.Log("You won");
        }
        else if (other.CompareTag("StairsFirstStep"))
        {
            playerSpawner.StickmansBuildStairs();
        }
        else if (other.CompareTag("FinishChest"))
        {
            
            gamestate = false;
            DelayOpenWinScreen();
        }
        else if (other.CompareTag("BossFinishLine"))
        {
            // Align all the stickmans to center
            UIManager.UIManagerInstance.OpenBossRangeModifierUI();
            inFinishZone = true;
        }
        else if (other.CompareTag("BossFightZone"))
        {
            attackType = AttackType.Enemy;
            transform.GetChild(1).gameObject.SetActive(false); // Disable stickmans counter;    
            enemy = other.transform.GetChild(0);
            playerSpawner.PauseRegroup();
            playerSpawner.EngageEnemy(enemy);
            
            // attackBoss = true;
            inFinishZone = false;
            allowMovement = false;
            BossScript bossScript = enemy.GetComponent<BossScript>();
            bossScript.setAnimationPunch();
            CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(bossScript.bossCinemachineCamera);
        }
        
        
        else if (other.CompareTag("FinishCanonTrigger"))
        {
            //PlayerSpawner go into cannon;
            moveTowardsObject = true;
            CannonFinishManager cannonFinishManager = other.GetComponent<CannonFinishManager>();
            destinationObject = cannonFinishManager.cannon.transform;
            StartCoroutine(GetCannonReady(cannonFinishManager));
        }
    }

    private IEnumerator GetCannonReady(CannonFinishManager cannonFinishManager)
    {
        yield return new WaitForSeconds(1f);
        inFinishZone = true;
        stickmansCounters.SetActive(false);
        cannonFinishManager.ActivateCannonUI();
        cannonFinishManager.SwitchToCannonCamera();
        // move Camera here
        yield return new WaitForSeconds(1f);
        cannonFinishManager.AllowCannonAiming();
        // cannonFinishManager.StartShooting();
        yield return new WaitForSeconds(1f);
    }
    
    
    public void DelayOpenWinScreen()
    {
        StartCoroutine(DelayOpenWinScreenCoroutine());
    }
    private IEnumerator DelayOpenWinScreenCoroutine()
    {
        CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(4);
        playerSpawner.StickmansSetAnimDance();
        yield return new WaitForSeconds(3f);
        PlayerWin();
    }

    public void StopAttack()
    {
        attack = false;
    }
    
    
    
}