namespace rlmg.Tools.MediaPlayers
{
    using rlmg.Tools.ContentLoading;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// A loader to populate the frames of a single instance of ImageSequencePlayer.
    /// Features optional async loading.
    /// </summary>
    [RequireComponent(typeof(ImageSequencePlayer))]
    public class ImageSequenceLoader : MediaFileLoader
    {
        /// <summary>
        /// Attached player to load frames into
        /// </summary>
        protected ImageSequencePlayer player;

        /// <summary>
        /// Name of folder where images are stored
        /// For AssetBundles, StreamingAssets directory path will be prepended
        /// For Resources, Resources directory path will be prepended
        /// For StreamingAssets, StreamingAssets directory path will be prepended
        /// </summary>
        [SerializeField]
        protected string framesFolder;

        /// <summary>
        /// When true, frames loaded from StreamingAssets will be loaded using the async API instead of coroutines.
        /// </summary>
        [SerializeField]
        [Tooltip("When true, frames loaded from StreamingAssets will be loaded using the async API instead of coroutines.")]
        protected bool streamingAssetsUseAsync = false;

        /// <summary>
        /// For manual media loading Task cancellation
        /// Should be used whenever the loading coroutine is stopped.
        /// </summary>
        private CancellationTokenSource mediaLoadingCancellation;

        protected int nExpectedFrames;

        protected int nLoadedFrames;

        /// <summary>
        /// Progress of frames loaded
        /// </summary>
        public override float LoadingProgress
        {
            get
            {
                if (nExpectedFrames == 0)
                    return 0;

                return nLoadedFrames / nExpectedFrames;
            }
        }

        /// <summary>
        /// Path to frames folder
        /// </summary>
        protected override string moviePath
        {
            get
            {
                string path = Path.Combine(base.moviePath, framesFolder);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        protected override void Awake()
        {
            player = GetComponent<ImageSequencePlayer>();

            base.Awake();
        }

        protected virtual void OnDestroy()
        {
            CancelLoading();
        }

        protected override IEnumerator MainLoadMedia()
        {
            // Ensure any previous async loads are cancelled before starting a new one
            ResetCancellation();

            if (framesFolder == null)
            {
                LoadFailed?.Invoke(
                    "Frames cannot be loaded. Null frames folder.");
                yield break;
            }

            if (!Directory.Exists(moviePath))
            {
                LoadFailed?.Invoke(
                    string.Format( "Path to frames folder does not exist: {0}", moviePath));
                yield break;
            }

            switch (loadingMethod)
            {         
                case MediaLoadingMethod.AssetBundles:
                    yield return TryLoadingFromAssetBundle();
                    break;
                case MediaLoadingMethod.Resources:
                    yield return TryLoadingFromResources();
                    break;
                case MediaLoadingMethod.StreamingAssets:
                    if (streamingAssetsUseAsync)
                    {
                        mediaLoadingCancellation ??= new CancellationTokenSource();
                        Task loadTask = TryLoadingFromStreamingAssetsAsync(mediaLoadingCancellation.Token);
                        yield return MediaLoadingUtility.WaitForTask(loadTask);
                    }
                    else
                    {
                        yield return TryLoadingFromStreamingAssets();
                    }
                    break;
                case MediaLoadingMethod.None:
                default:
                    break;
            }

            DidLoadSucceed = nLoadedFrames == nExpectedFrames;

        }

        /// <summary>
        /// Tries to load images from AssetBundle at framesFolder path
        /// where framesFolder = path to AssetBundle itself
        /// </summary>
        protected virtual IEnumerator TryLoadingFromAssetBundle()
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(moviePath);

            if (myLoadedAssetBundle == null)
            {
                LoadFailed?.Invoke(
                    string.Format("Failed to load AssetBundle at path {0}!", moviePath));
                yield break;
            }

            player.frames = myLoadedAssetBundle.LoadAllAssets<Sprite>();

            if (player.frames.Length == 0)
            {
                LoadFailed?.Invoke(
                    string.Format("Loaded 0 frames from AssetBundle at path {0}", moviePath));
                yield break;
            }

            myLoadedAssetBundle.Unload(false);
        }

        /// <summary>
        /// Tries to load images from Resources folder at framesFolder path
        /// where framesFolder = subdirectory containing individual image files
        /// </summary>
        protected virtual IEnumerator TryLoadingFromResources()
        {
            player.frames = Resources.LoadAll<Sprite>(moviePath);
            yield break;
        }

        /// <summary>
        /// Via UnityWebRequest coroutines, tries to load images from StreamingAssets folder
        /// at framesFolder path
        /// where framesFolder = subdirectory containing individual image files
        /// </summary>
        protected virtual IEnumerator TryLoadingFromStreamingAssets()
        {
            DirectoryInfo dir = new DirectoryInfo(moviePath);

            FileInfo[] info = dir.GetFiles("*.*")
                .Where(file => MediaLoadingUtility.IsImageFile(file.Name))
                .ToArray();

            nExpectedFrames = info.Length;
            nLoadedFrames = 0;

            bool wasPlaying = player.VideoMoving;

            player.VideoMoving = false;

            player.frames = new Sprite[nExpectedFrames];

            for (int i = 0; i < info.Length; i++)
            {
                FileInfo file = info[i];

                string uri = Path.Combine(moviePath, file.Name);

                int index = i;

                yield return MediaLoadingUtility.LoadTextureCoroutine(
                    uri,
                    textureSetter: texture =>
                    {
                        // Give the texture a name so errors are clear about which texture failed.
                        texture.name = string.Format("Image Sequence Texture {0}", index);

                        Sprite sprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f)
                            );

                        // Give the sprite a name just for cross-referencing missing sprites in the Inspector
                        sprite.name = string.Format("Image Sequence Sprite {0}", index);

                        player.frames[index] = sprite;

                        nLoadedFrames++;
                    },
                    onError: (message, uri) =>
                    {
                        LoadFailed?.Invoke(
                            string.Format("Frames failed to load at {0}: {1}", uri, message));
                    });
            }

