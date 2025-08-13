using System;
using UnityEngine;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float healthBar = 100;
    public Slider healthBarSlider;
    public int decreasePerSecond = 0;
    public static BossScript bossScriptInstance;

    private float healthBarMax;

    private void Awake()
    {
        healthBarMax = healthBar;
        bossScriptInstance = this;
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

    void Update()
    {
        if (decreasePerSecond > 0)
        {
            healthBar -= decreasePerSecond * Time.deltaTime;
            healthBarSlider.value = healthBar / healthBarMax;
        }
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
