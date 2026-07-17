namespace rlmg.Tools.MediaPlayers.Examples
{
    using System.IO;
    using UnityEngine;
    using rlmg.Tools.MediaPlayers;

    public class ExampleVideoPlayerLoading : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayerManager videoPlayerManager;

        [Tooltip("Assumed to be in StreamingAssets")]
        [SerializeField]
        private string videoFileSubPath;

        private string videoFileFullPath => Path.Join(
            Application.streamingAssetsPath,
            videoFileSubPath);

        private void Awake()
        {
            if (videoPlayerManager == null)
                return;

            videoPlayerManager.LoadVideo(videoFileFullPath);
        }
    }

}