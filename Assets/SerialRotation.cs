using UnityEngine;
using System.IO.Ports;


public class SerialRotation : MonoBehaviour
{
    // Change this to your Micro's COM port (check Arduino IDE)
    SerialPort stream = new SerialPort("COM8", 115200); 
    public LineRenderer velocityLine;
    public DisplacementGraph graph;
    public EnergyGraph energyGraph;
    public float PendulumLength = 0.5f; // Length in meters (measure your string!)
    public float Gravity = 9.81f;
 public float velocityScale = 10000.0f; 
[Range(0.01f, 1.0f)]
public float smoothing = 0.0001f; // Lower = smoother/slower, Higher = more jitter/faster
public Transform sensorHead;
public Transform rodCylinder;
private Vector3 lastPosition;
private Vector3 filteredVelocity; // The "memory" variable

    void Start()
    {
        // 1. READ THE CHOICE FROM THE MENU
        // If we came from the menu, this static variable has the user's choice
        PendulumLength = MenuManager.chosenLength;
    

        UpdatePendulumVisuals();

        stream.Open();
        stream.ReadTimeout = 50;
    }

    void UpdatePendulumVisuals()
    {
        if (rodCylinder != null && sensorHead != null)
        {
            // A. Update the Sensor Head (The Weight)
            // Move it straight down to negative Length
            sensorHead.localPosition = new Vector3(0, -PendulumLength, 0);

            // B. Update the Rod (The Cylinder)
            // A standard Unity Cylinder is 2 meters tall by default.
            // So we scale Y by (Length / 2).
            float newScaleY = PendulumLength / 2.0f;
            rodCylinder.localScale = new Vector3(rodCylinder.localScale.x, newScaleY, rodCylinder.localScale.z);

            // Move the cylinder down by half the length so its top attaches to the pivot
            rodCylinder.localPosition = new Vector3(0, -PendulumLength / 2.0f, 0);
        }
    }
    void Update()
    {
        if (stream.IsOpen)
        {
            try
            {
                string data = stream.ReadLine();
                string[] values = data.Split(',');

                if (values.Length == 4)
                {
                    float w = float.Parse(values[0]);
                    float x = float.Parse(values[1]);
                    float y = float.Parse(values[2]);
                    float z = float.Parse(values[3]);

                    // Note: MPU6050 and Unity use different coordinate systems.
                    // We swap and invert axes to match Unity's Left-Handed system.
                    transform.rotation = new Quaternion(-y, -z, x, w);

                    // OLD: Measures everything, including spin
                    // float DisplacementAngle = Quaternion.Angle(Quaternion.identity, transform.rotation);

                    // NEW: Measures only the tilt away from gravity (Vertical Displacement)
                    // We compare the World's "Down" (Gravity) with the Sensor's local "Down" axis.
                    float DisplacementAngle = Vector3.Angle(Vector3.down, transform.rotation * Vector3.down);
                    
                    if (graph != null) 
                    {
                        graph.AddDataPoint(DisplacementAngle);
                    }
                    // 2. Calculate Raw Velocity
                    Vector3 currentPosition = sensorHead.position;
                    Vector3 rawVelocity = (currentPosition - lastPosition) / Time.deltaTime;

                    // 3. APPLY LOW-PASS FILTER
                    // We blend the new raw data with our previous filtered data
                    // Formula: Filtered = (Old * (1-alpha)) + (New * alpha)
                    filteredVelocity = Vector3.Lerp(filteredVelocity, rawVelocity, smoothing);

                    // Kinetic Energy = 0.5 * v^2
                    float speed = filteredVelocity.magnitude;
                    float ke = 0.5f * speed * speed;

                    // Potential Energy = g * h
                    // h = L * (1 - cos(theta))
                    // Convert degrees to radians for Mathf.Cos
                    float rad = DisplacementAngle * Mathf.Deg2Rad;
                    float height = PendulumLength * (1.0f - Mathf.Cos(rad));
                    float pe = Gravity * height;
                    pe=0.5f*pe;
                    ke=0.5f*ke;

                    float te = ke + pe; 

                    if (energyGraph != null)
                    {
                        // Pass all 3 values now
                        energyGraph.AddEnergyValues(ke, pe, te);
                    }
                    

                    // 4. Update the Line Renderer (Visuals Only)
                    if (velocityLine != null)
                    {
                        // A. Create a SEPARATE vector for drawing
                        // We do NOT change 'filteredVelocity' so physics/energy stays correct.
                        Vector3 visualArrow = filteredVelocity * velocityScale;

                        // B. Clamp the VISUAL length
                        // This prevents the giant lines from shooting off screen
                        visualArrow = Vector3.ClampMagnitude(visualArrow, 1.0f); // Max arrow length = 1 meter

                        // Only draw if we are actually moving
                        if (visualArrow.magnitude > 0.05f) 
                        {
                            velocityLine.positionCount = 5; // We need 5 points to draw: Line -> Tip -> Left -> Tip -> Right

                            Vector3 start = currentPosition;
                            Vector3 end = start + visualArrow;

                            // C. Calculate the Arrowhead Wings
                            Vector3 direction = visualArrow.normalized;
                            // Find the "Right" direction relative to the arrow (so wings are flat)
                            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
                            if (right == Vector3.zero) right = Vector3.right; // Safety check

                            float tipSize = 0.1f; // Size of the little wing lines

                            // Calculate wing positions
                            Vector3 arrowLeft = end - (direction * tipSize) + (right * tipSize * 0.5f);
                            Vector3 arrowRight = end - (direction * tipSize) - (right * tipSize * 0.5f);

                            // D. Draw the sequence
                            velocityLine.SetPosition(0, start);      // Start of line
                            velocityLine.SetPosition(1, end);        // Tip of arrow
                            velocityLine.SetPosition(2, arrowLeft);  // Left wing
                            velocityLine.SetPosition(3, end);        // Back to Tip
                            velocityLine.SetPosition(4, arrowRight); // Right wing
                        }
                        else
                        {
                            // If stopped, hide the line
                            velocityLine.positionCount = 0;
                        }
                    }

                    // 5. Store position for next frame
                    lastPosition = currentPosition;
                }
            }
            catch (System.Exception)
            {
                // Silently skip corrupted frames
            }
        }
        
  
    }

    void OnApplicationQuit()
    {
        stream.Close();
    }
}