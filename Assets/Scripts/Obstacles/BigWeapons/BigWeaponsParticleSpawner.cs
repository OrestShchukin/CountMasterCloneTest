using UnityEngine;

public class BigWeaponsParticleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject particlePrefab;

    [SerializeField] private Transform target;

    [SerializeField] private float offsetX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SpawnParticlesOnCollision()
    {
        Vector3 spawnPos = new Vector3(target.position.x + offsetX, 0f, target.position.z);
        Destroy(Instantiate(particlePrefab, spawnPos, Quaternion.identity),2f);
    }

    // Update is called once per frame
}
