using UnityEngine;
using TMPro;

public class StopwatchUI : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;

    float elapsed;
    bool running;

    public void StartTimer()
    {
        elapsed = 0f;
        running = true;
        UpdateText();
    }

    public void StopTimer()
    {
        running = false;
        UpdateText();
    }

    void Update()
    {
        if (!running) return;

        elapsed += Time.unscaledDeltaTime;
        UpdateText();
    }

    void UpdateText()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsed);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
