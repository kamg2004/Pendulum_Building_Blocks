using UnityEngine;
using System.IO.Ports;


public class SerialRotation : MonoBehaviour
{
    // Change this to your Micro's COM port (check Arduino IDE)
    SerialPort stream = new SerialPort("COM10", 115200); 
    public LineRenderer velocityLine;
 public float velocityScale = 1.0f; 
[Range(0.01f, 1.0f)]
public float smoothing = 0.001f; // Lower = smoother/slower, Higher = more jitter/faster
public Transform sensorHead;
private Vector3 lastPosition;
private Vector3 filteredVelocity; // The "memory" variable

    void Start()
    {
        stream.Open();
        stream.ReadTimeout = 50;
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
                    // 2. Calculate Raw Velocity
                    Vector3 currentPosition = sensorHead.position;
                    Vector3 rawVelocity = (currentPosition - lastPosition) / Time.deltaTime;

                    // 3. APPLY LOW-PASS FILTER
                    // We blend the new raw data with our previous filtered data
                    // Formula: Filtered = (Old * (1-alpha)) + (New * alpha)
                    filteredVelocity = Vector3.Lerp(filteredVelocity, rawVelocity, smoothing);

                    // 4. Update the Line Renderer
                    if (velocityLine != null)
                    {
                        velocityLine.SetPosition(0, currentPosition);
                        velocityLine.SetPosition(1, currentPosition + (filteredVelocity * velocityScale));
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