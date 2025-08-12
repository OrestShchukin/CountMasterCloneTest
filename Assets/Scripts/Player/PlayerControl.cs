using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
    
    private float currentX = 0f;
    private Vector2 lastTouchPosition;
    private bool isTouching = false;
    public static bool allowMovement = true;
    private bool attack;
    private bool attackBoss;
    private Transform enemy;
    private float currentForwardSpeed;

    private int inBossCloseRange = 0;
    


    void Awake()
    {
        playerControlInstance = this;
        inFinishZone = false;
        allowMovement = true;
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
            else if (attackBoss)
            {
                AttackBoss();
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
            Debug.Log($"{transform.GetChild(0).gameObject.name} set to false");
            UIManager.UIManagerInstance.OpenLoseScreen();
        }
    }
    

    void Move(Vector3 direction = default, float overrideSpeed = 0)
    {
        direction = direction == default ? Vector3.forward : direction.normalized;
        if (inFinishZone) direction.x = 0;
        
        currentForwardSpeed = attack ? attackSpeed : forwardSpeed;
        currentForwardSpeed = overrideSpeed == 0 ? currentForwardSpeed: overrideSpeed;
        transform.Translate(direction * (currentForwardSpeed * Time.deltaTime));
        if (inFinishZone)
        {
            transform.DOMoveX(0, 2f).SetEase(Ease.Linear);
            return;
        }
        if (attack) return;


#if UNITY_EDITOR // Переписати для нової input system;
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
            

            for (int i = 0; i < followerParent.childCount; i++)
            {
                Transform follower = followerParent.GetChild(i);

                // Vector3 EnemyDirection = new Vector3(enemy.position.x, transform.position.y, enemy.position.z) - follower.position;
                follower.rotation =
                    Quaternion.Slerp(follower.rotation, Quaternion.LookRotation(EnemyDirection, Vector3.up),
                        Time.deltaTime * 3f);
            }

            Transform enemyParent = enemy.GetChild(1);
            if (enemyParent.childCount > 1)
            {
                for (int i = 0; i < followerParent.childCount; i++)
                {
                    Vector3 Distance = enemyParent.GetChild(0).position - followerParent.GetChild(i).position;

                    if (Distance.magnitude < 8f)
                    {
                        Transform follower = followerParent.GetChild(i);
                        follower.position =
                            Vector3.Lerp(follower.position, new Vector3(enemyParent.GetChild(0).position.x,
                                follower.position.y,
                                enemyParent.GetChild(0).position.z), Time.deltaTime * 1f);
                    }
                }
            }
            Move(EnemyDirection);
            
            if (enemy.GetChild(1).childCount == 0)
            {
                attack = false;
                enemy.gameObject.SetActive(false);
                playerSpawner.FormatStickMan();
            }
        }
    }

    // void AttackBoss()
    // {
    //     Vector3 enemyDirection = new Vector3(enemy.position.x, transform.position.y, enemy.position.z) -
    //                              transform.position;
    //     Vector3 enemyPosition = enemyDirection;
    //     
    //     
    //     // Move(enemyPosition ,1f);
    //     for (int i = 0; i < followerParent.childCount; i++)
    //     {
    //         Transform follower = followerParent.GetChild(i);
    //         
    //         follower.rotation =
    //             Quaternion.Slerp(follower.rotation, Quaternion.LookRotation(enemyDirection, Vector3.up),
    //                 Time.deltaTime * 3f);
    //         // Debug.Log($"child({i}) is in close range and should move");
    //     }
    //     
    //     
    //     if (followerParent.childCount > 1)
    //     {
    //         bool moveCloser = inBossCloseRange < 5;
    //         if (moveCloser) inBossCloseRange = 0;
    //         
    //         for (int i = 0; i < followerParent.childCount; i++)
    //         {
    //             Vector3 Distance = enemy.position - followerParent.GetChild(i).position;
    //
    //             if (Distance.magnitude < 4f)
    //             {
    //                 inBossCloseRange++;
    //                 Transform follower = followerParent.GetChild(i);
    //                 follower.position =
    //                     Vector3.Lerp(follower.position, new Vector3(enemy.position.x,
    //                         follower.position.y,
    //                         enemy.position.z), Time.deltaTime * 0.2f);
    //                 Debug.Log($"Enemy Position {enemy.position},  Distance {Distance.magnitude}, follower position {follower.position}");
    //             }
    //             
    //             if (!moveCloser) return;
    //             
    //             if (Distance.magnitude < 8f)
    //             {
    //                 Transform follower = followerParent.GetChild(i);
    //                 follower.position =
    //                     Vector3.Lerp(follower.position, new Vector3(enemy.position.x,
    //                         follower.position.y,
    //                         enemy.position.z), Time.deltaTime * 0.2f);
    //             }
    //             else if (Distance.magnitude < 12f)
    //             {
    //                 Transform follower = followerParent.GetChild(i);
    //                 follower.position =
    //                     Vector3.Lerp(follower.position, new Vector3(follower.position.x,
    //                         enemy.position.y,
    //                         enemy.position.z), Time.deltaTime * 0.2f);
    //             }
    //             
    //             else if (Distance.magnitude > 12f)
    //             {
    //                 Transform follower = followerParent.GetChild(i);
    //                 follower.position =
    //                     Vector3.Lerp(follower.position, new Vector3(follower.position.x,
    //                         follower.position.y,
    //                         enemy.position.z), Time.deltaTime * 0.1f);
    //             }
    //             
    //         }
    //     }
    //     playerSpawner.PauseRegroup();
    //
    // }
    
    void AttackBoss()
    {
        if (enemy == null || followerParent == null) return;
        for (int i = 0; i < followerParent.childCount; i++)
        {
            Transform follower = followerParent.GetChild(i);
            Vector3 toEnemy = enemy.position - follower.position;

            // Повернути фоловера обличчям до ворога
            if (toEnemy.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(new Vector3(toEnemy.x, 0f, toEnemy.z), Vector3.up);
                follower.rotation = Quaternion.Slerp(follower.rotation, look, Time.deltaTime * 3f);
            }

            float dist = toEnemy.magnitude;
            float zDistance = enemy.position.z - follower.position.z;
            float stopDistance = 1f;     // наскільки близько підходимо
            if (dist <= stopDistance)
            {
                BossScript bosscript = enemy.gameObject.GetComponent<BossScript>();
                bosscript.DecreaseHealthBar();
                PlayerSpawner.playerSpawnerInstance.DestroyAndDelete(follower.gameObject);
                continue;
            }

            // Швидкість залежно від відстані (можеш підігратись)
            float speed =
                dist > 12f ? 3.0f :
                dist >  8f ? 2.5f :
                dist >  4f ? 2.0f :
                1.8f;

            // Рухаємось до позиції ворога, але зберігаємо свою Y
            float targetPositionX = zDistance < 3f ? enemy.position.x : follower.position.x; 
            Vector3 target = new Vector3(targetPositionX, follower.position.y, enemy.position.z);
            follower.position = Vector3.MoveTowards(follower.position, target, speed * Time.deltaTime);
        }

        playerSpawner.PauseRegroup();
    }

    
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            GateControl gateControl = other.GetComponent<GateControl>();
            gateControl.Activate(playerSpawner);
            playerSpawner.StickmansSetAnimRun();
        }
        else if (other.CompareTag("EnemyZone"))
        {
            enemy = other.transform;
            attack = true;

            other.transform.GetChild(1).GetComponent<EnemyManager>().AttackThem(transform);
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
            PlayerSpawner.playerSpawnerInstance.textCounter.gameObject.transform.parent.gameObject.SetActive(false);
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
            CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(4);
            gamestate = false;
            playerSpawner.StickmansSetAnimDance();
            StartCoroutine(DelayOpenWinScreen());

            IEnumerator DelayOpenWinScreen()
            {
                yield return new WaitForSeconds(3f);
                PlayerWin();
            }
        }
        else if (other.CompareTag("BossFinishLine"))
        {
            // Align all the stickmans to center
            inFinishZone = true;
            Debug.Log("boss finish road entered");
        }
        else if (other.CompareTag("BossFightZone"))
        {
            transform.GetChild(1).gameObject.SetActive(false); // Disable stickmans counter;    
            enemy = other.transform.GetChild(0);
            playerSpawner.PauseRegroup();
            playerSpawner.EngageEnemy(enemy);
            
            // attackBoss = true;
            allowMovement = false;
            enemy.GetComponent<BossScript>().setAnimationPunch();
        }
        
    }
}