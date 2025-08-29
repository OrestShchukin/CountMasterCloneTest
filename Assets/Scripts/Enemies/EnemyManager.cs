  

using System;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;
using Unity.Mathematics;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro counterTxt;
    [SerializeField] private GameObject stickMan;
    [SerializeField] private Transform ZoneOutline;

    public Transform enemy;
    public bool attack;
    private int _childCount;

    void Start()
    {
        int unitsMultiplier = LevelGenerator.segmentIndex > 20 ? LevelGenerator.segmentIndex / 2 : 10;
        int minUnits = unitsMultiplier * 2;
        int maxUnits = unitsMultiplier * 3;
        for (int i = 0; i < UnityEngine.Random.Range(minUnits, maxUnits); i++)
        {
            Instantiate(stickMan, transform.position, new Quaternion(0f, 180f, 0f, 1f), transform);
        }

        counterTxt.text = transform.childCount.ToString();

        FormatStickMan();
        _childCount = transform.childCount;
        
        
    }

    void Update()
    {
        if (attack && transform.childCount > 0)
        {
            var enemyPos = new Vector3(enemy.position.x, transform.position.y, enemy.position.z);
            var enemyDirection = enemy.position - transform.position;

            Transform attacker;

            for (int i = 0; i < transform.childCount; i++)
            {
                attacker = transform.GetChild(i);
                attacker.rotation = Quaternion.Slerp(attacker.rotation, 
                    quaternion.LookRotation(enemyDirection, Vector3.up),
                    Time.deltaTime * 3f); 

                var distance = enemy.GetChild(1).position - attacker.position;

                if (distance.magnitude < 10f)
                {
                    attacker.position =
                        Vector3.Lerp(attacker.position, enemy.GetChild(0).position, Time.deltaTime * 1f);
                }
            }

            counterTxt.text = transform.childCount.ToString();
            if (enemy.GetChild(0).childCount == 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("Run", false);
                }

                attack = false;
            }
        }
    }



    void FormatStickMan()
    {
        int count = transform.childCount;

        var positions = PlayerSpawner.GenerateRingBlueNoise(count, 
            PlayerSpawner.playerSpawnerInstance.minDistance,
            PlayerSpawner.playerSpawnerInstance.radiusStep + 0.2f, 0f);
        
        for (int i = 0; i < count; i++)
        {

            transform.GetChild(i).localPosition = positions[i];
        }
    }
    

    public void AttackThem(Transform enemyForce)
    {
        enemy = enemyForce;
        attack = true;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("Run", true);
        }
        
        ZoneOutline.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 6f, 1).SetLoops(-1);
    }
}