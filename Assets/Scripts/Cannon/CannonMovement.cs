using DG.Tweening;
using UnityEngine;

public class CannonMovement : MonoBehaviour
{
    [Header("Pivots")]
    [SerializeField] Transform yawPivot;    // крутиться навколо Y (вліво/вправо)
    [SerializeField] Transform pitchPivot;  // крутиться навколо X (вгору/вниз)

    [Header("Aim zone (Screen Space - Overlay)")]
    [SerializeField] RectTransform aimArea; // null => весь екран

    [Header("Angles (deg)")] 
    [SerializeField] float yawMin = -60f;
    [SerializeField] float yawMax = 60f;
    [SerializeField] float pitchMin = -10f, pitchMax = 35f;

    [Header("Controls")]
    [SerializeField] bool requireHold = true;             // наводити тільки коли затиснуто
    [SerializeField] bool invertY = false;
    [SerializeField, Range(0f, 0.5f)] float dead = 0.05f; // мертва зона біля центру
    [SerializeField] float smooth = 12f;                  // плавність

    [Header("Aim visual")] 
    [SerializeField] LayerMask targetMask;    // Castle/Enemies/Environment
    [SerializeField] LayerMask catchMask;      // по яких шарах “цілимось”
    [SerializeField] RectTransform crosshair;
    [SerializeField] RectTransform canvasRectTransform;
    // UI-іконка прицілу всередині aimArea

    private Camera cam; // основна камера (для WorldToScreenPoint)
    private Transform muzzle; // кінець ствола (звідси кидаємо промінь)

    private Vector2 crosshairVel;
    
    
    private CannonShooting cannonShootingScript;
    
    public bool active;
    private bool firstPress = true;
    
    float cy, cp;
    private bool stopAiming = false;

    void Awake()
    {
        cannonShootingScript = GetComponent<CannonShooting>();
        muzzle = cannonShootingScript.barrel;
        cam = Camera.main;
    }
    
    void Reset()
    {
        if (!yawPivot)   yawPivot   = transform;
        if (!pitchPivot) pitchPivot = transform;
    }

    void OnEnable()
    {
        cy = Normalize(yawPivot.localEulerAngles.y);
        cp = Normalize(pitchPivot.localEulerAngles.x);
        StartAiming();
    }

    void Update()
    {
        if (stopAiming || !active) return;
        
        Vector2 pos;
        bool pressed;
        GetPointer(out pos, out pressed);
        if (requireHold && !pressed) return;
        if (firstPress)
        {
            GetComponent<CannonShooting>().StartShooting();
            firstPress = false;
        }
        
        
        Rect r = GetScreenRect();
        if (!r.Contains(pos)) return;

        // 3) нормалізуємо в [-1..1] відносно центру зони
        float nx = (Mathf.InverseLerp(r.xMin, r.xMax, pos.x) * 2f) - 1f;
        float ny = (Mathf.InverseLerp(r.yMin, r.yMax, pos.y) * 2f) - 1f;

        if (Mathf.Abs(nx) < dead) nx = 0f;
        if (Mathf.Abs(ny) < dead) ny = 0f;
        if (invertY) ny = -ny;

        // 4) мапимо у кути з клемпом через Lerp
        float ty = Mathf.Lerp(yawMin,   yawMax,   (nx + 1f) * 0.5f); // X -> yaw(Y)
        float tp = Mathf.Lerp(pitchMin, pitchMax, (ny + 1f) * 0.5f); // Y -> pitch(X)

        // 5) плавно підводимо
        cy = Mathf.LerpAngle(cy, ty, Time.deltaTime * smooth);
        cp = Mathf.LerpAngle(cp, tp, Time.deltaTime * smooth);

        // 6) застосовуємо
        if (yawPivot)   yawPivot.localRotation   = Quaternion.Euler(0f, 0f, cy);
        if (pitchPivot) pitchPivot.localRotation = Quaternion.Euler(0f, cp, 0f);
        
        UpdateAimReticle();
    }

    void GetPointer(out Vector2 pos, out bool pressed)
    {
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            pos = t.position;
            pressed = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
            return;
        }
        pos = Input.mousePosition;
        pressed = Input.GetMouseButton(0);
    }

    public void AimAtCoin()
    {
        StopAiming();
        Sequence sequence = DOTween.Sequence();
        if (yawPivot)   sequence.Append(yawPivot.DOLocalRotate(new Vector3(0f, 0f, 0f), 1f));
        if (pitchPivot) sequence.Append(pitchPivot.DOLocalRotate(new Vector3(0f, -55f, 0f), 1f));
        sequence.Play();
    }

    public void StartAiming()
    {
        stopAiming = false;
        if (crosshair) crosshair.gameObject.SetActive(true);
        
    }
    public void StopAiming()
    {
        stopAiming = true;
        if (crosshair) crosshair.gameObject.SetActive(false);
    }

    Rect GetScreenRect()
    {
        // if (!aimArea) return new Rect(0, 0, Screen.width, Screen.height);

        // Для Overlay достатньо world-corners без камери
        var c = new Vector3[4];
        aimArea.GetWorldCorners(c);
        Vector2 min = c[0], max = c[0];
        for (int i = 1; i < 4; i++) { min = Vector2.Min(min, c[i]); max = Vector2.Max(max, c[i]); }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    static float Normalize(float a) { a %= 360f; if (a > 180f) a -= 360f; return a; }
    
    
    void UpdateAimReticle()
    {
        if (!cam || !muzzle || !crosshair || !aimArea) return;

        Vector3 aimPoint;
        Ray ray = new Ray(muzzle.position, muzzle.forward);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        // 1) Спочатку — реальні цілі
        if (Physics.Raycast(ray, out var hit, 5000f, targetMask))
        {
            aimPoint = hit.point;
        }
        // 2) Якщо промах — ловимося на невидимій площині
        else if (Physics.Raycast(ray, out hit, 5000f, catchMask))
        {
            aimPoint = hit.point;
        }
        // 3) На крайній випадок — далека точка по променю
        else
        {
            aimPoint = ray.GetPoint(200f);
        }


        Vector3 sp = cam.WorldToScreenPoint(aimPoint);
        if (sp.z < 0f) { crosshair.gameObject.SetActive(false); return; }
        crosshair.gameObject.SetActive(true);

        Rect r = GetScreenRect();
        sp.x = Mathf.Clamp(sp.x, r.xMin, r.xMax);
        // sp.y = Mathf.Clamp(sp.y, r.yMin, r.yMax);

        
        
        // Screen → local
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, sp, null, out var local);
        // local.y -= 700f;
        // Додаткове згладжування, щоб прибрати дрібний “діринг”
        crosshair.anchoredPosition = Vector2.SmoothDamp(
            crosshair.anchoredPosition, local, ref crosshairVel, 0.05f
        );
    }

}
