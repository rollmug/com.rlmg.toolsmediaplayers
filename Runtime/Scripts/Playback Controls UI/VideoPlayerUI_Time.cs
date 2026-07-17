using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoPlayerUI_Time : VideoPlayerUI_Base
{
	public Action<double,double> OnUpdateTime;

	[HideInInspector]
	private string timeText = "{0}:{1:00} | {2}:{3:00}";

	private Text _text;
	private StringBuilder _timeSB;
	private int minDur, minPos, secDur, secPos;

	protected override void Start()
	{
		base.Start();

		_text = GetComponent<Text>();
		_timeSB  = new StringBuilder();
		if (_text != null)
		{
			OnUpdateTime = new Action<double, double>((pos, dur) => 
			{
				_timeSB.Length = 0;
				_timeSB.Capacity = 0;

				try
				{
					minPos = (int)Mathf.Floor(Convert.ToInt32(pos) / 60f);
					secPos = (int)(Convert.ToInt32(pos) - (minPos * 60));

					minDur = (int)Mathf.Floor(Convert.ToInt32(dur) / 60f);
					secDur = (int)(Convert.ToInt32(dur) - (minDur * 60));
				}
				catch (Exception)
				{

				};

				_timeSB.AppendFormat(timeText, minPos, secPos, minDur, secDur);
				_text.text = _timeSB.ToString();
			});
		}
	}

	void Update()
	{
		if (OnUpdateTime != null)
		{
//			Debug.Log("player.clip.length = "+(player.clip == null ? 0f : player.clip.length));
//			Debug.Log("Duration = "+Duration);

			OnUpdateTime(player.time, Duration);
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
	}
}
