using UnityEngine;
using UnityEngine.UI;

public class ButtonEffects : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip clickSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;

        clickSound = Resources.Load<AudioClip>("Audio/sci-fi-ui-sfx/JDSherbert - Sci Fi UI SFX Pack - Cancel - 1");
    } 

    public void OnHoverEnterEffect(GameObject go)
    {
        Button button = go.GetComponentInParent<Button>();

        if (button == null || !button.interactable)
            return;

        button.transform.localScale = Vector3.one * 1.2f;
    }

    public void OnHoverExitEffect(GameObject go)
    {
        Button button = go.GetComponentInParent<Button>();

        if (button == null || !button.interactable)
            return;

        button.transform.localScale = Vector3.one;
    }

    public void OnClickEffect()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}