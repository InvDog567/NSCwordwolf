// Assets/kin/OpenAI/Scripts/NPCOverheardSubtitle.cs

using TMPro;
using UnityEngine;

public class NPCOverheardSubtitle : MonoBehaviour
{
    private const float DisplaySeconds = 4f;

    private TMP_Text _text;
    private float _hideTime;

    public static void Show(NPCMemory listener, string message)
    {
        if (listener == null)
            return;

        NPCOverheardSubtitle subtitle = listener.GetComponentInChildren<NPCOverheardSubtitle>(true);
        if (subtitle == null)
        {
            GameObject subtitleObject = new GameObject("Overheard Subtitle");
            subtitleObject.transform.SetParent(listener.transform, false);
            subtitleObject.transform.localPosition = Vector3.up * 2.4f;
            subtitleObject.transform.localScale = Vector3.one * 0.01f;
            subtitle = subtitleObject.AddComponent<NPCOverheardSubtitle>();
            subtitle._text = subtitleObject.AddComponent<TextMeshPro>();
            subtitle._text.font = TMP_Settings.defaultFontAsset;
            subtitle._text.fontSize = 34f;
            subtitle._text.alignment = TextAlignmentOptions.Center;
            subtitle._text.color = new Color(1f, 0.88f, 0.45f, 1f);
            subtitle._text.enableWordWrapping = true;
            subtitle._text.rectTransform.sizeDelta = new Vector2(420f, 140f);
        }

        subtitle._text.text = message;
        subtitle._hideTime = Time.time + DisplaySeconds;
        subtitle.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        if (Time.time >= _hideTime)
            gameObject.SetActive(false);
    }
}
