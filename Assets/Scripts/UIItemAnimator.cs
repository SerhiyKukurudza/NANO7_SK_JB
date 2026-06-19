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
            enabled = false;
            return;
        }
        
        if (animationFrames == null || animationFrames.Length == 0)
        {
            enabled = false;
            return;
        }
        
        itemImage.sprite = animationFrames[0];
    }

    void Update()
    {
        if (animationFrames == null || animationFrames.Length == 0)
            return;
        
        frameTimer += Time.deltaTime;
        
        float frameDuration = 1f / frameRate;
        
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame = (currentFrame + 1) % animationFrames.Length;
            itemImage.sprite = animationFrames[currentFrame];
        }
    }
}
