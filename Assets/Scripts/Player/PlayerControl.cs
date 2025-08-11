using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float forwardSpeed = 6f, attackSpeed = 5f, swipeSensitivity = 0.01f, maxX = 5f;

    [SerializeField] PlayerSpawner playerSpawner;
    [SerializeField] Transform followerParent;
    [SerializeField] FormationManager formation; // assign in Inspector

    public static PlayerControl playerControlInstance;
    public static bool gamestate;
    public static bool inFinishZone;
    public static float progress = 0f;
    public static float characterSizeScaleModifier;

    public static float userSensitivity; // set elsewhere
    
    float currentX = 0f;
    Vector2 lastTouchPosition; bool isTouching = false;
    public static bool allowMovement = true;

    bool attack; bool attackBoss; Transform enemy;

    void Awake(){ playerControlInstance = this; inFinishZone = false; allowMovement = true; }

    void Start(){ characterSizeScaleModifier = playerSpawner.followerPrefab.transform.localScale.x; Application.targetFrameRate = 60; }

    void Update()
    {
        if (!gamestate) return;
        if (followerParent.childCount != 0 || inFinishZone)
        {
            if (allowMovement) Move();
            progress = transform.position.z / (LevelGenerator.pathDistance + 12f);
            UIManager.UIManagerInstance.progressBar.fillAmount = progress;
        }
        else
        {
            attack = false; gamestate = false; transform.GetChild(1).gameObject.SetActive(false);
            UIManager.UIManagerInstance.OpenLoseScreen();
        }
    }

    void Move(Vector3 direction = default, float overrideSpeed = 0)
    {
        direction = direction == default ? Vector3.forward : direction.normalized;
        if (inFinishZone) direction.x = 0;
        float speed = overrideSpeed == 0 ? forwardSpeed : overrideSpeed;
        transform.Translate(direction * (speed * Time.deltaTime));
        if (inFinishZone) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) { lastTouchPosition = Input.mousePosition; isTouching = true; }
        else if (Input.GetMouseButton(0) && isTouching)
        {   Vector2 delta = (Vector2)Input.mousePosition - lastTouchPosition; lastTouchPosition = Input.mousePosition;
            currentX += delta.x * swipeSensitivity * userSensitivity; CalculateMaxX(); }
        else if (Input.GetMouseButtonUp(0)) isTouching = false;
#else
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began){ lastTouchPosition = touch.position; isTouching = true; }
            else if (touch.phase == TouchPhase.Moved && isTouching){ Vector2 delta = touch.position - lastTouchPosition; lastTouchPosition = touch.position; currentX += delta.x * swipeSensitivity * userSensitivity; CalculateMaxX(); }
            else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){ isTouching = false; }
        }
#endif
        void CalculateMaxX(){
            if (playerSpawner.followers.Count < 50 / characterSizeScaleModifier) currentX = Mathf.Clamp(currentX, -maxX, maxX);
            else { float clampSizeModifier = 0.6f / characterSizeScaleModifier; currentX = Mathf.Clamp(currentX, -maxX * clampSizeModifier, maxX * clampSizeModifier); }
        }
        Vector3 p = transform.position; p.x = currentX; transform.position = p;
    }

    public void PlayerWin(){ UIManager.UIManagerInstance.OpenWinScreen(); }

    public void ResetFormationToLeader(){ formation?.SetCenterToLeader(); formation?.RefreshNow(); playerSpawner.StickmansSetAnimRun(); }

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
            enemy = other.transform; attack = false; formation?.SetCenterToEnemy(other.transform);
            other.transform.GetChild(1).GetComponent<EnemyManager>().AttackThem(transform);
        }
        else if (other.CompareTag("HorizontalObstacleButton")) other.GetComponent<TrapButton>().ActivateButton();
        else if (other.CompareTag("FinishStairs"))
        {
            playerSpawner.StickmansBuildPyramid(); inFinishZone = true; PlayerSpawner.playerSpawnerInstance.textCounter.gameObject.transform.parent.gameObject.SetActive(false);
        }
        else if (other.CompareTag("StairsFirstStep")) playerSpawner.StickmansBuildStairs();
        else if (other.CompareTag("FinishChest"))
        {
            CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(4); gamestate = false; playerSpawner.StickmansSetAnimDance();
            StartCoroutine(DelayOpenWinScreen()); IEnumerator DelayOpenWinScreen(){ yield return new WaitForSeconds(3f); PlayerWin(); }
        }
        else if (other.CompareTag("BossFightZone"))
        {
            enemy = other.transform.GetChild(0); attackBoss = false; formation?.SetCenterToEnemy(enemy);
            enemy.GetComponent<BossScript>().setAnimationPunch();
        }
    }
}
