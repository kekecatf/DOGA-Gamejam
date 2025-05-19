using UnityEngine;

public class KarakterHareket : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Engel"))
        {
            // Karakteri yavaşlat
            GetComponent<Rigidbody2D>().linearVelocity *= 0.5f; // Hızı yarıya indir
            Destroy(other.gameObject); // Engeli yok et
        }
    }
}
