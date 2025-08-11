using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class stickManManager : MonoBehaviour
{
    [SerializeField]
    ParticleSystem fightParticles;
    [SerializeField]
    ParticleSystem bloodParticles;

    [SerializeField]
    float jumpHeight = 4f, jumpDuration = 2f;

    public static int areJumpingCount = 0, areFallingCount = 0;
    
    PlayerSpawner playerSpawner;
    bool wasAttacked = false;



    void Start()
    {
        playerSpawner = transform.parent.parent.GetComponent<PlayerSpawner>();
        if (PlayerControl.gamestate)
        {
            transform.GetChild(0).GetComponent<Animator>().SetTrigger("Running");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "red":
                EnemyCharacter enemyCharacter = other.GetComponent<EnemyCharacter>();
                if (!enemyCharacter.collidedWithEnemy && !wasAttacked)
                {
                    wasAttacked = true;
                    enemyCharacter.collidedWithEnemy = true;
                    playerSpawner.DestroyAndDelete(this.gameObject);
                    Destroy(other.gameObject);
                    // playerSpawner.ScheduleRegroup();

                    Instantiate(fightParticles, transform.position, Quaternion.identity);
                }
                break;
            case "Ramp":
                playerSpawner.PauseRegroup();
                areJumpingCount += 1;
                // playerSpawner.ScheduleRegroup();
                Jump();
                if (areJumpingCount == 0) playerSpawner.FormatStickMan();
                break;
            case "Obstacle":
                playerSpawner.DestroyAndDelete(this.gameObject);
                Instantiate(bloodParticles, transform.position, Quaternion.identity);
                // playerSpawner.ScheduleRegroup();
                break;
            case "Gap":
                playerSpawner.PauseRegroup();
                areFallingCount += 1;
                Fall();
                if (areFallingCount == 0) playerSpawner.FormatStickMan();
                break;
            case "Fist":
                playerSpawner.DestroyAndDelete(this.gameObject);
                Instantiate(bloodParticles, transform.position, Quaternion.identity);
                // playerSpawner.ScheduleRegroup();
                break;
        }
    }

    void Jump()
    {
        Sequence jumpSequence = DOTween.Sequence();
        Animator animator =  transform.GetChild(0).GetComponent<Animator>();
        animator.SetTrigger("Jumping");   

        jumpSequence.Append(
            transform.DOMoveY(0 + jumpHeight, jumpDuration / 2f).SetEase(Ease.OutQuad)
        );
        jumpSequence.Append(
            transform.DOMoveY(0, jumpDuration / 2f).SetEase(Ease.InQuad).OnComplete(JumpEndedFunction)
        );

        void JumpEndedFunction()
        {
            areJumpingCount -= 1;
            animator.ResetTrigger("Jumping");    
            animator.SetTrigger("Running");
        }
        
    }

    void Fall()
    {
        Sequence fallSequence = DOTween.Sequence();

        fallSequence.Append(
            transform.DOMoveY(transform.position.y - jumpHeight, jumpDuration / 2f).SetEase(Ease.InQuad).OnComplete(ManagePlayerFalling)
        );

        void ManagePlayerFalling()
        {
            playerSpawner.DestroyAndDelete(this.gameObject);
            areFallingCount -= 1;
        }
    }
}
