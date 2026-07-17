using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerUI_PlayPause : VideoPlayerUI_Base
{
	public Button button;
	public Image buttonIconImage;

    public Sprite playIcon;
    public Sprite replayIcon;
	public Sprite pauseIcon;

    public bool allowPausing = true;

	protected override void Start()
	{
		base.Start();

		if (button == null)
			button = GetComponent<Button>();

		if (button != null)
		{
			if (buttonIconImage == null)
				buttonIconImage = button.GetComponentInChildren<Image>();

			button.onClick.AddListener(() => OnClick());
		}
	}

	private void OnClick()
	{
		if (player == null)
			return;

        if (player.isPlaying)
        {
            player.Pause();
        }
        else
        {
            //VideoPlayerManager playerManager = player.GetComponent<VideoPlayerManager>();
            //if (playerManager != null)
            //{
            //    playerManager.Play();
            //}
            //else
            //{
                player.Play();
            //}
        }
	}

	private void Update()
	{
		if (player == null || buttonIconImage == null)
			return;

        if (player.isPlaying)
        {
            buttonIconImage.sprite = pauseIcon;

            button.interactable = allowPausing;
        }
        else
        {
            //Debug.Log("video complete.  player.time=" + player.time + "  Duration=" + Duration);

            //if (Mathf.Approximately((float)player.time, Duration) && replayIcon != null)
            if (Mathf.Abs(Duration - (float)player.time) < 0.1f && replayIcon != null)
                buttonIconImage.sprite = replayIcon;
            else
                buttonIconImage.sprite = playIcon;

            button.interactable = true;
        }
	}
}
