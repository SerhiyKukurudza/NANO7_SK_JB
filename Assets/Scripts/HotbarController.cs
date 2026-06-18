using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private Image[] hotbarSlotImages = new Image[3];
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.2f);
    
    private WeaponManager weaponManager;
    private int currentSelectedSlot = -1;

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager not found in scene!");
            return;
        }

        weaponManager.OnWeaponChanged += HandleWeaponChanged;
        
        // Initialize slot colors based on locked/unlocked status.
        for (int i = 0; i < hotbarSlotImages.Length; i++)
        {
            if (hotbarSlotImages[i] != null)
            {
                hotbarSlotImages[i].color = weaponManager.HasWeaponUnlocked(i) ? unselectedColor : lockedColor;
            }
        }
    }

    void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= HandleWeaponChanged;
        }
    }

    void Update()
    {
        // Select weapon with number keys (1, 2, 3)
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectSlot(2);
        }
    }

    private void SelectSlot(int slotIndex)
    {
        WeaponData weapon = weaponManager.GetWeaponInSlot(slotIndex);
        
        if (weapon == null)
        {
            Debug.LogWarning("Slot " + slotIndex + " is empty or locked!");
            return;
        }
        
        // Deselect previous slot
        if (currentSelectedSlot >= 0 && hotbarSlotImages[currentSelectedSlot] != null)
        {
            hotbarSlotImages[currentSelectedSlot].color = unselectedColor;
        }
        
        // Select new slot
        currentSelectedSlot = slotIndex;
        if (hotbarSlotImages[slotIndex] != null)
        {
            hotbarSlotImages[slotIndex].color = selectedColor;
        }
        
        weaponManager.SelectWeapon(slotIndex);
        Debug.Log("Selected weapon: " + weapon.weaponName);
    }

    private void HandleWeaponChanged(WeaponData newWeapon, int slotIndex)
    {
        for (int i = 0; i < hotbarSlotImages.Length; i++)
        {
            if (hotbarSlotImages[i] == null)
            {
                continue;
            }

            WeaponData weaponInSlot = weaponManager.GetWeaponInSlot(i);
            if (weaponInSlot == null)
            {
                hotbarSlotImages[i].color = lockedColor;
                continue;
            }

            hotbarSlotImages[i].color = i == slotIndex ? selectedColor : unselectedColor;
        }

        currentSelectedSlot = slotIndex;
    }

    public int GetSelectedSlot()
    {
        return currentSelectedSlot;
    }
}
