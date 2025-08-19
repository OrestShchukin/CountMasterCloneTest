using System;
using UnityEngine;

public class ShootingAreaEndScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShootingAreaArrow"))
        {
            GameObject arrow = other.gameObject;
            arrow.SetActive(false);
            Destroy(arrow);
        }
    }
}
