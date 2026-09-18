namespace rlmg.Tools.MediaPlayers
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// seeker/scrubber UI for image sequence player
	/// </summary>
	[RequireComponent(typeof(Slider))]
	public class ImageSequenceScrubber : MonoBehaviour
	{
		/// <summary>
		/// the ImageSequencePlayer this scrubber affects
		/// </summary>
		[SerializeField]
		private ImageSequencePlayer imageSequence;

		private Slider slider;

		private bool useSliderAsInput = true;

		void Start()
		{
			if (imageSequence == null)
			{
				imageSequence = FindAnyObjectByType<ImageSequencePlayer>();
			}

			slider = GetComponent<Slider>();

			if (slider != null)
			{
				slider.onValueChanged.AddListener((x) => { MovedSlider(x); });
			}
		}
		
		void Update()
		{
			UpdateSliderToMatchSequence();
		}

		private void UpdateSliderToMatchSequence()
		{
			if (imageSequence == null)
			{
				return;
			}

			useSliderAsInput = false;

			if (slider != null)
			{
				slider.value = imageSequence.Percent;
			}

			useSliderAsInput = true;
		}

		private void MovedSlider(float sliderValue)
		{
			if (!useSliderAsInput)
			{
				return;
			}

			if (imageSequence == null)
			{
				return;
			}

			imageSequence.Percent = sliderValue;
		}
	}
}
