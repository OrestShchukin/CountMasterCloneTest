using UnityEngine;

public class VerticalSpikesLogRotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 360f; // degrees per second
    [SerializeField] bool inverted = false;

    private int modifier = 1;
    void Awake()
    {
        if (inverted) modifier = -1;
    }
    void Update()
    {
        transform.Rotate(0f, modifier * rotationSpeed * Time.deltaTime, 0f);
    }

}
