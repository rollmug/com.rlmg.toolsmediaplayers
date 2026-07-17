using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VideoPlayerUI_Seek : VideoPlayerUI_Base, IPointerDownHandler, IPointerUpHandler
{
	public Action<float> OnDragged;

	private Slider _slider;
    public Slider slider {  get { return _slider; } }

	private bool _isPointerDown;
	private bool _isPlaying;

    private bool isMidSeek = false;
    //private bool didFinishSeekingThisFrame = false;
    public int finishedSeekingGraceFrames = 10;
    private int finishedSeekingGraceFrameCount = 0;

	protected override void Start()
	{
		base.Start();

		player.seekCompleted += SeekCompletedCallback;

		_slider = GetComponent<Slider>();
		//OnDragged = new Action<float>((f) => { player.SeekTo(f,true); });
		OnDragged = new Action<float>((f) => 
			{
				if (!player.isPrepared)
				{
					Debug.Log("Video not prepared.");
					return;
				}

				if (!player.canSetTime)
				{
					Debug.Log("Video doesn't allow setting time.");
					return;
				}

				f = Mathf.Clamp(f, 0f, 1f);
				player.time = f * Duration;
				Debug.Log("seek to "+player.time);
                //player.Prepare();

                isMidSeek = true;
            });
	}

	void SeekCompletedCallback(VideoPlayer vp)
	{
		Debug.Log("seek completed");

        isMidSeek = false;
        //didFinishSeekingThisFrame = true;
        finishedSeekingGraceFrameCount = finishedSeekingGraceFrames;
    }

	void Update()
	{
        //if (_slider && !_isPointerDown)
        //if (_slider && !_isPointerDown && !isMidSeek && !wasPointerUpThisFrame && !didFinishSeekingThisFrame && player.isPlaying)
        if (_slider && !_isPointerDown && !wasPointerUpThisFrame && !isMidSeek && finishedSeekingGraceFrameCount <= 0)
		{                     
			try
			{
				//_slider.value = Mathf.Clamp01(System.Convert.ToSingle(player.GetCurrentPosition(true)));

//				Debug.Log("Duration = "+Duration);

				if (Duration > 0f)
					_slider.value = Mathf.Clamp01(System.Convert.ToSingle(player.time / Duration));
				else
					_slider.value = 0f;
			}
			catch (Exception)
			{
				Debug.Log("ERROR Converting double to float:");
			}

			//Debug.Log("GetCurrentPosition:" + _slider.value);
		}
		if (_slider && _isPointerDown)
		{
			_UpdateDrag();
		}

        //Deselect the current GUI element to display the right color state
        //		if (_eventSystem)
        //		{  
        //			if (_eventSystem.currentSelectedGameObject == gameObject)
        //			{
        //				//Debug.Log("RESET");
        //				_eventSystem.SetSelectedGameObject(null);
        //			}
        //
        //		}

        wasPointerUpThisFrame = false;
        //didFinishSeekingThisFrame = false;
        finishedSeekingGraceFrameCount--;
    }



	public void OnPointerDown(PointerEventData eventData)
	{
		// Debug.Log("OnPointerDown:");
		_isPointerDown = true;

		_isPlaying = player.isPlaying;

		if (_isPlaying)
		{
//			player.Pause();
		}

		_UpdateDrag();
	}

    private bool wasPointerUpThisFrame = false;

	public void OnPointerUp(PointerEventData eventData)
	{
		// Debug.Log("OnPointerUp:"+ eventData);
		_isPointerDown = false;

		_UpdateDrag();

		if (_isPlaying)
		{ 
//			player.Play();
		}

        wasPointerUpThisFrame = true;
    }

	private void _UpdateDrag()
	{
		if (_slider)
		{
			// Debug.Log("_Seek:" + _slider.value);
			if (OnDragged != null)
				OnDragged(_slider.value);
		}
	}
}
