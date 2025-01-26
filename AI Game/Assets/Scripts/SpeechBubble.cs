using TMPro;
using UnityEngine;

[ExecuteAlways]
public class SpeechBubble : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    SpriteRenderer background;
    [SerializeField]
    float scrollSpeed = 1.0f;
    [SerializeField]
    Agent agent;
    [SerializeField]
    AudioClip[] plops;
    [SerializeField]
    AudioClip[] clicks;
    [SerializeField]
    AudioSource plopAudio;
    [SerializeField]
    BoxCollider2D hoverCollider;

    public string targetText = "Hello World!";
    string cachedTargetText = "";
    float targetTextTimestamp = 0;
    float width;
    float height;
    int cachedScrollPos;

    private void Awake()
    {
        TextChanged();
        agent.OnAgentSaysSomething += SetText;
    }

    private void OnMouseOver()
    {
        GameManager.instance.HighlightChatBubble(this);
    }

    public void SetRenderingLayer(int layer)
    {
        background.sortingOrder = layer;
        canvas.sortingOrder = layer + 1;
    }

    void SetText(string input)
    {
        bool valid = !string.IsNullOrEmpty(input);

        targetText = input;
        targetTextTimestamp = Time.time;
        cachedScrollPos = 0;
        text.text = string.Empty;

        gameObject.SetActive(valid);
        if (valid)
        {
            plopAudio.pitch = 1.0f;
            plopAudio.volume = 1.0f;
            plopAudio.PlayOneShot(plops[Random.Range(0, plops.Length)]);
        }
    }

    private void Update()
    {
        //editor only
        if (cachedTargetText != targetText)
        {
            //SetText(targetText);
        }

        if (text.text != targetText)
        {
            TextChanged();
        }
    }

    void SizeChanged()
    {
        background.size = new Vector2(width, height);
        Vector2 offset = new Vector2(width * 0.5f, height * 0.5f);
        background.transform.localPosition = offset;
        
        hoverCollider.size = background.size;
        hoverCollider.offset = offset;

        canvas.GetComponent<RectTransform>().sizeDelta = background.size;
    }

    void TextChanged()
    {
        int scrollPos = Mathf.CeilToInt((Time.time - targetTextTimestamp) * scrollSpeed);
        text.text = targetText.Substring(0, Mathf.Min(targetText.Length, scrollPos));

        if (scrollPos != cachedScrollPos && scrollPos % 5 == 0)
        {
            cachedScrollPos = scrollPos;
            plopAudio.pitch = Random.Range(0.5f, 1.5f);
            plopAudio.volume = 0.5f;
            plopAudio.PlayOneShot(clicks[Random.Range(0, clicks.Length)]);
        }

        const float padding2 = 0.25f * 2;
        width = Mathf.Min(5, Mathf.Ceil(text.preferredWidth + padding2));
        height = Mathf.Ceil(text.preferredHeight + padding2);

        SizeChanged();
    }
}
