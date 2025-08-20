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


    [Header("Generation Settings")] public int numberOfSegments = 10;

    [SerializeField] Transform levelParent;
    [SerializeField] Transform player;

    [Header("Other essentials")] public Transform followersParent;

    public static int segmentIndex;
    public static float pathDistance;

    [Header("Prefabs Section")] 
    [SerializeField] private List<PlatformDef> obstaclesList = new();
    [SerializeField] private List<PlatformDef> gatesList = new();
    [SerializeField] private List<PlatformDef> finishLinePrefabsList = new();
    [SerializeField] private List<PlatformDef> enemyObstaclesList = new();

    float segmentLength = 10f;
    float currentZ = 20f;
    int currentObstaclesInRow = 3;
    int gatesPassedSinceFight = 0;


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
        SpawnElementFromDef(gateDef, position);
        gatesPassedSinceFight += 1;
    }

    void SpawnObstacle(Vector3 position)
    {
        SpawnElementFromList(obstaclesList, position);
        currentObstaclesInRow += 1;
    }

    void SpawnEnemy(Vector3 position)
    {
        SpawnElementFromList(enemyObstaclesList, position);
        currentObstaclesInRow += 4;
        gatesPassedSinceFight = 0;
    }

    void SpawnShootingRange(Vector3 position)
    {
        PlatformDef shootingRangeDef = gatesList.Last();
        SpawnElementFromDef(shootingRangeDef, position);
        currentObstaclesInRow = 0;
    }

    void SpawnFinishLine(Vector3 position)
    {
        SpawnElementFromList(finishLinePrefabsList, position);
    }


    void SpawnElementFromList(List<PlatformDef> elementsList, Vector3 position)
    {
        int index = Random.Range(0, elementsList.Count);
        PlatformDef elementDef = elementsList[index];
        Instantiate(elementDef.prefab, position, Quaternion.identity, levelParent);
        currentZ += elementDef.length;
        segmentIndex += (int)(elementDef.length / segmentLength);
    }

    void SpawnElementFromDef(PlatformDef elementDef, Vector3 position)
    {
        Instantiate(elementDef.prefab, position, Quaternion.identity, levelParent);
        currentZ += elementDef.length;
        segmentIndex += (int)(elementDef.length / segmentLength);
    }
}