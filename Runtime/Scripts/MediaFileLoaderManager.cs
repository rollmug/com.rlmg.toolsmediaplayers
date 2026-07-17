namespace rlmg.Tools.MediaPlayers
{
    using rlmg.Tools.ContentLoading;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Base class for managing multiple media loaders in a single scene.
    /// Features:
    /// 1. Progress tracking across all managed loaders
    /// 2. Override configuration of all managed loaders
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class MediaFileLoaderManager : MonoBehaviour,
        ILoadingProgressTracker
    {
        /// <summary>
        /// Final managed loaders list
        /// </summary>
        protected List<MediaFileLoader> managedLoaders = new();

        /// <summary>
        /// Loaders to add to managed loaders, which will have their loading executed in order
        /// </summary>
        [SerializeField]
        [Tooltip("Loaders to add to managed loaders, which will have their loading executed in order")]
        protected List<MediaFileLoader> orderedLoaders = new();

        /// <summary>
        /// Whether any loaders present in the scene that are not included in orderedLoaders should be appended to the managed loaders list
        /// </summary>
        [SerializeField]
        [Tooltip("Whether any loaders present in the scene that are not included in orderedLoaders should be appended to the managed loaders list")]
        protected bool doFindAllLoadersAtRuntime;

        /// <summary>
        /// Loaders to exclude from the final managed loaders list.
        /// Will result in excluding loaders included in orderedLoaders and loaders found at runtime.
        /// </summary>
        [Tooltip("Loaders to exclude from the final managed loaders list.")]
        [SerializeField]
        protected List<MediaFileLoader> excludedLoaders = new();

        /// <summary>
        /// MediaLoadingMethod used to override the loading method of the loaders managed by this class.
        /// </summary>
        [Header("Configuration Overrides")]
        [Tooltip("MediaLoadingMethod used to override the loading method of the loaders managed by this class.")]
        [SerializeField]
        protected MediaLoadingMethod overrideLoadingMethod = MediaLoadingMethod.None;

        /// <summary>
        /// Number of managed loaders
        /// </summary>
        public int LoadersCount => managedLoaders.Count;

        /// <summary>
        /// Number of managed loaders that are currently loading
        /// </summary>
        public int ActiveLoadersCount => managedLoaders.Where(i => i.IsLoading).Count();

        /// <summary>
        /// Is any managed loader currently loading?
        /// </summary>
        public bool IsAnyLoaderActive => ActiveLoadersCount > 0;

        /// <summary>
        /// Is any managed loader currently loading?
        /// </summary>
        public bool IsLoading => IsAnyLoaderActive;

        /// <summary>
        /// Loading progress towards the cumulative progress of all managed loaders
        /// </summary>
        public float LoadingProgress
        {
            get
            {
                if (managedLoaders.Count == 0)
                    return 0;

                float total = 0;

                foreach (var loader in managedLoaders)
                    total += loader.LoadingProgress;

                return total / managedLoaders.Count;
            }
        }

        protected virtual void Awake()
        {
            BuildManagedLoadersList();

            List<MediaFileLoader> toLoadOnAwake = new();

            foreach (var loader in managedLoaders)
            {
                if (loader.DoLoadOnAwake)
                    toLoadOnAwake.Add(loader);

                loader.Configure(
                    _doLoadOnAwake: false,
                    _loadingMethod: overrideLoadingMethod);
            }

            foreach (var loader in toLoadOnAwake)
                loader.LoadMovie();
                
        }

        protected void BuildManagedLoadersList()
        {
            managedLoaders.Clear();

            foreach (var loader in orderedLoaders)
                managedLoaders.Add(loader);

            if (doFindAllLoadersAtRuntime)
                foreach (var loader in FindLoaders())
                    if (!managedLoaders.Contains(loader))
                        managedLoaders.Add(loader);

            foreach (var loader in excludedLoaders)
                managedLoaders.Remove(loader);
        }

        protected List<MediaFileLoader> FindLoaders() => FindObjectsByType<MediaFileLoader>(FindObjectsSortMode.InstanceID).ToList();
        
    }

}