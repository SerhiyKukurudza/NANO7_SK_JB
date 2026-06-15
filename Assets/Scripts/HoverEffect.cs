using UnityEngine;
using UnityEngine.UI;

public class HoverEffect : MonoBehaviour
{
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
}