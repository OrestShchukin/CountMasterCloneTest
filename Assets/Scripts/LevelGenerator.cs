using System.Collections.Generic;
using UnityEngine;
public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs")] public List<GameObject> basicObstaclePrefabs;
    public List<GameObject> doubleSizeObstaclePrefabs;
    public List<GameObject> gatePrefabs;
    public GameObject enemySpawnerPrefab;
    public List<GameObject> finishLinePrefabsList;
    public GameObject planePrefab;
        
    [Header("Generation Settings")] public int numberOfSegments = 10;
    
    [SerializeField] Transform levelParent;
    [SerializeField] Transform player;

    [Header("Other essentials")] public Transform followersParent;
    
    float segmentLength = 10f;
    float currentZ = 20f; 
    int currentObstaclesInRow = 3;  
    int gatesPassedSinceFight = 0;
    
    public static int segmentIndex;
    public static float pathDistance;

    
    void Awake()
    {
        segmentIndex = 0;
        StartLevelGeneration();
        pathDistance = numberOfSegments * segmentLength;
    }

    void FixedUpdate()
    {
        if (PlayerControl.inFinishZone) return;
        if (levelParent.childCount == 0) return;
        
        Transform firstChild = levelParent.GetChild(0);
        
        if (player.position.z - segmentLength * 2 > firstChild.position.z)
        {
            if (segmentIndex < numberOfSegments)
            {
                Destroy(firstChild.gameObject);
                SpawnElement();
            }
        }
    }

    void StartLevelGeneration()
    {
        for (segmentIndex = 0; segmentIndex < 10;)
        {
            SpawnElement();
        }
    }


    void SpawnElement()
    {
        Vector3 spawnPos = new Vector3(0, 0, currentZ);

        if (currentObstaclesInRow >= 4)
        {
            SpawnGate(spawnPos);
        }
        else
        {
            if (Random.value > 0.7f && segmentIndex > 4 && gatesPassedSinceFight >= 2)
            {
                SpawnEnemy(spawnPos);
            }
            else
            {
                SpawnObstacle(spawnPos);
            }
        }

        segmentIndex++;
        currentZ += segmentLength;

        if (segmentIndex >= numberOfSegments)
        {
            spawnPos = new Vector3(0, 0, currentZ);
            SpawnFinishLine(spawnPos);
        }
    }


    void SpawnGate(Vector3 position)
    {
        currentObstaclesInRow = 0;
        int index = Random.Range(0, gatePrefabs.Count);
        GameObject gate = Instantiate(gatePrefabs[index], position, Quaternion.identity, levelParent);
        gatesPassedSinceFight += 1;
    }

    void SpawnObstacle(Vector3 position)
    {
        currentObstaclesInRow += 1;
        List<GameObject> prefabsList;
        bool doubleSize = false;

        int randomNum = Random.Range(0, 2);
        if (randomNum == 0)
        {
            prefabsList = basicObstaclePrefabs;
        }
        else
        {
            prefabsList = doubleSizeObstaclePrefabs;
            doubleSize = true;
        }

        int index = Random.Range(0, prefabsList.Count);
        GameObject obstacle = Instantiate(prefabsList[index], position, Quaternion.identity, levelParent);
        if (doubleSize)
        {
            currentZ += segmentLength;
            segmentIndex++;
        }
    }

    void SpawnEnemy(Vector3 position)
    {
        if (enemySpawnerPrefab)
        {
            Instantiate(enemySpawnerPrefab, position, Quaternion.identity, levelParent);
        }

        currentObstaclesInRow += 4;
        gatesPassedSinceFight = 0;
        currentZ += segmentLength;
    }

    void SpawnFinishLine(Vector3 position)
    {
        int index = Random.Range(0, finishLinePrefabsList.Count);
        GameObject obstacle = finishLinePrefabsList[index];
        if (obstacle)
        {
            Instantiate(finishLinePrefabsList[index], position, Quaternion.identity, levelParent);
        }

        currentZ += segmentLength;
    }
}