using TMPro;
using UnityEngine;

public class woodenBox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TextMeshPro healthCounter;
    public float health = 5;


    public void DecreaseHealth()
    {
        health--;
        healthCounter.text = health.ToString();
    }

    public void UpdateHealthCounter()
    {
        healthCounter.text = health.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
