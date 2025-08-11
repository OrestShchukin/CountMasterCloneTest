using UnityEngine;

public class HandIconAnimation : MonoBehaviour
{
    public float moveDistance = 50f; // Distance in each direction (X)
    public float speed = 100f;       // Speed of movement

    private RectTransform rectTransform;
    private float targetX;
    private int direction = 1;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        targetX = moveDistance;
    }

    void Update()
    {
        // Move in the current direction
        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            new Vector2(targetX, rectTransform.anchoredPosition.y),
            speed * Time.deltaTime
        );

        // Switch direction if target reached
        if (Mathf.Approximately(rectTransform.anchoredPosition.x, targetX))
        {
            direction *= -1;
            targetX = direction * moveDistance;
        }
    }
}
