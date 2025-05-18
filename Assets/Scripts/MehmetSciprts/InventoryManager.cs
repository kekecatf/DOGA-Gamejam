using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public PlayerData playerData;
    public Text metalParaText;
    public Button zeplinMinigunButon;
    public Button zeplinRoketButon;
    public Button zeplinSaglikButon;

    // Geliştirme maliyetleri
    public int saglikCost = 100;
    public int minigunCost = 150;
    public int roketCost = 200;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI();
    }

    public void ZeplinMinigunUpgrade()
    {
        if (playerData.zeplinMinigunLevel >= 4) return;

        int cost = minigunCost * (playerData.zeplinMinigunLevel + 1);
        if (playerData.metalPara < cost) return;

        playerData.metalPara -= cost;
        playerData.zeplinMinigunLevel++;

        switch (playerData.zeplinMinigunLevel)
        {
            case 1:
                playerData.zeplinMinigunDamage = Mathf.RoundToInt(playerData.zeplinMinigunDamage * 1.25f);
                break;
            case 2:
                playerData.zeplinMinigunDamage = Mathf.RoundToInt(playerData.zeplinMinigunDamage / 1.25f * 1.5f);
                break;
            case 3:
                playerData.zeplinMinigunCooldown += 1;
                break;
            case 4:
                playerData.zeplinMinigunCount += 1;
                break;
        }
        UpdateUI();

        if (playerData.zeplinMinigunLevel >= 4)
            zeplinMinigunButon.interactable = false;
    }

    public void ZeplinRoketUpgrade()
    {
        if (playerData.zeplinRoketLevel >= 4) return;

        int cost = roketCost * (playerData.zeplinRoketLevel + 1);
        if (playerData.metalPara < cost) return;

        playerData.metalPara -= cost;
        playerData.zeplinRoketLevel++;

        switch (playerData.zeplinRoketLevel)
        {
            case 1:
                playerData.zeplinRoketDamage = Mathf.RoundToInt(playerData.zeplinRoketDamage * 1.10f);
                break;
            case 2:
                playerData.zeplinRoketDamage = Mathf.RoundToInt(playerData.zeplinRoketDamage / 1.10f * 1.25f);
                break;
            case 3:
                playerData.zeplinRoketCount = 2;
                break;
            case 4:
                playerData.zeplinRoketDelay *= 0.8f;
                break;
        }
        UpdateUI();

        if (playerData.zeplinRoketLevel >= 4)
            zeplinRoketButon.interactable = false;
    }

    public void ZeplinSaglikUpgrade()
    {
        if (playerData.zeplinSaglikLevel >= 4) return;

        int cost = saglikCost * (playerData.zeplinSaglikLevel + 1);
        if (playerData.metalPara < cost) return;

        playerData.metalPara -= cost;
        playerData.zeplinSaglikLevel++;

        switch (playerData.zeplinSaglikLevel)
        {
            case 1:
                playerData.zeplinSaglik = Mathf.RoundToInt(playerData.zeplinSaglik * 1.25f);
                break;
            case 2:
                playerData.zeplinSaglik = Mathf.RoundToInt(playerData.zeplinSaglik / 1.25f * 1.5f);
                break;
            case 3:
                playerData.zeplinSaglik = Mathf.RoundToInt(playerData.zeplinSaglik / 1.5f * 1.75f);
                break;
            case 4:
                playerData.zeplinSaglik = Mathf.RoundToInt(playerData.zeplinSaglik / 1.75f * 2f);
                break;
        }
        UpdateUI();

        if (playerData.zeplinSaglikLevel >= 4)
            zeplinSaglikButon.interactable = false;
    }

    public void AnaGemiSaglikUpgrade()
    {
        if (playerData.metalPara >= saglikCost)
        {
            playerData.anaGemiSaglik += 20;
            playerData.metalPara -= saglikCost;
            UpdateUI();
        }
    }

    public void AnaGemiMinigunUpgrade()
    {
        if (playerData.metalPara >= minigunCost)
        {
            playerData.anaGemiMinigunLevel++;
            playerData.metalPara -= minigunCost;
            UpdateUI();
        }
    }

    public void AnaGemiRoketUpgrade()
    {
        if (playerData.metalPara >= roketCost)
        {
            playerData.anaGemiRoketLevel++;
            playerData.metalPara -= roketCost;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        metalParaText.text = "Metal: " + playerData.metalPara;
        // seviye, hasar, cooldown gibi diğer UI elemanlarını da burada güncelleyebilirsin
    }

    public void GeriDon()
    {
        SceneManager.LoadScene("Oynanis"); // Oyun sahnenin adı neyse onu yaz
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
