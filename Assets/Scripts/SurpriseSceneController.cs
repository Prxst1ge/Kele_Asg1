/*
 * Author: Javier
 * Date: 13 December 2025
 * Description:
 * Controls the final surprise / reward scene shown after the player
 * completes the full KELE AR experience. Manages UI sequencing,
 * special card reveal, 3D model animation, and navigation buttons.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the congratulatory surprise scene shown at the end of the experience.
/// Handles timed UI reveals, special card animation, and floating 3D model display.
/// </summary>
public class SurpriseSceneController : MonoBehaviour
{
    [Header("UI")]

    /// <summary>
    /// Panel displaying the congratulatory message.
    /// </summary>
    [SerializeField] private GameObject congratsPanel;

    /// <summary>
    /// Main title text (e.g. "Congratulations!").
    /// </summary>
    [SerializeField] private TMP_Text titleText;

    /// <summary>
    /// Subtitle text describing completion of the experience.
    /// </summary>
    [SerializeField] private TMP_Text subtitleText;

    /// <summary>
    /// Panel containing the special reward card UI.
    /// </summary>
    [SerializeField] private GameObject cardPanel;

    /// <summary>
    /// RectTransform of the special card used for pop-in animation.
    /// </summary>
    [SerializeField] private RectTransform specialCardRect;

    [Header("3D Model")]

    /// <summary>
    /// Root GameObject that contains the 3D reward model.
    /// </summary>
    [SerializeField] private GameObject modelRoot;

    /// <summary>
    /// Transform of the actual 3D model to animate.
    /// </summary>
    [SerializeField] private Transform modelTransform;

    /// <summary>
    /// Rotation speed of the 3D model (degrees per second).
    /// </summary>
    [SerializeField] private float modelRotateSpeed = 45f;

    /// <summary>
    /// Vertical floating amplitude of the 3D model.
    /// </summary>
    [SerializeField] private float floatAmplitude = 0.05f;

    /// <summary>
    /// Speed of the vertical floating motion.
    /// </summary>
    [SerializeField] private float floatSpeed = 1.5f;

    [Header("Buttons")]

    /// <summary>
    /// Panel containing navigation buttons (e.g. Return to Menu).
    /// </summary>
    [SerializeField] private GameObject buttonsPanel;

    [Header("Sequence Timings")]

    /// <summary>
    /// Delay before the special card is revealed.
    /// </summary>
    [SerializeField] private float delayBeforeCard = 0.8f;

    /// <summary>
    /// Duration of the card pop-in animation.
    /// </summary>
    [SerializeField] private float cardPopDuration = 0.25f;

    /// <summary>
    /// Delay before revealing the 3D model.
    /// </summary>
    [SerializeField] private float delayBeforeModel = 0.6f;

    /// <summary>
    /// Initial world position of the 3D model.
    /// Used for floating animation.
    /// </summary>
    private Vector3 modelStartPos;

    /// <summary>
    /// Determines whether the 3D model animation is active.
    /// </summary>
    private bool animateModel;

    /// <summary>
    /// Initializes UI and model states, then starts the reveal sequence.
    /// </summary>
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

    /// <summary>
    /// Plays the full reveal sequence:
    /// card pop-in, model reveal, then button display.
    /// </summary>
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

    /// <summary>
    /// Performs a smooth pop-in scale animation with a slight overshoot.
    /// </summary>
    /// <param name="target">RectTransform to animate.</param>
    /// <param name="duration">Duration of the pop animation.</param>
    IEnumerator PopScale(RectTransform target, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

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

    /// <summary>
    /// Handles continuous rotation and floating animation of the 3D model.
    /// </summary>
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
