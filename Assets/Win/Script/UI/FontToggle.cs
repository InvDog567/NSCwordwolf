using TMPro;
using UnityEngine;

public class FontToggle : MonoBehaviour
{
    public TMP_Text targetText;
    public TMP_FontAsset fontA;
    public TMP_FontAsset fontB;

    private bool usingFontA = true;

    void Start()
    {
        if (targetText != null && fontA != null)
        {
            targetText.font = fontA;
        }
    }

    public void ToggleFont()
    {
        if (targetText == null || fontA == null || fontB == null)
            return;

        usingFontA = !usingFontA;
        targetText.font = usingFontA ? fontA : fontB;
    }
}