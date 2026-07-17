using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoStreamingAssetURLHelper : MonoBehaviour
{
	public VideoPlayer videoPlayer;
	public string videoPath;

	void Awake()
	{
		if (videoPlayer == null)
		{
			videoPlayer = GetComponent<VideoPlayer>();
		}

		if (videoPlayer != null)
		{
			videoPlayer.url = Application.streamingAssetsPath + "/" + videoPath;
		}
	}
}
