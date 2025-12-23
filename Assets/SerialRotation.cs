using UnityEngine;
using System.IO.Ports;

public class SerialRotation : MonoBehaviour
{
    // Change this to your Micro's COM port (check Arduino IDE)
    SerialPort stream = new SerialPort("COM10", 115200); 

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