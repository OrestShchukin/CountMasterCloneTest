using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class EnemyShootingManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro counterTxt;
    [SerializeField] private GameObject stickMan;
    [SerializeField] private Transform ZoneOutline;

    public Transform enemy;
    public bool attack;
    
    [Header("Shooting")]
    [SerializeField] float interval = 1f;
    
    private int _childCount;
    public List<GameObject> followersToShoot = new ();
    public List<EnemyCharacter> redList = new ();
    

    
    
    void Start()
    {
        int unitsMultiplier = LevelGenerator.segmentIndex > 16 ? LevelGenerator.segmentIndex / 2 : 8;
        int minUnits = unitsMultiplier * 2;
        int maxUnits = unitsMultiplier * 3;
        for (int i = 0; i < UnityEngine.Random.Range(minUnits, maxUnits); i++)
        {
            GameObject red = Instantiate(stickMan, transform.position, new Quaternion(0f, 180f, 0f, 1f), transform);
            redList.Add(red.GetComponent<EnemyCharacter>());
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
                
                if (distance.z < 5f)
                {
                    attacker.position =
                        Vector3.Lerp(attacker.position, transform.parent.position, Time.deltaTime * 0.3f);
                }
            }

            counterTxt.text = transform.childCount.ToString();
            if (enemy.GetChild(0).childCount == 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("RiffleWalk", false);
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

        FillTheList();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("RiffleWalk", true);
        }
        StartAutoShooting();
        
        ZoneOutline.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 6f, 1).SetLoops(-1);
    }

    private void FillTheList()
    {
        foreach (Transform follower in PlayerSpawner.playerSpawnerInstance.followerParent)
        {
            followersToShoot.Add(follower.gameObject);
        }

        // followersToShoot = PlayerSpawner.playerSpawnerInstance.followers;
    }


    private void ShootEnemies()
    {
        followersToShoot.RemoveAll(item => item == null);
        redList.RemoveAll(item => item == null);
        int amountToShoot = Mathf.Min(followersToShoot.Count, redList.Count);
        if (amountToShoot == 0)
        {
            CancelInvoke(nameof(ShootEnemies));
            return;
        }
        for (int i = amountToShoot - 1; i >= 0; i--)
        {
            redList[i].Shoot(followersToShoot[i].transform);
            followersToShoot.Remove(followersToShoot[i]);
        }
    }

    private void StartAutoShooting()
    {
        CancelInvoke(nameof(ShootEnemies));
        InvokeRepeating(nameof(ShootEnemies), 0f, interval);
    }

    public void StopAutoShooting()
    {
        CancelInvoke(nameof(ShootEnemies));
        attack = true;
    }
}
