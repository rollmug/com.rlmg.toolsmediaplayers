using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerUI_Viewport : VideoPlayerUI_Base
{
	private RawImage viewportImage;

	protected override void Start()
	{
		base.Start();

		viewportImage = GetComponent<RawImage>();

		if (viewportImage != null && player != null && player.targetTexture != null)
		{
			//todo: generate render texture at same dimensions as video if one doesn't already exist

			viewportImage.texture = player.targetTexture;
		}
	}
}
