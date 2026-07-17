namespace rlmg.Tools.MediaPlayers
{
    using System.Collections;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Events;
    using rlmg.Tools.ContentLoading;

    /// <summary>
    /// A base class for loading media, usually for a playback manager
    /// </summary>
    public class MediaFileLoader : MonoBehaviour, ILoadingProgressTracker
    {
        /// <summary>
        /// Sets the loadingMethod by which the images should be loaded into frames array
        /// </summary>
        [SerializeField]
        protected MediaLoadingMethod loadingMethod = MediaLoadingMethod.None;

        /// <summary>
        /// If true, LoadMovie is called on Awake
        /// </summary>
        [SerializeField]
        private bool doLoadOnAwake = true;

        public bool DoLoadOnAwake
        {
            get
            {
                return doLoadOnAwake;
            }
            private set
            {
                doLoadOnAwake = value;
            }
        }

        /// <summary>
        /// If false, LoadMovieRoutine completes without calling the loading methods.
        /// </summary>
		[SerializeField]
        protected bool doLoadMovie = true;

        /// <summary>
        /// Is set to true after loading the movie the first time.
        /// </summary>
        public bool DidLoadSucceed = false;

        protected bool isLoading;

        public UnityEvent LoadStarting;

        public UnityEvent LoadSucceeded;

        public UnityEvent<string> LoadFailed;

        public UnityEvent LoadFinished;

        /// <summary>
        /// Path to directory where image sequence should be looked for.
        /// Based on defaultLoadingMethod.
        /// </summary>
        protected virtual string moviePath
        {
            get
            {
                string path = "";

                switch (loadingMethod)
                {
                    case MediaLoadingMethod.StreamingAssets:
                    case MediaLoadingMethod.AssetBundles: // any bundles shipped with the game should also be in StreamingAssets
                        path = Application.streamingAssetsPath;
                        break;
                    case MediaLoadingMethod.Resources:
                        // nothing to prepend; Unity will handle searching all Resources folders
                        break;
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public virtual float LoadingProgress => 0f;

        public virtual bool IsLoading => isLoading;

        protected virtual void Awake()
        {
            if (doLoadOnAwake)
                LoadMovie();
        }

        /// <summary>
        /// Set the loading method / source for this loader (e.g. Streaming Assets, Resources)
        /// </summary>
        /// <param name="_loadingMethod"></param>
        public virtual void Configure(
            bool _doLoadOnAwake = true,
            MediaLoadingMethod _loadingMethod = MediaLoadingMethod.None)
        {
            doLoadOnAwake = _doLoadOnAwake;
            loadingMethod = _loadingMethod;
        }

        /// <summary>
        /// Start loading media
        /// </summary>
        public virtual void LoadMovie()
        {
            StopAllCoroutines();
            StartCoroutine(LoadMovieRoutine());
        }

        /// <summary>
        /// Routine for loading media
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator LoadMovieRoutine()
        {
            isLoading = true;

            DidLoadSucceed = false;

            LoadStarting?.Invoke();

            if (doLoadMovie)
            {
                yield return MainLoadMedia();
            }

            isLoading = false;

            LoadFinished?.Invoke();

            yield break;
        }

        /// <summary>
        /// Where a media file(s) should actually be loaded in a subclass
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator MainLoadMedia()
        {
            DidLoadSucceed = true;
            LoadSucceeded?.Invoke();
            yield break;
        }
    }

}