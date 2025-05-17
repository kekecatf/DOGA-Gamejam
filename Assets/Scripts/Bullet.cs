using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 2f;
    
    // Hareket yönü (1 sağa, -1 sola)
    private int direction;
    // Mermi rotasyonu
    private float rotation = 0f;
    // X-flip durumu
    private bool isFlippedX = false;
    
    private Vector2 moveDirection; // Hareket yönü vektörü
    
    private void Start()
    {
        // Belirli bir süre sonra mermiyi yok et
        Destroy(gameObject, lifetime);
    }
    
    public void SetDirection(bool isMovingLeft)
    {
        // Hareket yönünü ayarla (-1: sol, 1: sağ)
        direction = isMovingLeft ? -1 : 1;
        // X flip durumunu kaydet
        isFlippedX = isMovingLeft;
        
        // İlk hareket yönünü ayarla
        CalculateMoveDirection();
    }
    
    public void SetRotation(float rotationAngle)
    {
        // Mermi rotasyonunu ayarla
        rotation = rotationAngle;
        
        // Fiziksel rotasyonu uygula - Z ekseni etrafında döndür
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        
        // X rotasyonunu flip durumuna göre tersine çevir
        Vector3 currentScale = transform.localScale;
        currentScale.x = isFlippedX ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);
        transform.localScale = currentScale;
        
        // Hareket yönünü yeniden hesapla
        CalculateMoveDirection();
        
        // Debug için
        Debug.Log("Mermi rotasyon ayarlandı: " + rotation + ", Yön: " + direction + ", X-Scale: " + currentScale.x);
    }
    
    private void CalculateMoveDirection()
    {
        // Flip durumu ve rotasyona göre hareket yönünü hesapla
        if (isFlippedX) 
        {
            // Sola gidiyorsa (flip durumunda)
            float radians = (180f - rotation) * Mathf.Deg2Rad; // Açıyı düzelt
            moveDirection = new Vector2(-Mathf.Cos(radians), -Mathf.Sin(radians)).normalized;
        }
        else 
        {
            // Sağa gidiyorsa (normal durum)
            float radians = rotation * Mathf.Deg2Rad;
            moveDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
        }
        
        Debug.Log("Hareket yönü hesaplandı: " + moveDirection + " (Flip: " + isFlippedX + ", Rot: " + rotation + ")");
    }
    
    private void Update()
    {
        // Önceden hesaplanmış yönde ilerleme
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Düşmanla çarpışma kontrolü
        if (collision.CompareTag("Enemy"))
        {
            // Düşmana hasar ver
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage();
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
        }
    }
} 