using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class GraphContainer : MonoBehaviour
{
    [Header("Configurações de Escala")]
    public float yMax = 50f;
    public float yMin = -50f;

    [Header("Ajuste Visual (Tamanho e Posição)")]
    [Tooltip("Arraste o Texto do Topo aqui")]
    public TMP_Text labelTop;
    
    [Tooltip("Largura total da linha")]
    public float graphWidth = 1.0f; 

    [Tooltip("Empurrar a linha para os lados (Positivo = Direita, Negativo = Esquerda)")]
    public float xOffset = 0.0f; // <--- NOVA VARIÁVEL PARA CORRIGIR O VAZAMENTO

    [Header("Resolução")]
    public int resolution = 100;

    private LineRenderer lr;
    private List<float> values = new List<float>();

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 0;
    }

    public void SetMaxY(float newMax)
    {
        yMax = newMax;
        if (labelTop != null) labelTop.text = "+" + newMax.ToString("F1");
    }

    public void AddValue(float newValue)
    {
        values.Add(newValue);
        if (values.Count > resolution) values.RemoveAt(0);
        DrawGraph();
    }

    private void DrawGraph()
    {
        int count = values.Count;
        lr.positionCount = count;
        Vector3[] points = new Vector3[count];
        
        float stepX = 0;
        if (count > 1) stepX = graphWidth / (count - 1); 

        // Começa da esquerda (baseado na largura) + o seu ajuste manual (offset)
        float startX = (-graphWidth / 2f) + xOffset; 

        for (int i = 0; i < count; i++)
        {
            float xPos = startX + (i * stepX);
            
            float range = yMax - yMin;
            if (range <= 0.001f) range = 1f;

            float normalizedY = Mathf.InverseLerp(yMin, yMax, values[i]);
            float yPos = -0.5f + normalizedY; 

            // Mantém Z negativo para ficar na frente
            points[i] = new Vector3(xPos, yPos, -0.05f);
        }
        lr.SetPositions(points);
    }

    public void Clear()
    {
        values.Clear();
        lr.positionCount = 0;
    }
}