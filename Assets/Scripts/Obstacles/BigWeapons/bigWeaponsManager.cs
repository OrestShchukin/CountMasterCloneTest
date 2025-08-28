using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bigWeaponsManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private List<Animator> animators = new ();
    
        void Awake()
    {
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
            float delay = Random.Range(0f, 2f);
            StartCoroutine(EnableAnimation(animator, delay));
        }
    }

    IEnumerator EnableAnimation(Animator animator, float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.enabled = true;
    }

   
    // Update is called once per frame
    void Update()
    {
        
    }
}
