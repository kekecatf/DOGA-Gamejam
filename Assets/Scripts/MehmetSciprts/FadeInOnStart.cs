using UnityEngine;

public class FadeInOnStart : MonoBehaviour
{
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("FadeIn"); // Animator'da bir "FadeIn" trigger'ı varsa
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
