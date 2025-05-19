using UnityEngine;

public class EngelSpawn : MonoBehaviour
{
    public GameObject engelPrefab;
    public float spawnInterval = 1.0f;
    public int maxEngelSayisi = 5;
    public float toplamSure = 8f;
    public Transform zemin; // Inspector'dan Zemin nesnesini ata

    private int cikanEngelSayisi = 0;
    private float gecenSure = 0f;

    void Start()
    {
        InvokeRepeating("SpawnEngel", 0f, spawnInterval);
    }

    void SpawnEngel()
    {
        if (cikanEngelSayisi >= maxEngelSayisi || gecenSure >= toplamSure)
        {
            CancelInvoke("SpawnEngel");
            return;
        }

        // Zemin sınırlarını hesapla
        float zeminX = zemin.position.x;
        float zeminWidth = zemin.localScale.x;
        float minX = zeminX - zeminWidth / 2f;
        float maxX = zeminX + zeminWidth / 2f;

        float randomX = Random.Range(minX, maxX);
        float spawnY = -4.5f; // Mevcut spawn yüksekliğin

        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
        if (engelPrefab != null)
        {
            Instantiate(engelPrefab, spawnPos, engelPrefab.transform.rotation);
        }
        else
        {
            Debug.LogError("Engel prefab'ı atanmamış!");
        }

        cikanEngelSayisi++;
        gecenSure += spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
