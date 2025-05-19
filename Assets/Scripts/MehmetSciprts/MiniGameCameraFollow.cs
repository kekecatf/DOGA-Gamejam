using UnityEngine;

public class MiniGameCameraFollow : MonoBehaviour
{
    public Transform target; // Takip edilecek obje (Player)
    public Vector3 offset = new Vector3(0, 0, -10); // Kameranın hedefe göre konumu

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
