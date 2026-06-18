using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public Sprite weaponSprite;
    public int damage;
    public float fireRate;
    public float range;
    public int levelRequired;
}

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponData[] allWeapons = new WeaponData[3]; // Pistol, Rifle, Shotgun
    [SerializeField] private Transform[] hotbarSlots = new Transform[3]; // 3 slots na hotbar
    [SerializeField, Range(0.2f, 0.45f)] private float hotbarIconInset = 0.28f;
    
    private WeaponData[] unlockedWeapons = new WeaponData[3];
    private int currentWeaponIndex = -1;
    private int currentLevel = 1;
    
    private static WeaponManager instance;
    
    // Event system for weapon changes
    public delegate void WeaponChangeDelegate(WeaponData newWeapon, int slotIndex);
    public event WeaponChangeDelegate OnWeaponChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize empty hotbar - don't unlock anything by default
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            unlockedWeapons[i] = null;
            SetSlotLockedVisual(i);
        }
        
        // Hotbar starts completely empty
        Debug.Log("Weapon system initialized - hotbar is empty");
    }

    public void PickupWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length)
        {
            Debug.LogError("Invalid weapon index: " + weaponIndex);
            return;
        }

        if (allWeapons[weaponIndex] == null)
        {
            Debug.LogError("Weapon data missing at index: " + weaponIndex);
            return;
        }
        
        UnlockWeapon(weaponIndex);
    }

    private void UnlockWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= unlockedWeapons.Length)
        {
            Debug.LogError("Weapon index out of unlocked array range: " + weaponIndex);
            return;
        }

        if (weaponIndex >= hotbarSlots.Length)
        {
            Debug.LogError("No matching hotbar slot for weapon index: " + weaponIndex);
            return;
        }

        // Do not unlock the same weapon twice.
        if (unlockedWeapons[weaponIndex] != null)
        {
            Debug.Log("Weapon already unlocked: " + allWeapons[weaponIndex].weaponName);
            return;
        }

        // Weapon index maps directly to its slot index.
        unlockedWeapons[weaponIndex] = allWeapons[weaponIndex];
        UpdateHotbarDisplay(weaponIndex);

        // Auto-select the newly picked weapon so the animator updates immediately.
        SelectWeapon(weaponIndex);

        Debug.Log("Weapon unlocked in slot " + weaponIndex + ": " + allWeapons[weaponIndex].weaponName);
        Debug.Log($"[WeaponManager] Current state - currentWeaponIndex: {currentWeaponIndex}, unlockedWeapons[{weaponIndex}]: {(unlockedWeapons[weaponIndex] != null ? unlockedWeapons[weaponIndex].weaponName : "null")}");
    }

    public void SelectWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= unlockedWeapons.Length)
        {
            Debug.LogError("Invalid slot index: " + slotIndex);
            return;
        }
        
        if (unlockedWeapons[slotIndex] == null)
        {
            Debug.LogWarning("Slot is empty!");
            return;
        }
        
        currentWeaponIndex = slotIndex;
        Debug.Log("Weapon selected: " + unlockedWeapons[slotIndex].weaponName);
        
        // Trigger weapon change event
        if (OnWeaponChanged != null)
        {
            OnWeaponChanged(unlockedWeapons[slotIndex], slotIndex);
            Debug.Log($"[WeaponManager] OnWeaponChanged event fired - slot {slotIndex}");
        }
        else
        {
            Debug.LogError("[WeaponManager] OnWeaponChanged delegate is NULL!");
        }
    }

    private void UpdateHotbarDisplay(int slotIndex)
    {
        if (hotbarSlots[slotIndex] == null)
        {
            Debug.LogError("Hotbar slot not assigned: " + slotIndex);
            return;
        }

        if (unlockedWeapons[slotIndex] != null)
        {
            Image iconImage = GetOrCreateSlotIconImage(hotbarSlots[slotIndex]);
            if (iconImage != null)
            {
                iconImage.sprite = unlockedWeapons[slotIndex].weaponSprite;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }
        }
    }

    private void SetSlotLockedVisual(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSlots.Length)
        {
            return;
        }

        Transform slotTransform = hotbarSlots[slotIndex];
        if (slotTransform == null)
        {
            return;
        }

        Image iconImage = GetOrCreateSlotIconImage(slotTransform);
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.color = Color.clear;
        }
    }

    private Image GetOrCreateSlotIconImage(Transform slotTransform)
    {
        Transform iconTransform = slotTransform.Find("WeaponIcon");
        Image iconImage;

        if (iconTransform == null)
        {
            GameObject iconObject = new GameObject("WeaponIcon", typeof(RectTransform), typeof(Image));
            iconTransform = iconObject.transform;
            iconTransform.SetParent(slotTransform, false);

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            ApplyIconRect(rect);

            iconImage = iconObject.GetComponent<Image>();
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
        }
        else
        {
            iconImage = iconTransform.GetComponent<Image>();
            RectTransform rect = iconTransform.GetComponent<RectTransform>();
            if (rect != null)
            {
                ApplyIconRect(rect);
            }
        }

        return iconImage;
    }

    private void ApplyIconRect(RectTransform rect)
    {
        float inset = Mathf.Clamp(hotbarIconInset, 0.2f, 0.45f);
        rect.anchorMin = new Vector2(inset, inset);
        rect.anchorMax = new Vector2(1f - inset, 1f - inset);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public bool HasWeaponUnlocked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= unlockedWeapons.Length)
        {
            return false;
        }

        return unlockedWeapons[slotIndex] != null;
    }

    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    public WeaponData GetWeaponData(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length)
        {
            return null;
        }

        return allWeapons[weaponIndex];
    }

    public WeaponData GetCurrentWeapon()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < unlockedWeapons.Length)
        {
            return unlockedWeapons[currentWeaponIndex];
        }
        return null;
    }

    public WeaponData GetWeaponInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
        {
            return unlockedWeapons[slotIndex];
        }
        return null;
    }

    public int GetCurrentSlot()
    {
        return currentWeaponIndex;
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
    }
}
