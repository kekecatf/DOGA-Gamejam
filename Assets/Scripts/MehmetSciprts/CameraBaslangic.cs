using UnityEngine;

public class CameraZoomAndFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 startOffset = new Vector3(0, 10, -20); // Başlangıçta uzak pozisyon
    public Vector3 followOffset = new Vector3(0, 2, -10); // Takip pozisyonu
    public float zoomDuration = 3.5f;

    private float timer = 0f;
    private bool zooming = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = player.position + startOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (zooming)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(player.position + startOffset, player.position + followOffset, timer / zoomDuration);
            if (timer >= zoomDuration)
            {
                zooming = false;
            }
        }
        else
        {
            // Normal takip
            transform.position = player.position + followOffset;
        }
    }
}
