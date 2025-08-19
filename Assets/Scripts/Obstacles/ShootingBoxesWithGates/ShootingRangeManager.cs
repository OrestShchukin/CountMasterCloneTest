using System.Collections.Generic;
using UnityEngine;

public class ShootingRangeManager : MonoBehaviour
{
    [Header("Boxes Scripts")] 
    [SerializeField] private WoodenBox box1;
    [SerializeField] private WoodenBox box2;
    [SerializeField] private WoodenBox box3;

    [Header("Gate Scripts")]
    [SerializeField] private GateControl gate1;
    [SerializeField] private GateControl gate2;
    [SerializeField] private GateControl gate3;
    
    
    private List<WoodenBox> boxes;

    private List<GateControl> gates;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxes = new List<WoodenBox>() {box1, box2, box3};
        gates = new List<GateControl>() {gate1, gate2, gate3};
        Invoke(nameof(CalculateHealth), 0.5f);;
        
    }

    void CalculateHealth()
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            int health = 0;
            GateControl gate = gates[i];
            if (gate.gateType == GateControl.GateType.Add)
            {
                health += gate.value + Random.Range(5, 25);
            }
            else if (gate.gateType == GateControl.GateType.Multiply)
            {
                health += gate.value * Random.Range(5, 15);
            }
            boxes[i].health = health;
            boxes[i].UpdateHealthCounter();
        }
    }
    
}
