using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class GraphLine : MonoBehaviour
{
    public float timeWindow = 5.0f; // O gráfico mostra os últimos 5 segundos
    public float yMultiplier = 1.0f; // Escala vertical (zoom no Y)
    public float xSpacing = 1.0f;    // Velocidade de rolagem horizontal

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private float timer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false; // Importante: Gráfico se move com o objeto
    }

    public void AddValue(float value)
    {
        // Adiciona um ponto novo na posição (0, valor, 0)
        // O eixo X será ajustado no Update para criar o efeito de rolagem
        points.Add(new Vector3(0, value * yMultiplier, 0));

        // Limpa pontos muito velhos para não pesar a memória
        if (points.Count > 1000) points.RemoveAt(0);
    }

    void Update()
    {
        // Avança o tempo
        float timeStep = Time.deltaTime * xSpacing;
        
        // Move todos os pontos para a esquerda
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p = points[i];
            p.x -= timeStep; // Desloca para a esquerda
            points[i] = p;
        }

        // Remove pontos que saíram da tela (assumindo largura de 5 a 10 unidades)
        if (points.Count > 0 && points[0].x < -timeWindow)
        {
            points.RemoveAt(0);
        }

        // Atualiza o desenho da linha
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
    
    // Função para limpar o gráfico (reset)
    public void Clear()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
    }
}