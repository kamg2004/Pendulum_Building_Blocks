using UnityEngine;
using System.Collections.Generic;

public class EnergyGraph : MonoBehaviour
{
    [Header("Settings")]
    public float timeWindow = 5.0f;
    public float maxEnergy = 2.0f; 
    public float graphWidth = 8.0f;
    public float graphHeight = 4.0f;
    public float updateInterval = 0.05f;

    [Header("Visuals")]
    public LineRenderer keLine; // Kinetic (Red)
    public LineRenderer peLine; // Potential (Blue)
    public LineRenderer teLine; // Total (Green/Yellow)

    // Internal Data
    private List<float> keBuffer = new List<float>();
    private List<float> peBuffer = new List<float>();
    private List<float> teBuffer = new List<float>(); // <--- NEW
    private float timer;

    void Start()
    {
        SetupLine(keLine, Color.red);    // Kinetic
        SetupLine(peLine, Color.blue);   // Potential
        SetupLine(teLine, Color.yellow); // Total (Yellow stands out well)
    }

    void SetupLine(LineRenderer lr, Color c)
    {
        if (lr == null) return;
        lr.useWorldSpace = false;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = c;
        lr.endColor = c;
    }

    // UPDATED FUNCTION: Now accepts 3 arguments
    public void AddEnergyValues(float ke, float pe, float te)
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            keBuffer.Add(ke);
            peBuffer.Add(pe);
            teBuffer.Add(te); // <--- NEW
            timer = 0;

            // Maintain 5-second window
            int maxPoints = Mathf.RoundToInt(timeWindow / updateInterval);
            while (keBuffer.Count > maxPoints) 
            {
                keBuffer.RemoveAt(0);
                peBuffer.RemoveAt(0);
                teBuffer.RemoveAt(0); // <--- NEW
            }

            DrawGraph(keLine, keBuffer);
            DrawGraph(peLine, peBuffer);
            DrawGraph(teLine, teBuffer); // <--- NEW
        }
    }

    void DrawGraph(LineRenderer lr, List<float> data)
    {
        if (lr == null) return;
        lr.positionCount = data.Count;
        for (int i = 0; i < data.Count; i++)
        {
            float x = (i / (float)data.Count) * graphWidth;
            float y = (data[i] / maxEnergy) * graphHeight;
            
            // Note: We put TE slightly behind or in front of others to avoid Z-fighting
            // KE/PE at -0.1, TE at -0.15 (closer to camera)
            float z = (lr == teLine) ? -0.15f : -0.1f; 
            
            lr.SetPosition(i, new Vector3(x, y, z)); 
        }
    }
}