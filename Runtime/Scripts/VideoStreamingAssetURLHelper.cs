namespace rlmg.Tools.MediaPlayers
{
	using UnityEngine;
	using UnityEngine.Video;
	using System.IO;

	/// <summary>
	/// A simple little hack to support a local path within StreamingAssets, as the built-in Video Player component requires a full path.
	/// </summary>
	public class VideoStreamingAssetURLHelper : MonoBehaviour
	{
		[SerializeField]
		private VideoPlayer videoPlayer;

		[SerializeField]
		private string videoPath;

		void Awake()
		{
			if (videoPlayer == null)
			{
				videoPlayer = GetComponent<VideoPlayer>();
			}

			if (videoPlayer != null)
			{
				videoPlayer.url = Path.Combine(Application.streamingAssetsPath, videoPath);
			}
		}
	}
}
