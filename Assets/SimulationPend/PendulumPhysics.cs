using UnityEngine;
using TMPro;

public class PendulumPhysics : MonoBehaviour
{
    [Header("--- Configurações Físicas ---")]
    public float length = 1.0f;        
    public float gravity = 9.81f;     
    public float initialAngle = 45.0f;
    public float mass = 1.0f;

    [Header("--- Controle de Tempo ---")]
    [Range(-2f, 2f)] public float simulationSpeed = 1.0f;

    [Header("--- Referências Visuais ---")]
    public Transform visualRope;    
    public Transform visualBall;    
    public TrailRenderer ballTrail; 

    [Header("--- Gráficos ---")]
    public GraphContainer graphAngle;      
    public GraphContainer graphKinetic; 
    public GraphContainer graphPotential; 
    public GraphContainer graphTotal;   

    [Header("--- Display de Dados ---")]
    public TMP_Text displayPeriodo; 
    public TMP_Text displayAngulo;  

    private float elapsedTime = 0f;

    void Start()
    {
        // Ao iniciar, já calcula a escala correta
        UpdateGraphScales();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime * simulationSpeed;
        if (elapsedTime < 0f) elapsedTime = 0f;
        if (length < 0.1f) length = 0.1f;

        // --- CÁLCULOS FÍSICOS ---
        float angularFrequency = 0f;
        if (length > 0 && gravity >= 0) 
            angularFrequency = Mathf.Sqrt(gravity / length);

        float currentAngleDeg = initialAngle * Mathf.Cos(angularFrequency * elapsedTime);
        float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad; 

        float initialAngleRad = initialAngle * Mathf.Deg2Rad;
        float angularVelocity = -initialAngleRad * angularFrequency * Mathf.Sin(angularFrequency * elapsedTime);
        float linearVelocity = angularVelocity * length;

        // --- ENERGIAS ---
        float height = length * (1 - Mathf.Cos(currentAngleRad));
        float potEnergy = mass * gravity * height;
        float kinEnergy = 0.5f * mass * (linearVelocity * linearVelocity);
        float totalEnergy = potEnergy + kinEnergy;

        // --- VISUAL ---
        transform.localRotation = Quaternion.Euler(0, 0, currentAngleDeg);

        if (visualBall != null && visualRope != null)
        {
            visualBall.localPosition = new Vector3(0, -length, 0);
            float ballScale = 0.2f + (mass * 0.05f); 
            visualBall.localScale = new Vector3(ballScale, ballScale, ballScale);
            
            float ropeScaleY = length / 2.0f;
            visualRope.localScale = new Vector3(0.01f, ropeScaleY, 0.01f); 
            visualRope.localPosition = new Vector3(0, -length / 2.0f, 0);
        }

        UpdateUI(currentAngleDeg);
        UpdateGraphs(currentAngleDeg, kinEnergy, potEnergy, totalEnergy);
    }

    // --- NOVA FUNÇÃO MÁGICA ---
    void UpdateGraphScales()
    {
        // 1. Calcula a Altura Máxima possível (quando o pêndulo está na ponta)
        float h_max = length * (1 - Mathf.Cos(initialAngle * Mathf.Deg2Rad));
        
        // 2. Calcula a Energia Máxima (E = m * g * h_max)
        float maxEnergy = mass * gravity * h_max;

        // Adiciona uma margem de segurança (20%) para a linha não bater no teto exato
        float graphLimit = maxEnergy * 1.2f;
        
        // Se a energia for muito pequena (quase zero), definimos um mínimo para não bugar
        if (graphLimit < 1f) graphLimit = 1f;

        // 3. Aplica aos monitores de ENERGIA
        // Nota: Aplicamos ao "Total" (Pai) e às camadas filhas para ficarem iguais
        if (graphTotal != null) graphTotal.SetMaxY(graphLimit);
        if (graphKinetic != null) graphKinetic.SetMaxY(graphLimit);
        if (graphPotential != null) graphPotential.SetMaxY(graphLimit);

        // 4. (Opcional) Ajusta o gráfico de Ângulo também
        if (graphAngle != null)
        {
            float angleLimit = Mathf.Abs(initialAngle) * 1.2f; 
            if (angleLimit < 10) angleLimit = 10;
            // O gráfico de ângulo é simétrico (vai de -Limit a +Limit)
            graphAngle.yMax = angleLimit;
            graphAngle.yMin = -angleLimit;
            // Atualiza o texto do topo se tiver
            if (graphAngle.labelTop != null) graphAngle.labelTop.text = "+" + angleLimit.ToString("F0") + "°";
        }
    }

    void UpdateGraphs(float angle, float k, float u, float total)
    {
        if (Mathf.Abs(simulationSpeed) > 0.01f)
        {
            if (graphAngle != null) graphAngle.AddValue(angle);
            if (graphKinetic != null) graphKinetic.AddValue(k);
            if (graphPotential != null) graphPotential.AddValue(u);
            if (graphTotal != null) graphTotal.AddValue(total);
        }
    }

    void UpdateUI(float currentAngle)
    {
        if (displayPeriodo != null)
        {
            if (gravity > 0.01f) {
                float periodoT = 2 * Mathf.PI * Mathf.Sqrt(length / gravity);
                displayPeriodo.text = "Period (T): " + periodoT.ToString("F2") + " s";
            } else displayPeriodo.text = "Period: Infinite";
        }
        if (displayAngulo != null) displayAngulo.text = "Angle: " + currentAngle.ToString("F1") + "°";
    }

    // --- SETTERS ATUALIZADOS ---
    // Agora eles chamam UpdateGraphScales() sempre que algo muda!

    public void SetGravity(float v) { gravity = v; ResetGraphs(); UpdateGraphScales(); }
    public void SetLength(float v) { length = v; ResetGraphs(); UpdateGraphScales(); }
    public void SetMass(float v) { mass = v; ResetGraphs(); UpdateGraphScales(); }
    
    // Se você tiver um slider de ângulo inicial:
    public void SetInitialAngle(float v) { initialAngle = v; ResetGraphs(); UpdateGraphScales(); }

    public void SetTimeSpeed(float v) { simulationSpeed = v; }
    
    public void SetPlanet(int index) 
    {
        switch (index) { 
            case 0: gravity = 9.81f; break; 
            case 1: gravity = 1.62f; break; 
            case 2: gravity = 24.79f; break; 
            case 3: gravity = 3.73f; break; 
        }
        ResetSimulation();
        UpdateGraphScales();
    }

    public void TogglePause() {
        if (simulationSpeed == 0) simulationSpeed = 1; else simulationSpeed = 0;
    }

    public void ResetSimulation() {
        elapsedTime = 0f;
        ResetGraphs();
    }

    void ResetGraphs() {
        if (graphAngle != null) graphAngle.Clear();
        if (graphKinetic != null) graphKinetic.Clear();
        if (graphPotential != null) graphPotential.Clear();
        if (graphTotal != null) graphTotal.Clear();
    }
}