using Managers;
using UnityEngine;
using UnityEngine.UI;

public class VideoControls : MonoBehaviour
{
    [SerializeField] private GameObject controlsCanvas;

    [SerializeField] private Image pauseImage;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    [SerializeField] private Image fastForwardImage;
    [SerializeField] private Color fastForwardDefaultColor;
    [SerializeField] private Color fastForwardHighlightColor;

    private bool paused = false;
    private bool fastForwarding = false;

    private void OnEnable()
    {
        pauseImage.sprite = pauseSprite;
        paused = false;
        fastForwardImage.color = fastForwardDefaultColor;
        fastForwarding = false;
    }

    public void Btn_FastForward()
    {
        if (VideoPlayerManager.Instance == null)
        {
            Debug.LogError("Tried to use video controls while video player manager doesn't exist");
        }

        if (fastForwarding)
        {
            VideoPlayerManager.Instance.SetPlaybackSpeed(1.0f);
            fastForwardImage.color = fastForwardDefaultColor;
        }
        else
        {
            VideoPlayerManager.Instance.SetPlaybackSpeed(2.0f);
            fastForwardImage.color = fastForwardHighlightColor;
        }
        fastForwarding = !fastForwarding;
    }

    public void Btn_Rewind()
    {
        if (VideoPlayerManager.Instance == null)
        {
            Debug.LogError("Tried to use video controls while video player manager doesn't exist");
        }

        VideoPlayerManager.Instance.Rewind(5.0f);
    }

    public void Btn_PlayPause()
    {
        if (VideoPlayerManager.Instance == null)
        {
            Debug.LogError("Tried to use video controls while video player manager doesn't exist");
        }

        paused = !paused;
        if (paused)
        {
            pauseImage.sprite = playSprite;
        }
        else
        {
            pauseImage.sprite = pauseSprite;
        }

        VideoPlayerManager.Instance.SetPause(paused);
    }

    public void Btn_Exit()
    {
        if (VideoPlayerManager.Instance == null)
        {
            Debug.LogError("Tried to use video controls while video player manager doesn't exist");
        }

        VideoPlayerManager.Instance.StopVideo();
    }
}
