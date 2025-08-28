using UnityEngine;

public class SingleSpikesArm : MonoBehaviour
{
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float speed = 50f;     // Швидкість руху (град/сек)
    [SerializeField] private float startAngle = 90; // Початковий кут
    [SerializeField] private float endAngle = 180;  // Кінцевий кут
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Вісь обертання
    [SerializeField] private bool inverse = false;  // Якщо true — рух почнеться з протилежного краю

    void Update()
    {
        float minAngle = Mathf.Min(startAngle, endAngle);
        float maxAngle = Mathf.Max(startAngle, endAngle);

        float range = maxAngle - minAngle;

        // Стандартний PingPong від 0 до range
        float t = Mathf.PingPong(Time.time * speed, range);

        // Якщо inverse — розвертаємо рух
        if (inverse)
            t = range - t;

        float angle = minAngle + t;

        pivotPoint.rotation = Quaternion.AngleAxis(angle, rotationAxis);
    }
}