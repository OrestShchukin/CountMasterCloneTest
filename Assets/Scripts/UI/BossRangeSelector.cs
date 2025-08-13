using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using NUnit.Framework.Constraints;
using UnityEngine;

public class BossRangeSelector : MonoBehaviour
{
    [SerializeField] private Transform arrowAxis;
    public static bool selected = false;

    [SerializeField] private float minAngle = -42f;
    [SerializeField] private float maxAngle = 42f;
    [SerializeField] float rotationSpeed = 100f;

    [System.Serializable]
    public class RangeAngleToModifier
    {
        public Vector2 AngleStartEnd; // [start, end] у градусах, наприклад
        public int AddModifier;
    }

    [SerializeField] private List<RangeAngleToModifier> ranges = new();

    
    private int direction = 1;

    private void Awake()
    {
        selected = false;
    }

    private void Update()
    {
        if (!selected)
        {
            RotateBetweenAngles();
        }
    }

    private void RotateBetweenAngles()
    {
        float newY = arrowAxis.localEulerAngles.z;

        // Конвертуємо у [-180..180] для зручності
        if (newY > 180f) newY -= 360f;
        
        newY += rotationSpeed * direction * Time.unscaledDeltaTime;
        
        if (newY >= maxAngle)
        {
            newY = maxAngle;
            direction = -1; 
        }
        else if (newY <= minAngle)
        {
            newY = minAngle;
            direction = 1;
        }
        
        arrowAxis.localEulerAngles = new Vector3(
            arrowAxis.localEulerAngles.x,
            arrowAxis.localEulerAngles.y,
            newY
        );
    }
    

    public void SelectModifierOnScreenPress()
    {
        selected = true;
        float newY = arrowAxis.localEulerAngles.z;
        if (newY > 180f) newY -= 360f;
        
        
        int followersToAdd = GetModifierForAngle(newY);
        Debug.Log($"Modifier for the euler angle {newY} is {followersToAdd}");

        StartCoroutine(resumeAfterDelay());


        IEnumerator resumeAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            UIManager.UIManagerInstance.CloseBossRangeModifierUI();
            PlayerSpawner.playerSpawnerInstance.AddFollowers(followersToAdd);
            CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(2);
        }
    }
        
    private int GetModifierForAngle(float angle)
    {
        foreach (var r in ranges)
        {
            // якщо діапазони в межах [0..360) без «wrap-around»
            if (angle <= r.AngleStartEnd.x && angle > r.AngleStartEnd.y)
                return r.AddModifier;
        }

        return 0;
    }
    
}
