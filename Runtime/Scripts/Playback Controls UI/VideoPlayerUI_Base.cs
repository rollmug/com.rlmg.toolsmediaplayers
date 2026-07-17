using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoPlayerUI_Base : MonoBehaviour
{
	public VideoPlayer player;

	protected virtual void Start()
	{
        if (player == null)
            player = GetComponentInChildren<VideoPlayer>();

        if (player == null)
            player = GetComponentInParent<VideoPlayer>();

        if (player == null)
			player = (VideoPlayer)FindObjectOfType(typeof(VideoPlayer));
	}

	protected float Duration
	{
		get
		{
			if (player == null)
				return 0f;

			if (player.frameRate >= 0f)
				return (float)(player.frameCount / player.frameRate);
			else
				return 0f;
		}
	}
}