            player.VideoMoving = wasPlaying;
            
        }

        /// <summary>
        /// Helper method for cancelling the ongoing media loading Task
        /// and preparing for a new one
        /// This should be called to clean up any ongoing Tasks before starting new ones.
        /// </summary>
        private void ResetCancellation()
        {
            mediaLoadingCancellation?.Cancel();
            mediaLoadingCancellation?.Dispose();

            mediaLoadingCancellation = new CancellationTokenSource();
        }

        /// <summary>
        /// Stop asynchronous (Tasks and Coroutines) loading
        /// </summary>
        public void CancelLoading()
        {
            // Clean up the media loading Coroutine managed by this MonoBehaviour
            StopAllCoroutines();

            // Clean up the media loading Tasks managed by this MonoBehaviour
            mediaLoadingCancellation?.Cancel();
            mediaLoadingCancellation?.Dispose();
            mediaLoadingCancellation = null;
        }

        /// <summary>
        /// Async variant for loading images from StreamingAssets folder using the async API.
        /// This can be awaited via MediaLoadingUtility.WaitForTask when called from a coroutine.
        /// </summary>
        protected virtual async Task TryLoadingFromStreamingAssetsAsync(CancellationToken cancellationToken)
        {
            DirectoryInfo dir = new DirectoryInfo(moviePath);

            FileInfo[] info = dir.GetFiles("*.*")
                .Where(file => MediaLoadingUtility.IsImageFile(file.Name))
                .ToArray();

            nExpectedFrames = info.Length;
            nLoadedFrames = 0;

            bool wasPlaying = player.VideoMoving;

            player.VideoMoving = false;

            player.frames = new Sprite[nExpectedFrames];

            for (int i = 0; i < info.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                FileInfo file = info[i];

                string uri = Path.Combine(moviePath, file.Name);

                int index = i;

                var texture = await MediaLoadingUtility.LoadTextureAsync(
                    uri,
                    cancellationToken: cancellationToken,
                    onError: (message, u) =>
                    {
                        LoadFailed?.Invoke(
                            string.Format("Frames failed to load at {0}: {1}", u, message));
                    });

                if (cancellationToken.IsCancellationRequested)
                    break;

                if (texture != null)
                {
                    texture.name = string.Format("Image Sequence Texture {0}", index);

                    Sprite sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f)
                        );

                    sprite.name = string.Format("Image Sequence Sprite {0}", index);

                    player.frames[index] = sprite;

                    nLoadedFrames++;
                }
            }

            player.VideoMoving = wasPlaying;
        }
    }

}

