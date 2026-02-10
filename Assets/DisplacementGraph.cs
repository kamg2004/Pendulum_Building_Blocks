using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class DisplacementGraph : MonoBehaviour
{
    // SETTINGS
    [Header("Graph Settings")]
    public float timeWindow = 5.0f; // Show past 5 seconds
    public float graphWidth = 8.0f; // Width in Unity units
    public float graphHeight = 4.0f; // Height in Unity units
    public float maxAngle = 90.0f;   // The angle that reaches the top of the graph

    // INTERNAL DATA
    private LineRenderer lr;
    private List<float> valueBuffer = new List<float>();
    private float updateInterval; 
    private float timer;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        
        // AUTO-SETUP: Fixes invisible/pink lines automatically
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.useWorldSpace = false; // Moves with the object
        lr.material = new Material(Shader.Find("Sprites/Default")); // Fixes pink square
        lr.startColor = Color.green;
        lr.endColor = Color.green;

        // Calculate how fast to update (e.g. 20 times a second is enough for a smooth graph)
        updateInterval = 0.05f; 
    }

    public void AddDataPoint(float angle)
    {
        timer += Time.deltaTime;

        // Only add data at fixed intervals to keep the graph steady
        if (timer >= updateInterval)
        {
            valueBuffer.Add(angle);
            timer = 0;

            // Remove old data to keep exactly 5 seconds worth
            // (5 seconds / 0.05 interval = 100 points)
            int maxPoints = Mathf.RoundToInt(timeWindow / updateInterval);
            while (valueBuffer.Count > maxPoints)
            {
                valueBuffer.RemoveAt(0);
            }

            DrawGraph();
        }
    }

    void DrawGraph()
    {
        lr.positionCount = valueBuffer.Count;

        for (int i = 0; i < valueBuffer.Count; i++)
        {
            // Calculate X: 0 to graphWidth
            float x = (i / (float)valueBuffer.Count) * graphWidth;
            
            // Calculate Y: Normalized by maxAngle
            float y = (valueBuffer[i] / maxAngle) * graphHeight;

            lr.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}