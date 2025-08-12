using System;
using UnityEngine;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int healthBar = 15;
    public Slider healthBarSlider;

    private int healthBarMax;

    private void Awake()
    {
        healthBarMax = healthBar;
    }

    public void DecreaseHealthBar()
    {
        healthBar--;
        healthBarSlider.value = healthBar / healthBarMax;
        if (healthBar <= 0)
        {
            healthBarSlider.value = 0f;
            this.gameObject.SetActive(false);
            UIManager.UIManagerInstance.OpenWinScreen();
        }
    }

    public void setAnimationPunch()
    {
        transform.GetChild(0).GetComponent<Animator>().SetBool("Punching", true);
    }
    public void unsetAnimationPunch()
    {
        transform.GetChild(0).GetComponent<Animator>().SetBool("Punching", false);
    }
}
