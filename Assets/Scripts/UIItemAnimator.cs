using UnityEngine;
using UnityEngine.UI;

public class UIItemAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField] private float frameRate = 10f;
    
    private Image itemImage;
    private int currentFrame = 0;
    private float frameTimer = 0f;

    void Start()
    {
        itemImage = GetComponent<Image>();
        
        if (itemImage == null)
        {
            Debug.LogError("UIItemAnimator needs an Image component!");
            enabled = false;
            return;
        }
        
        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning("No animation frames assigned to UIItemAnimator on " + gameObject.name);
            enabled = false;
            return;
        }
        
        // Set initial sprite
        itemImage.sprite = animationFrames[0];
    }

    void Update()
    {
        if (animationFrames == null || animationFrames.Length == 0)
            return;
        
        // Update frame timer
        frameTimer += Time.deltaTime;
        
        // Calculate frame duration
        float frameDuration = 1f / frameRate;
        
        // Switch frame if timer exceeds duration
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame = (currentFrame + 1) % animationFrames.Length;
            itemImage.sprite = animationFrames[currentFrame];
        }
    }
}
