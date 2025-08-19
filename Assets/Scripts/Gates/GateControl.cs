using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;

public class GateControl : MonoBehaviour
{
    public enum GateType
    {
        Add,
        Multiply
    }
    
    [SerializeField] TextMeshPro textDisplay;
    [SerializeField] bool randomAction = true;
    
    public GateType gateType = GateType.Add;
    public int value = 10;

    

    void Start()
    {
        // int unitsMultiplier = LevelGenerator.segmentIndex > 13 ? LevelGenerator.segmentIndex : 13;
        if (randomAction)
        {
            gateType = Random.Range(0, 2) == 1 ? GateType.Multiply : GateType.Add;
        }

        CalculateNum();
    }

    public void Activate(PlayerSpawner spawner)
    {
        if (spawner != null)
        {
            if (gateType == GateType.Add)
                spawner.AddFollowers(value);
            else if (gateType == GateType.Multiply)
                spawner.MultiplyFollowers(value);
        }

        if (transform.parent != null && transform.parent.parent != null)
        {
            if (transform.parent.parent.gameObject.CompareTag("GateManager"))
            {
                Transform gateManager = transform.parent.parent;
                gateManager.GetChild(0).Find("TriggerZone").GetComponent<BoxCollider>().enabled = false;
                gateManager.GetChild(1).Find("TriggerZone").GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

    void CalculateNum()
    {
        int[] nums = { 10, 15, 20, 25};
        int segmentIndex = LevelGenerator.segmentIndex;
        if (gateType == GateType.Multiply)
        {
            int minMultiplyNum = 2, maxMultiplyNum = 10;
            if (segmentIndex < nums[0])
            {
                maxMultiplyNum = 10;
            }
            else if (segmentIndex < nums[1])
            {
                maxMultiplyNum = 6;
            }
            else if (segmentIndex < nums[2])
            {
                maxMultiplyNum = 4;
            }
            else
            {
                maxMultiplyNum = 3;
            }
            
            value = Random.Range(minMultiplyNum, maxMultiplyNum + 1);
            textDisplay.text = $"X{value}";
        }
        else if (gateType == GateType.Add)
        {
            int minAddNum = 10, maxAddNum = 100;
            if (segmentIndex < nums[0])
            {
                minAddNum = 5;
                maxAddNum = 20;
            }
            else if (segmentIndex < nums[1])
            {
                minAddNum = 10;
                maxAddNum = 25;
            }
            else if (segmentIndex < nums[2])
            {
                minAddNum = 20;
                maxAddNum = 40;
            }
            else
            {
                minAddNum = 20;
                maxAddNum = 50;
            }

            value = Random.Range(minAddNum, maxAddNum);
            textDisplay.text = $"+{value}";
        }
    }
}