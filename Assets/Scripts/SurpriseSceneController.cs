using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SurpriseSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject congratsPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [SerializeField] private GameObject cardPanel;
    [SerializeField] private RectTransform specialCardRect; // the Image's RectTransform

    [Header("3D Model")]
    [SerializeField] private GameObject modelRoot;          // parent object of the 3D model
    [SerializeField] private Transform modelTransform;      // actual model transform
    [SerializeField] private float modelRotateSpeed = 45f;
    [SerializeField] private float floatAmplitude = 0.05f;
    [SerializeField] private float floatSpeed = 1.5f;

    [Header("Buttons")]
    [SerializeField] private GameObject buttonsPanel;

    [Header("Sequence Timings")]
    [SerializeField] private float delayBeforeCard = 0.8f;
    [SerializeField] private float cardPopDuration = 0.25f;
    [SerializeField] private float delayBeforeModel = 0.6f;

    private Vector3 modelStartPos;
    private bool animateModel;

    void Start()
    {
        // Initial states
        if (buttonsPanel) buttonsPanel.SetActive(false);

        if (cardPanel) cardPanel.SetActive(false);
        if (modelRoot) modelRoot.SetActive(false);

        if (titleText) titleText.text = "Congratulations!";
        if (subtitleText) subtitleText.text = "You completed the KELE AR experience!";

        if (modelTransform != null)
            modelStartPos = modelTransform.position;

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 1) Congrats already visible
        yield return new WaitForSeconds(delayBeforeCard);

        // 2) Card popup
        if (cardPanel) cardPanel.SetActive(true);

        if (specialCardRect != null)
        {
            specialCardRect.localScale = Vector3.zero;
            yield return StartCoroutine(PopScale(specialCardRect, cardPopDuration));
        }

        yield return new WaitForSeconds(delayBeforeModel);

        // 3) Model reveal
        if (modelRoot) modelRoot.SetActive(true);
        animateModel = true;

        // 4) Show buttons
        if (buttonsPanel) buttonsPanel.SetActive(true);
    }

    IEnumerator PopScale(RectTransform target, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            // smooth pop (overshoot)
            float s = Mathf.SmoothStep(0f, 1.15f, p);
            target.localScale = Vector3.one * s;
            yield return null;
        }

        // settle back to 1
        float settle = 0.12f;
        float tt = 0f;
        Vector3 start = target.localScale;
        while (tt < settle)
        {
            tt += Time.deltaTime;
            float p = Mathf.Clamp01(tt / settle);
            target.localScale = Vector3.Lerp(start, Vector3.one, p);
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    void Update()
    {
        if (!animateModel || modelTransform == null) return;

        // rotate
        modelTransform.Rotate(0f, modelRotateSpeed * Time.deltaTime, 0f, Space.World);

        // float
        Vector3 pos = modelStartPos;
        pos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        modelTransform.position = pos;
    }
}
