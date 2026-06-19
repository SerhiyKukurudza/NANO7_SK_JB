using UnityEngine;

[DisallowMultipleComponent]
public class WeaponPickupDisplay : MonoBehaviour
{
    [SerializeField] private int weaponIndex;
    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int orderInLayer = 50;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private bool alreadyPickedUp = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = orderInLayer;

        if (weaponSprite == null)
        {
            WeaponPickup pickup = GetComponent<WeaponPickup>();
            if (pickup == null)
            {
                pickup = GetComponentInParent<WeaponPickup>();
            }

            if (pickup == null)
            {
                pickup = GetComponentInChildren<WeaponPickup>();
            }

            if (pickup == null)
            {
                WeaponPickup[] pickups = transform.root.GetComponentsInChildren<WeaponPickup>(true);
                if (pickups.Length > 0)
                {
                    pickup = pickups[0];
                }
            }

            WeaponManager manager = FindFirstObjectByType<WeaponManager>();
            if (pickup != null && manager != null)
            {
                WeaponData data = manager.GetWeaponData(pickup.GetWeaponIndex());
                if (data != null)
                {
                    weaponSprite = data.weaponSprite;
                    weaponIndex = pickup.GetWeaponIndex();
                }
            }
            else if (manager != null)
            {
                WeaponData data = manager.GetWeaponData(weaponIndex);
                if (data != null)
                {
                    weaponSprite = data.weaponSprite;
                }
            }
        }
        
        if (weaponSprite != null)
        {
            spriteRenderer.sprite = weaponSprite;
            spriteRenderer.color = Color.white;
        }
        else
        {
            spriteRenderer.sprite = null;
        }
        
        startPosition = transform.position;
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }
    }

    void Update()
    {
        if (!alreadyPickedUp)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
    public void OnPickedUp()
    {
        alreadyPickedUp = true;
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        StartCoroutine(FadeOutAndDestroy());
    }

    public int GetWeaponIndex()
    {
        return weaponIndex;
    }
    
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float elapsedTime = 0f;
        float fadeDuration = 0.3f;
        Color originalColor = spriteRenderer.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
