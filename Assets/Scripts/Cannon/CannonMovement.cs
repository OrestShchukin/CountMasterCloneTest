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

    public bool active;
    
    float cy, cp;
    private bool stopAiming = false;
    

    void Reset()
    {
        if (!yawPivot)   yawPivot   = transform;
        if (!pitchPivot) pitchPivot = transform;
    }

    void OnEnable()
    {
        cy = Normalize(yawPivot.localEulerAngles.y);
        cp = Normalize(pitchPivot.localEulerAngles.x);
    }

    void Update()
    {
        if (stopAiming || !active) return;
        
        Vector2 pos;
        bool pressed;
        GetPointer(out pos, out pressed);
        if (requireHold && !pressed) return;
        
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

    public void StopAiming()
    {
        stopAiming = true;
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
}
