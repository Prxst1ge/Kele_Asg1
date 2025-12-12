using UnityEngine;

public class StageStartController : MonoBehaviour
{
    [SerializeField] GameObject stageIntroPanel;
    [SerializeField] StopwatchUI stopwatch;

    public void PressStart()
    {
        if (stageIntroPanel != null)
            stageIntroPanel.SetActive(false);

        if (stopwatch != null)
            stopwatch.StartTimer();
    }
}
