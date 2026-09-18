namespace rlmg.Tools.MediaPlayers
{
	using UnityEngine;
	using System;
	using UnityEngine.Video;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	
	/// <summary>
	/// Seeker/scrubber UI for video player
	/// </summary>
	[RequireComponent(typeof(Slider))]
	public class VideoPlayerUI_Seek : VideoPlayerUI_Base, IPointerDownHandler, IPointerUpHandler
	{
		public Action<float> OnDragged;

		private Slider slider;

		private bool _isPointerDown;
		private bool _isPlaying;

		private bool isMidSeek = false;

		/// <summary>
		/// how many frames after a seek before slider starts dynamically representing video position again?
		/// </summary>
		public int finishedSeekingGraceFrames = 10;
		private int finishedSeekingGraceFrameCount = 0;

		protected override void Start()
		{
			base.Start();

			player.seekCompleted += SeekCompletedCallback;

			slider = GetComponent<Slider>();

			OnDragged = new Action<float>((f) => 
				{
					if (!player.isPrepared)
					{
						Debug.LogWarning("Video not prepared.");
						return;
					}

					if (!player.canSetTime)
					{
						Debug.LogWarning("Video doesn't allow setting time.");
						return;
					}

					f = Mathf.Clamp(f, 0f, 1f);
					player.time = f * Duration;

					isMidSeek = true;
				});
		}

		void SeekCompletedCallback(VideoPlayer vp)
		{
			isMidSeek = false;

			finishedSeekingGraceFrameCount = finishedSeekingGraceFrames;
		}

		void Update()
		{
			if (slider != null && !_isPointerDown && !wasPointerUpThisFrame && !isMidSeek && finishedSeekingGraceFrameCount <= 0)
			{
				try
				{
					if (Duration > 0f)
					{
						slider.value = Mathf.Clamp01(System.Convert.ToSingle(player.time / Duration));
					}
					else
					{
						slider.value = 0f;
					}
				}
				catch (Exception)
				{
					Debug.Log("ERROR Converting double to float:");
				}
			}

			if (slider != null && _isPointerDown)
			{
				UpdateDrag();
			}

			wasPointerUpThisFrame = false;

			finishedSeekingGraceFrameCount--;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_isPointerDown = true;

			_isPlaying = player.isPlaying;

			// if (_isPlaying)
			// {
				// player.Pause();
			// }

			UpdateDrag();
		}

		private bool wasPointerUpThisFrame = false;

		public void OnPointerUp(PointerEventData eventData)
		{
			_isPointerDown = false;

			UpdateDrag();

			// if (_isPlaying)
			// { 
				// player.Play();
			// }

			wasPointerUpThisFrame = true;
		}

		private void UpdateDrag()
		{
			if (slider)
			{
				if (OnDragged != null)
				{
					OnDragged(slider.value);
				}
			}
		}
	}
}
