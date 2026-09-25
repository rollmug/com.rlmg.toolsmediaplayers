namespace rlmg.Tools.MediaPlayers
{
    using rlmg.Tools.ContentLoading;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;

    /// <summary>
    /// Base video player manager.
    /// Features:
    /// 1. Explicit control over video loading, with option to load 
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    [DefaultExecutionOrder(-10)]
    public class VideoPlayerManager : MonoBehaviour, ILoadingProgressTracker
    {
        /// <summary>
        /// Native VideoPlayer component to manage
        /// </summary>
        public VideoPlayer VideoPlayer;

        /// <summary>
        /// Canvas element display
        /// </summary>
        [SerializeField]
        protected RawImage viewportImage;

        /// <summary>
        /// Whether or not the video should play once it's loaded; if false, the video will load its first frame and pause.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether or not the video should play whenever it's relaoded; if false, the video will load its first frame and pause.")]
        protected bool doPlayOnLoaded = true;

        [Header("Fade On Load Settings")]
        /// <summary>
        /// If true, viewport will dip to fade color (black by default) while VideoPlayer is loading
        /// </summary>
        [SerializeField]
        [Tooltip("If true, viewport will dip to fade color (black by default) while VideoPlayer is loading")]
        protected bool doFadeIfLoading = true;
        
        /// <summary>
        /// If true, video will not fade up after loading while it remains paused. Playing the video will cause the video to fade up.
        /// </summary>
        [SerializeField]
        [Tooltip("If true, video will not fade up after loading while it remains paused. Playing the video will cause the video to fade up.")]
        protected bool dontFadeUpIfPaused = false;

        /// <summary>
        /// Duration of loading-related dip to fade color
        /// </summary>
        [SerializeField]
        protected float fadeOnLoadDuration = 0.25f;

        /// <summary>
        /// Fade color to dip to while loading
        /// </summary>
        [SerializeField]
        protected Color fadeStartColor = Color.black;

        /// <summary>
        /// Color to fade up to while playing; this will tint the video playback if not white.
        /// </summary>
        [SerializeField]
        protected Color fadeEndColor = Color.white;


        /// <summary>
        /// If true, the RenderTexture used by the VideoPlayer clears its memory
        /// </summary>
        [Header("Render Texture Settings")]
        [SerializeField]
        protected bool clearRenderTextureBeforeLoad = true;

        /// <summary>
        /// If true, replaces the RenderTexture assigned in the Editor with a new object
        /// </summary>
        [SerializeField]
        protected bool regenerateRenderTexture;

        /// <summary>
        /// Size for new RenderTexture, when regenerateRenderTexture is true
        /// </summary>
        [SerializeField]
        protected Vector2 regeneratedRenderTextureSize = new Vector2(1920, 1080);

        /// <summary>
        /// Wait used when loading video
        /// </summary>
        protected WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// Coroutine used when loading video
        /// </summary>
        protected Coroutine loadRoutine;

        /// <summary>
        /// Coroutine used for dip to/from fade color
        /// </summary>
        protected Coroutine fadeUpRoutine;
        
        /// <summary>
        /// Current video loading status
        /// </summary>
        protected LoadStatus currentStatus = LoadStatus.NotYetLoaded;

        /// <summary>
        /// Did video load successfully?
        /// </summary>
        public bool DidLoadSucceed => currentStatus == LoadStatus.Succeeded;

        /// <summary>
        /// Progress fraction for video loading.
        /// This base implementation has no intermediary values: it's 0 if unloaded; 1, if loaded.
        /// </summary>
        public virtual float LoadingProgress => DidLoadSucceed ? 1f : 0f;

        public virtual LoadStatus CurrentStatus => currentStatus;
        

        protected virtual void Awake()
        {
            VideoPlayer = GetComponent<VideoPlayer>();

            if (VideoPlayer == null)
            {
                Debug.LogError("[VideoPlayerManager] Can't find VideoPlayer.");
                return;
            }

            if (regenerateRenderTexture)
            {
                VideoPlayer.targetTexture = new RenderTexture(
                    (int)regeneratedRenderTextureSize.x,
                    (int)regeneratedRenderTextureSize.y, 24);

                if (viewportImage != null)
                    viewportImage.texture = VideoPlayer.targetTexture;
            }

            if (!doPlayOnLoaded)
                VideoPlayer.playOnAwake = false;

            if (doFadeIfLoading &&
                viewportImage != null)
            {
                viewportImage.color = fadeStartColor;
            }

            LoadVideo();
        }

        protected virtual void OnEnable()
        {
            ClearRenderTexture();
        }

        /// <summary>
        /// Load video at supplied filepath
        /// </summary>
        /// <param name="videoPath"></param>
        public virtual void LoadVideo(string videoPath)
        {
            if (loadRoutine != null)
            {
                StopCoroutine(loadRoutine);
                loadRoutine = null;
            }

            if (VideoPlayer == null)
                return;

            VideoPlayer.Stop();
            
            VideoPlayer.source = VideoSource.Url;
            VideoPlayer.url = videoPath;

            LoadVideo();
        }

        /// <summary>
        /// Load video from supplied clip
        /// </summary>
        /// <param name="videoClip"></param>
        public virtual void LoadVideoClip(VideoClip videoClip)
        {
            if (loadRoutine != null)
            {
                StopCoroutine(loadRoutine);
                loadRoutine = null;
            }
                
            if (VideoPlayer == null)
                return;

            VideoPlayer.Stop();

            VideoPlayer.source = VideoSource.VideoClip;
            VideoPlayer.clip = videoClip;

            LoadVideo();
        }

        /// <summary>
        /// Main video loading method
        /// </summary>
        protected virtual void LoadVideo()
        {
            if (loadRoutine != null)
            {
                StopCoroutine(loadRoutine);
                loadRoutine = null;
            }

            if (VideoPlayer == null)
            {
                currentStatus = LoadStatus.Failed;
                return;
            }

            VideoPlayer.Stop();

            currentStatus = LoadStatus.Loading;

            // Fade viewport out before loading frame
            if (doFadeIfLoading &&
                viewportImage != null)
                viewportImage.color = fadeStartColor;

            if (clearRenderTextureBeforeLoad)
                ClearRenderTexture();

            VideoPlayer.Prepare();

            loadRoutine = StartCoroutine(LoadVideoRoutine());
        }

        /// <summary>
        /// Main video loading routine
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LoadVideoRoutine()
        {
            while (!VideoPlayer.isPrepared)
                yield return null;

            // Play to load the first frame into the RenderTexture
            VideoPlayer.Play();
            yield return waitForEndOfFrame;

            // Fade up if it should
            if (doPlayOnLoaded)
            {

                if (doFadeIfLoading &&
                    viewportImage != null)
                {
                    FadeUp();
                }

            }
            // Optionally pause after first frame is loaded
            else
            {

                VideoPlayer.Pause();
                yield return waitForEndOfFrame;

                if (doFadeIfLoading &&
                    !dontFadeUpIfPaused &&
                    viewportImage != null)
                {
                    FadeUp();
                }
            }

            currentStatus = LoadStatus.Succeeded;
            loadRoutine = null;
        }

        protected virtual void FadeUp()
        {
            if (fadeUpRoutine != null)
                StopCoroutine(fadeUpRoutine);

            fadeUpRoutine = StartCoroutine(FadeUpRoutine());
        }

        protected virtual IEnumerator FadeUpRoutine()
        {
            if (viewportImage == null)
                yield break;

            Color startColor = viewportImage.color;

            float time = 0f;

            while (time < fadeOnLoadDuration)
            {
                float progress = time / fadeOnLoadDuration;

                viewportImage.color = Color.Lerp(startColor, fadeEndColor, progress);

                time += Time.deltaTime;

                yield return null;
            }

            viewportImage.color = fadeEndColor;

            fadeUpRoutine = null;
        }

        /// <summary>
        /// Wipe RenderTexture memory and replace with fadeStartColor
        /// </summary>
        protected void ClearRenderTexture()
        {
            ClearRenderTexture(fadeStartColor);
        }

        /// <summary>
        /// Wipe RenderTexture memory and replace with supplied color
        /// </summary>
        /// <param name="color">Color of RenderTexture on wipe</param>
        protected void ClearRenderTexture(Color color)
        {
            //attempting to clear the render texture to avoid any vestiges from the previously played video
            //https://forum.unity.com/threads/how-to-clear-a-render-texture-to-transparent-color-all-bytes-at-0.147431/

            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = VideoPlayer.targetTexture;

            GL.Clear(true, true, color);

            RenderTexture.active = rt;
        }
    }

}