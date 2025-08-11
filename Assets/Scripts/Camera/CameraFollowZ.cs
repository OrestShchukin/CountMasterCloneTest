using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{

    [SerializeField]
    Transform player;

    float fixedY, fixedZ;

    [SerializeField]
    float xDriftAmount = 2f, driftSmoothing = 3f;


    float currentXDrift = 0f;

    void Awake()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }
    void LateUpdate()
    {
        if (player == null) return;

        // Target drift based on player's X position (normalized)
        float normalizedX = Mathf.Clamp(player.position.x / 5f, -1f, 1f); // assumin max X range is -5 to +5
        float targetDrift = normalizedX * xDriftAmount;
        // Smoothly Interpolate to drift
        currentXDrift = Mathf.Lerp(currentXDrift, targetDrift, Time.deltaTime * driftSmoothing);

        // Set Camera Position
        transform.position = new Vector3(currentXDrift, player.position.y + fixedY, player.position.z + fixedZ);
    }
    
}
