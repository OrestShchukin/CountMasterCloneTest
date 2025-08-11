using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerFollowMine : MonoBehaviour
{
    [SerializeField]
    float forwardSpeed = 5f, xMovementSpeed = 10f, xLimit = 5f;
    [SerializeField]
    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
            MoveToTouch(Input.mousePosition);

#else
        if (Input.touchCount > 0)
            MoveToTouch(Input.GetTouch(0).position);

#endif
    }
    void MoveToTouch(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = transform.position;
            targetPos.x = Mathf.Clamp(hit.point.x, -xLimit, xLimit);
            transform.position = Vector3.Lerp(transform.position, targetPos, xMovementSpeed * Time.deltaTime);
        }

    }
}
