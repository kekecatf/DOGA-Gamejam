using UnityEngine;

public class EngelHareket : MonoBehaviour
{
    public float moveSpeed = 3f; // Engel yukarıya hareket hızı
    public float yokOlmaY = 13.2f;  // Engel bu Y değerine ulaşınca yok olacak

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Engel yukarıya hareket etsin
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.Self);

        // Belirli bir Y noktasına gelince yok olsun
        if (transform.position.y >= yokOlmaY)
        {
            Destroy(gameObject);
        }
    }
}
