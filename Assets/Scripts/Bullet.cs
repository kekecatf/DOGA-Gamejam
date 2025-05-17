using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;  // Mermi ömrü - belirli süre sonra yok olur
    
    private bool isMovingLeft = false;
    
    void Start()
    {
        // Belirli süre sonra mermiyi yok et
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Hareket yönünü belirle (transform.right kullanarak rotasyona göre)
        Vector2 direction = transform.right;
        
        // Eğer sola bakıyorsa, yönü tersine çevir
        if (isMovingLeft)
        {
            direction = -direction;
        }
        
        // Hesaplanan yönde hareket et
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
    
    // Oyuncudan yön bilgisini al
    public void SetDirection(bool isLeft)
    {
        isMovingLeft = isLeft;
        
        // Sprite'ın yönünü ayarla (sol veya sağ)
        Vector3 scale = transform.localScale;
        scale.x = isMovingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    
    // Rotasyon bilgisini al (firePoint'in rotasyonu)
    public void SetRotation(float newRotation)
    {
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Düşmanla çarpışma kontrolü
        if (other.CompareTag("Enemy"))
        {
            // Düşmanı yok et
            Destroy(other.gameObject);
            
            // Mermiyi yok et
            Destroy(gameObject);
            
            Debug.Log("Mermi düşmana çarptı: " + other.name);
        }
    }
} 