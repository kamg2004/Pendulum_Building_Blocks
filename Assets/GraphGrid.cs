using UnityEngine;
using TMPro; // TextMeshPro for numbers

public class GraphGrid : MonoBehaviour
{
    [Header("Settings")]
    public int timeSegments = 5;  // Vertical lines (seconds)
    public int angleSegments = 4; // Horizontal lines (angles)
    public Vector2 graphSize = new Vector2(8, 4);
    public Color gridColor = new Color(1, 1, 1, 0.2f); // Faint white
    public GameObject labelPrefab; // OPTIONAL: Drag a TextMeshPro object here

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // 1. Create Vertical Lines (Time)
        for (int i = 0; i <= timeSegments; i++)
        {
            float x = (graphSize.x / timeSegments) * i;
            CreateLine(new Vector3(x, 0, 0), new Vector3(x, graphSize.y, 0));
        }

        // 2. Create Horizontal Lines (Angle)
        for (int i = 0; i <= angleSegments; i++)
        {
            float y = (graphSize.y / angleSegments) * i;
            CreateLine(new Vector3(0, y, 0), new Vector3(graphSize.x, y, 0));
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(this.transform, false);
        
        // Add LineRenderer
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        
        // VISUAL SETUP
        lr.material = new Material(Shader.Find("Sprites/Default")); // Fixes pink line
        lr.startColor = gridColor;
        lr.endColor = gridColor;
        lr.startWidth = 0.003f; // Thickness of grid lines
        lr.endWidth = 0.003f;
        
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}