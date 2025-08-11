using System;
using UnityEngine;

public class PlayerSpawnerDebug : MonoBehaviour
{
    [SerializeField, Range(0, 500)]
    int numberOfStickmans = 10;
    
    [SerializeField, Range(0.01f, 10f)]
    float minDistance = 0.3f, radiusStep = 0.3f;
    
    
    PlayerSpawner spawner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        Application.targetFrameRate = Int32.MaxValue;
    }

    void Start()
    {
        spawner = GetComponent<PlayerSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        spawner.minDistance = minDistance;
        spawner.radiusStep = radiusStep;
        
        spawner.ChangeNumOfStickMansDebug(numberOfStickmans);
        spawner.FormatStickMan();
    }
}
