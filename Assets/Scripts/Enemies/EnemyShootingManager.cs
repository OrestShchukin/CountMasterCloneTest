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
        int unitsMultiplier = LevelGenerator.segmentIndex > 20 ? LevelGenerator.segmentIndex / 2 : 10;
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

        FillTheList();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("Run", true);
        }
        StartAutoShooting();
        
        ZoneOutline.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 6f, 1).SetLoops(-1);
    }

    private void FillTheList()
    {
        // foreach (Transform follower in enemy)
        // {
        //     followersToShoot.Add(follower.gameObject);
        // }

        followersToShoot = PlayerSpawner.playerSpawnerInstance.followers;
    }


    private void ShootEnemies()
    {
        followersToShoot.RemoveAll(item => item == null);
        int amountToShoot = Mathf.Min(followersToShoot.Count, redList.Count);
        if (amountToShoot == 0)
        {
            CancelInvoke(nameof(ShootEnemies));
            return;
        }
        for (int i = amountToShoot - 1; i >= 0; i--)
        {
            Debug.Log($"RedList Count:  {redList.Count}, amountToShoot: {amountToShoot}, followersToShoot.Count: {followersToShoot.Count}, redList.Count: {redList.Count}");
            redList[i].Shoot(followersToShoot[i].transform);
            Debug.Log($"redList[{i}] OK: {redList[i]}");
            followersToShoot.Remove(followersToShoot[i]);
        }
    }

    private void StartAutoShooting()
    {
        CancelInvoke(nameof(ShootEnemies));
        InvokeRepeating(nameof(ShootEnemies), 0f, interval);
    }
}
