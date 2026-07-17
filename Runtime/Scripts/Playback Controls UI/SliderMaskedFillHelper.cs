using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SliderMaskedFillHelper : MonoBehaviour
{
	public RectTransform rectToMatch;

	private RectTransform _myRectTransform;
	private RectTransform MyRectTransform
	{
		get
		{
			if (_myRectTransform == null)
				_myRectTransform = GetComponent<RectTransform>();

			return _myRectTransform;
		}
	}

//	private void Start()
//	{
//		UpdateRect();
//	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			UpdateRect();
		}
	}

	private void UpdateRect()
	{
		if (rectToMatch != null && MyRectTransform != null)
		{
			MyRectTransform.anchorMin = new Vector2(0f, 0.5f);
			MyRectTransform.anchorMax = new Vector2(0f, 0.5f);
			MyRectTransform.pivot = new Vector2(0f, 0.5f);

			MyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectToMatch.rect.width);
			MyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectToMatch.rect.height);

			MyRectTransform.anchoredPosition = rectToMatch.anchoredPosition;
		}
	}
}
