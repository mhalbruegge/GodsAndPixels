using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Extensions;

public class SacrificeUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    GameObject effectPrefab;
    [SerializeField]
    float effectDuration = 1.0f;
    [SerializeField]
    AudioSource effectSource;
    [SerializeField]
    AudioClip[] clips;
    [SerializeField]
    AudioClip pointClip;

    private void Start()
    {
        GameManager.instance.onSacrificeMade += OnSacrificeMade;
        text.text = "0";
    }

    private void OnSacrificeMade(GameManager.SacrificeType type, int amount, int points, Agent agent)
    {
        try
        {

            Vector3 screenPos = Camera.main.WorldToScreenPoint(agent.transform.position);
            GameObject instance = Instantiate(effectPrefab, screenPos, Quaternion.identity, transform);
            instance.GetComponentInChildren<Image>().sprite = agent.inventoryIcon.sprite;
            instance.GetComponentInChildren<TextMeshProUGUI>().text = amount.ToString();

            StartCoroutine(EffectAnimation(instance, Time.time));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    IEnumerator EffectAnimation(GameObject instance, float effectTime)
    {
        effectSource.pitch = 1.0f;
        effectSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)], 0.2f);
        Vector3 startPos = instance.transform.position;

        while (Time.time - effectTime < effectDuration)
        {
            float delta = (Time.time - effectTime) / effectDuration;
            instance.transform.position = Vector3.Lerp(startPos, transform.position, delta);
            yield return null;
        }
        Destroy(instance);

        text.text = GameManager.instance.sacrificePoints.ToString();
        effectSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        effectSource.PlayOneShot(pointClip, 2.0f);
    }
}
