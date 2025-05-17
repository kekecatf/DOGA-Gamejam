using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;  // Mermi ömrü - belirli süre sonra yok olur
    
    private bool isMovingLeft = false;
    private float rotation = 0f;
    
    void Start()
    {
        // Belirli süre sonra mermiyi yok et
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // İleri yönde hareket et (sola veya sağa doğru)
        Vector2 direction = isMovingLeft ? Vector2.left : Vector2.right;
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
    
    // Oyuncudan rotasyon bilgisini al
    public void SetRotation(float newRotation)
    {
        rotation = newRotation;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
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
        }
    }
} 