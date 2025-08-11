using UnityEngine;
using System.Collections.Generic;

public class MountainSpawner : MonoBehaviour
{
    public GameObject mountainPrefab;
    
    public int mountainsPerSide = 7; // Per side per plain

    public float plainWidth = 10f;
    public float mountainSize = 4f;

    void Start()
    {
        SpawnMountainsAround(transform.position);
    }

    void SpawnMountainsAround(Vector3 plainPos)
    {
        // Mountain X offset range
        float minXOffset = 7f;
        float maxXOffset = 23f;

        // Z range around plain (same length)
        float minZ = -plainWidth / 2f;
        float maxZ = plainWidth / 2f;

        List<Vector3> usedPositions = new List<Vector3>();

        for (int side = -1; side <= 1; side += 2) // -1 for left, +1 for right
        {
            for (int i = 0; i < mountainsPerSide; i++)
            {
                // Random X offset from plain center
                float xOffset = Random.Range(minXOffset, maxXOffset) * side;
                float zOffset = Random.Range(minZ, maxZ);
                float yOffset = Random.Range(-6f, -8f);

                Vector3 spawnPos = plainPos + new Vector3(xOffset, yOffset, zOffset);

                // Check if too close to existing mountains
                bool tooClose = false;
                foreach (var pos in usedPositions)
                {
                    if (Vector3.Distance(spawnPos, pos) < mountainSize * 0.9f)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    GameObject mountain = Instantiate(mountainPrefab, spawnPos, randomRotation, transform);
                    float scaleModifierXZ = Random.Range(0.8f, 1.6f);
                    float scaleModifierY = Random.Range(0.8f, 2f);
                    mountain.transform.localScale = new Vector3(scaleModifierXZ, scaleModifierY, scaleModifierXZ);
                    usedPositions.Add(spawnPos);
                }
            }
        }
    }
}
