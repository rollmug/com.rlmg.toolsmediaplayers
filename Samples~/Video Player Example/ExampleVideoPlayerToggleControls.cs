namespace rlmg.Tools.MediaPlayers.Examples
{
    using UnityEngine;

    public class ExampleVideoPlayerToggleControls : MonoBehaviour
    {
        [SerializeField]
        GameObject scrubber, bottomBar;
        public void OnClick()
        {
            if (scrubber.activeSelf)
            {
                scrubber.SetActive(false);
                bottomBar.SetActive(true);
            }
            else
            {
                scrubber.SetActive(true);
                bottomBar.SetActive(false);
            }
        }
    }

}