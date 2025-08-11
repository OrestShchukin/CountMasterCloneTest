using UnityEngine;

public class VerticalSpikesLogRotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 360f; // degrees per second
    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

}
