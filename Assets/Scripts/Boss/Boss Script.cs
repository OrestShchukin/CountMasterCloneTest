using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Health Bar")]
    public float healthBar = 100;
    public Slider healthBarSlider;
    public int decreasePerSecond = 0;
    public Canvas canvas;
    
    public static BossScript bossScriptInstance;
    
    
    [Header("Camera Settings")]
    public CinemachineCamera bossCinemachineCamera;

    private float healthBarMax;

    private void Awake()
    {
        healthBarMax = healthBar;
        bossScriptInstance = this;
        bossCinemachineCamera.gameObject.SetActive(false);
        // canvas = UIManager.UIManagerInstance.BossRangeModifierUI.transform.GetChild(0).gameObject
        //     .GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = PlayerControl.playerControlInstance.mainCamera;
        
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
