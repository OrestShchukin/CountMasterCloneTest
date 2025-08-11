using TMPro;
using UnityEngine;

public class GateControl : MonoBehaviour
{
    public enum GateType { Add, Multiply }

    [Header("UI")]
    [SerializeField] TextMeshPro textDisplay;

    [Header("Logic")]
    [SerializeField] bool randomAction = true;
    [SerializeField] GateType gateType = GateType.Add;
    [SerializeField] public int value = 10;

    void Start()
    {
        if (randomAction)
            gateType = Random.Range(0, 2) == 1 ? GateType.Multiply : GateType.Add;
        CalculateNum();
    }

    // Called from PlayerControl when player hits a Gate trigger
    public void Activate(PlayerSpawner spawner)
    {
        if (spawner != null)
        {
            switch (gateType)
            {
                case GateType.Add:
                    spawner.AddFollowers(value);
                    break;
                case GateType.Multiply:
                    spawner.MultiplyFollowers(Mathf.Max(2, value)); // safety: at least x2
                    break;
            }
        }

        // disable both sibling gates' trigger zones to avoid double-activation
        if (transform.parent != null && transform.parent.parent != null && transform.parent.parent.CompareTag("GateManager"))
        {
            Transform gateManager = transform.parent.parent;
            for (int i = 0; i < gateManager.childCount; i++)
            {
                var t = gateManager.GetChild(i).Find("TriggerZone");
                if (t) { var box = t.GetComponent<BoxCollider>(); if (box) box.enabled = false; }
            }
        }
    }

    void CalculateNum()
    {
        int[] nums = { 10, 15, 20, 25 };
        int segmentIndex = LevelGenerator.segmentIndex;

        if (gateType == GateType.Multiply)
        {
            int minMultiplyNum = 2, maxMultiplyNum;
            if (segmentIndex < nums[0]) maxMultiplyNum = 10;
            else if (segmentIndex < nums[1]) maxMultiplyNum = 6;
            else if (segmentIndex < nums[2]) maxMultiplyNum = 4;
            else maxMultiplyNum = 3;

            value = Random.Range(minMultiplyNum, maxMultiplyNum + 1);
            if (textDisplay) textDisplay.text = $"x{value}"; // lowercase x to match many UIs
        }
        else // Add
        {
            int minAddNum, maxAddNum;
            if (segmentIndex < nums[0]) { minAddNum = 5;  maxAddNum = 20; }
            else if (segmentIndex < nums[1]) { minAddNum = 10; maxAddNum = 25; }
            else if (segmentIndex < nums[2]) { minAddNum = 20; maxAddNum = 40; }
            else { minAddNum = 20; maxAddNum = 50; }

            value = Random.Range(minAddNum, maxAddNum);
            if (textDisplay) textDisplay.text = $"+{value}";
        }
    }
}