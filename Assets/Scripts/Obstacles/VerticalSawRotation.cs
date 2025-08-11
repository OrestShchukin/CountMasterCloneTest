using UnityEngine;

public class Sawrotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 360f; // degrees per second

    [SerializeField] float speed = 2f, timeOffset = 0f;

    [SerializeField] bool horizontalMovementEnabled = true;

    public float moveDistance = 3.5f;    // Distance from center (total is 7)

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Rotate around Z axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        if (horizontalMovementEnabled)
        {
            float newX = Mathf.PingPong((Time.time + timeOffset) * speed, moveDistance * 2) - moveDistance;
            transform.position = new Vector3(startPos.x + newX, startPos.y, startPos.z);
        }
    }
}
