using UnityEngine;

public class EnemyArrow : MonoBehaviour
{

    [SerializeField] private Transform target;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float turnSpeedDeg = 720f; // як швидко «крутиться» у напрямку цілі
    [SerializeField] private float stickDistance = 0.2f; // коли вважати, що влучили
    [SerializeField] private bool alignForward = true;   // чи вирівнювати «нос» стріли

    public bool active = false;
    void Update()
    {
        if (!active) return;
        if (target == null) { Destroy(gameObject); return; }

        // Обчислюємо бажаний напрямок
        Vector3 toTarget = (target.position - transform.position);
        float dist = toTarget.magnitude;

        if (dist <= stickDistance)
        {
            // «прилипнути» до цілі
            transform.position = target.position;
            Destroy(gameObject);
            PlayerSpawner.playerSpawnerInstance.DestroyAndDelete(target.gameObject);
            return;
        }

        Vector3 dir = toTarget.normalized;

        // Плавно повертаємо «ніс» стріли у бік цілі
        if (alignForward)
        {
            Quaternion desiredRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, desiredRot, turnSpeedDeg * Time.deltaTime
            );
        }

        // Рухаємось в напрямку цілі (кінематично)
        transform.position += dir * speed * Time.deltaTime;
    }

    public void ActivateArrow(Transform target)
    {
        this.target = target;
        active = true;
    }
}
