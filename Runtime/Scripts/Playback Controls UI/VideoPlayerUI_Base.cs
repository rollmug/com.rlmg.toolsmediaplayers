namespace rlmg.Tools.MediaPlayers
{
	using UnityEngine;
	using UnityEngine.Video;
	
	/// <summary>
	/// Base class that other video player UI elements derive from.
	/// </summary>
	public class VideoPlayerUI_Base : MonoBehaviour
	{
		/// <summary>
		/// the built-in Unity VideoPlayer component at the core of everything
		/// </summary>
		[SerializeField]
		protected VideoPlayer player;

		protected virtual void Start()
		{
			if (player == null)
			{
				player = GetComponentInChildren<VideoPlayer>();
			}

			if (player == null)
			{
				player = GetComponentInParent<VideoPlayer>();
			}

			if (player == null)
			{
				player = (VideoPlayer)FindAnyObjectByType(typeof(VideoPlayer));
			}
		}

		protected float Duration
		{
			get
			{
				if (player == null)
				{
					return 0f;
				}

				if (player.frameRate >= 0f)
				{
					return (float)(player.frameCount / player.frameRate);
				}
				else
				{
					return 0f;
				}
			}
		}
	}
}
