using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class EnemyDefenceTowerManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro counterTxt;
    [SerializeField] private Transform ZoneOutline;
    [SerializeField] private Transform archersParent;
    public Transform tower;
    public int towerHealth = 1;

    [Header("Effects")] 
    public GameObject attackSmoke;
    public GameObject destroySmoke;

    [Header("Other")]
    public Transform enemy;
    public bool attack;

    private int initialTowerHealth;
    private float towerHeight;
    private float moveRange;
    void Awake()
    {
        int unitsMultiplier = LevelGenerator.segmentIndex > 20 ? LevelGenerator.segmentIndex / 2 : 10;
        int minUnits = unitsMultiplier * 2;
        int maxUnits = unitsMultiplier * 3;
        towerHealth = UnityEngine.Random.Range(minUnits, maxUnits);
        initialTowerHealth = towerHealth;
        counterTxt.text = towerHealth.ToString();
        
        towerHeight = tower.position.y * 2;
        moveRange = towerHeight / towerHealth;
    }

    void Update()
    {
        if (!attack) return;
        if (towerHealth <= 0)
        {
            PlayerControl.playerControlInstance.StopAttack();
            DestroyTower();
        }
    
        if (enemy.GetChild(0).childCount == 0)
        {
            for (int i = 0; i < archersParent.childCount; i++)
            {
                archersParent.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("Run", false);
            }

            attack = false;
        }
    }


    public bool AttackTower()
    {
        if (towerHealth <= 0) return false;
        towerHealth--;
        counterTxt.text = towerHealth.ToString();
        tower.DOLocalMoveY(-moveRange, 0.1f).SetEase(Ease.Linear);
        Destroy(Instantiate(attackSmoke, transform.position, Quaternion.identity),2f);
        return true;
    }

    private void OnDestroy()
    {
        Destroy(Instantiate(destroySmoke, transform.position, Quaternion.identity), 2f);
    }

    private void DestroyTower()
    {
        StartCoroutine(TowerLose());
    }

    IEnumerator TowerLose()
    {
        foreach (Transform child in archersParent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        
        Destroy(tower.gameObject);
        yield return null;
    }

    public void AttackThem(Transform enemyForce)
    {
        enemy = enemyForce;
        attack = true;

        for (int i = 0; i < archersParent.childCount; i++)
        {
            archersParent.GetChild(i).GetChild(0).GetComponent<Animator>().SetBool("Run", true);
        }

        ZoneOutline.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 6f, 1).SetLoops(-1);
    }
}