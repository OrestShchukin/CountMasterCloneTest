using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [Serializable]
    public struct PlatformDef
    {
        public GameObject prefab;
        public float length;
    }
    
    
    // [Header("Prefabs")] public List<GameObject> basicObstaclePrefabs;
    // public List<GameObject> doubleSizeObstaclePrefabs;
    // public List<GameObject> gatePrefabs;
    // public GameObject shootingRangePrefab;
    // public GameObject enemySpawnerPrefab;
    // public List<GameObject> finishLinePrefabsList;
        
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

    [Header("New Prefabs Section")] 
    [SerializeField] private List<PlatformDef> obstaclesList = new();
    [SerializeField] private List<PlatformDef> gatesList = new();
    [SerializeField] private List<PlatformDef> finishLinePrefabsList = new();
    [SerializeField] private PlatformDef enemySpawnerDef;
    
    
    
    
    void Awake()
    {
        segmentIndex = 0;
        StartLevelGeneration();
        pathDistance = numberOfSegments * segmentLength;
    }
    //
    void FixedUpdate()
    {
        if (PlayerControl.inFinishZone || levelParent.childCount == 0) return;
        
        Transform firstChild = levelParent.GetChild(0);
        
        if (player.position.z - segmentLength * 5 > firstChild.position.z)
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
        for (segmentIndex = 0; segmentIndex < 15;)
        {
            SpawnElement();
        }
    }
    //
    //
    void SpawnElement()
    {
        Vector3 spawnPos = new Vector3(0, 0, currentZ);
    
        if (currentObstaclesInRow >= 4)
        {
            if (Random.value > 0.8f && segmentIndex > 10)
            {
                SpawnShootingRange(spawnPos);
            }
            else
            {
                SpawnGate(spawnPos);
            }
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
        
    
        if (segmentIndex >= numberOfSegments)
        {
            spawnPos = new Vector3(0, 0, currentZ);
            SpawnFinishLine(spawnPos);
        }
    }
    
    
    void SpawnGate(Vector3 position)
    {
        currentObstaclesInRow = 0;
        int index = Random.Range(0, gatesList.Count - 1);
        PlatformDef gateDef = gatesList[index];
        GameObject gate = Instantiate(gateDef.prefab, position, Quaternion.identity, levelParent);
        currentZ += gateDef.length;
        segmentIndex += (int)(gateDef.length / segmentLength);
        gatesPassedSinceFight += 1;
        
    }

    void SpawnObstacle(Vector3 position)
    {
    
        int index = Random.Range(0, obstaclesList.Count);
        PlatformDef obstacleDef = obstaclesList[index];
        GameObject obstacle = Instantiate(obstacleDef.prefab, position, Quaternion.identity, levelParent);
        currentZ += obstacleDef.length;
        segmentIndex += (int)(obstacleDef.length / segmentLength);
        currentObstaclesInRow += 1;
    }
    
    void SpawnEnemy(Vector3 position)
    {
        Instantiate(enemySpawnerDef.prefab, position, Quaternion.identity, levelParent);
        
    
        currentObstaclesInRow += 4;
        gatesPassedSinceFight = 0;
        currentZ += enemySpawnerDef.length;
        segmentIndex += (int)(enemySpawnerDef.length / segmentLength);
    }
    
    void SpawnShootingRange(Vector3 position)
    {
        PlatformDef shootingRangeDef = gatesList.Last(); 
        Instantiate(shootingRangeDef.prefab, position, Quaternion.identity, levelParent);
        currentZ += shootingRangeDef.length;
        segmentIndex =+ (int)(shootingRangeDef.length /  segmentLength);
        currentObstaclesInRow = 0;
    }
    
    void SpawnFinishLine(Vector3 position)
    {
        int index = Random.Range(0, finishLinePrefabsList.Count);
        PlatformDef finishPrefaDef = finishLinePrefabsList[index];
        
        Instantiate(finishPrefaDef.prefab, position, Quaternion.identity, levelParent);

        currentZ += finishPrefaDef.length;
        segmentIndex += (int)(finishPrefaDef.length / segmentLength);
    }
}