using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] GameObject enemyArrow;
    [SerializeField] float force = 30f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool collidedWithEnemy = false;
    
    

    

    
    public void Shoot( Transform target )
    {
        if (enemyArrow == null)
        {
            Debug.LogError($"{name}: enemyArrow is NULL on EnemyCharacter.");
            return;
        }
        if (target == null)
        {
            Debug.LogWarning($"{name}: Shoot called with NULL target.");
            return;
        }
        
        GameObject arrow = Instantiate(enemyArrow, transform.position, transform.rotation);
        arrow.GetComponent<EnemyArrow>().ActivateArrow(target);
        Destroy(arrow, 4f);
    }
}
