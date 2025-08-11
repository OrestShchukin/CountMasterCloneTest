using System;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int healthBar = 15;

    void Start()
    {
        // setAnimationPunch();
    }
    public void UpdateHealthBar()
    {
        healthBar--;
        if (healthBar <= 0)
        {
            this.gameObject.SetActive(false);
            UIManager.UIManagerInstance.OpenWinScreen();
            
        }
    }

    public void setAnimationPunch()
    {
        transform.GetChild(0).GetComponent<Animator>().SetBool("Punching", true);
        Debug.Log("Bool punching set to true");
    }
}
