using System;
using TMPro;
using UnityEngine;

public class WoodenBox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TextMeshPro healthCounter;
    public int health = 5;


    public void DecreaseHealth()
    {
        health--;
        healthCounter.text = health.ToString();
        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateHealthCounter()
    {
        healthCounter.text = health.ToString();
    }
    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShootingAreaArrow"))
        {
            DecreaseHealth();
            Destroy(other.gameObject);
        }
    }
}
