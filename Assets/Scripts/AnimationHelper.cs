using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
#endif

// AnimationHelper - Player animasyonunu düzenlemek için yardımcı araç
public class AnimationHelper : MonoBehaviour
{
    // Player referansı - sürükleyip bırak için
    public Player player;

#if UNITY_EDITOR
    [Header("Sprite Kareleri Bulma Ayarları")]
    public string spriteSearchPath = "Assets/Sprites/Player";
    public string spriteNamePattern = "F16_"; // Örneğin F16_1, F16_2, ...
    
    // Editor'da çalışan buton
    [ContextMenu("Player'a Sprite Kareleri Ata")]
    public void FindAndAssignSpriteFrames()
    {
        if (player == null)
        {
            Debug.LogError("Player referansı atanmamış! Önce Player'ı sürükleyip bırakın.");
            return;
        }
        
        // Sprite'ları ara ve sırala
        List<Sprite> foundSprites = FindSpritesByPattern();
        if (foundSprites.Count == 0)
        {
            Debug.LogWarning("Hiç sprite bulunamadı! Arama yolu ve desen doğru mu kontrol edin: " + 
                            spriteSearchPath + ", Desen: " + spriteNamePattern);
            return;
        }
        
        // Player'ın animationFrames dizisini ayarla
        SerializedObject serializedPlayer = new SerializedObject(player);
        SerializedProperty framesProperty = serializedPlayer.FindProperty("animationFrames");
        
        // Diziyi yeniden boyutlandır
        framesProperty.arraySize = foundSprites.Count;
        
        // Sprite'ları diziye ekle
        for (int i = 0; i < foundSprites.Count; i++)
        {
            SerializedProperty elementProperty = framesProperty.GetArrayElementAtIndex(i);
            elementProperty.objectReferenceValue = foundSprites[i];
        }
        
        // Değişiklikleri uygula
        serializedPlayer.ApplyModifiedProperties();
        Debug.Log("Player'a " + foundSprites.Count + " adet sprite karesi atandı!");
    }
    
    // Editor'da sprite'ları bulma
    private List<Sprite> FindSpritesByPattern()
    {
        List<Sprite> sprites = new List<Sprite>();
        
        // Yolun geçerli olduğunu kontrol et
        if (!Directory.Exists(spriteSearchPath))
        {
            Debug.LogError("Belirtilen yol geçerli değil: " + spriteSearchPath);
            return sprites;
        }
        
        // Tüm sprite varlıklarını ara
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new string[] { spriteSearchPath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            // Desenle eşleşiyor mu kontrol et
            if (fileName.Contains(spriteNamePattern))
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
        }
        
        // Sprite'ları sırala (isimlere göre)
        sprites.Sort((a, b) => a.name.CompareTo(b.name));
        
        return sprites;
    }
    
    // F16Fly animasyonunu loop olarak ayarla
    [ContextMenu("F16Fly Animasyonunu Loop Yap")]
    public void MakeF16FlyAnimationLoop()
    {
        string animPath = "Assets/Animations/Player/F16Fly.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
        
        if (clip != null)
        {
            SerializedObject serializedClip = new SerializedObject(clip);
            SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
            SerializedProperty loopTime = settings.FindPropertyRelative("m_LoopTime");
            
            loopTime.boolValue = true;
            serializedClip.ApplyModifiedProperties();
            
            AssetDatabase.SaveAssets();
            Debug.Log("F16Fly animasyonu loop olarak ayarlandı!");
        }
        else
        {
            Debug.LogError("F16Fly animasyonu bulunamadı: " + animPath);
        }
    }
    
    // F16Animator controller'a animasyon state ekle
    [ContextMenu("F16Animator'a Animasyon State Ekle")]
    public void SetupF16AnimatorController()
    {
        string controllerPath = "Assets/Animations/Player/F16Animator.controller";
        UnityEditor.Animations.AnimatorController controller = 
            AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
        
        if (controller != null)
        {
            // AnimationClip'i bul
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/F16Fly.anim");
            if (clip == null)
            {
                Debug.LogError("F16Fly animasyonu bulunamadı!");
                return;
            }
            
            // State machine'i kontrol et
            if (controller.layers == null || controller.layers.Length == 0 || 
                controller.layers[0].stateMachine == null)
            {
                Debug.LogError("Animator controller geçersiz yapıya sahip!");
                return;
            }
            
            // Default state'i kontrol et ve yoksa ekle
            UnityEditor.Animations.AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            
            // Tüm state'leri kontrol et
            bool stateExists = false;
            foreach (var state in stateMachine.states)
            {
                if (state.state.motion == clip)
                {
                    stateExists = true;
                    break;
                }
            }
            
            // State yoksa ekle
            if (!stateExists)
            {
                UnityEditor.Animations.AnimatorState state = stateMachine.AddState("F16Fly");
                state.motion = clip;
                state.writeDefaultValues = true;
                stateMachine.defaultState = state;
                
                // Loop ayarını kontrol et
                MakeF16FlyAnimationLoop();
                
                Debug.Log("F16Animator'a F16Fly state eklendi ve default state olarak ayarlandı!");
            }
            else
            {
                Debug.Log("F16Fly state zaten controller'da mevcut.");
            }
            
            // Değişiklikleri kaydet
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogError("F16Animator controller bulunamadı: " + controllerPath);
        }
    }
#endif

    // Oyunda kullanımı (Test için)
    void Start()
    {
        // Player'ı otomatik bul
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player != null)
            {
                Debug.Log("Player referansı otomatik olarak bulundu.");
            }
        }
    }
} 