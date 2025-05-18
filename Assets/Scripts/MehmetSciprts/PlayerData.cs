using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public int metalPara = 500;

    // Zeplin (Üs) geliştirmeleri
    public int zeplinSaglik = 100;
    public int zeplinSaglikLevel = 0;

    public int zeplinMinigunDamage = 10;
    public int zeplinMinigunLevel = 0;
    public float zeplinMinigunCooldown = 1.0f;
    public int zeplinMinigunCount = 1;

    public int zeplinRoketDamage = 20;
    public int zeplinRoketLevel = 0;
    public int zeplinRoketCount = 1;
    public float zeplinRoketDelay = 2.0f;

    // Ana Gemi geliştirmeleri
    public int anaGemiSaglik = 100;
    public int anaGemiSaglikLevel = 0;

    public int anaGemiMinigunDamage = 10;
    public int anaGemiMinigunLevel = 0;
    public float anaGemiMinigunCooldown = 1.0f;
    public int anaGemiMinigunCount = 1;

    public int anaGemiRoketDamage = 20;
    public int anaGemiRoketLevel = 0;
    public int anaGemiRoketCount = 1;
    public float anaGemiRoketDelay = 2.0f;

    public Button zeplinMinigunButon; // Inspector'dan atayacaksın
    public Button anaGemiMinigunButon;
    public Button anaGemiRoketButon;
    public Button anaGemiSaglikButon;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Ana Gemi Minigun Geliştirme
    
}
