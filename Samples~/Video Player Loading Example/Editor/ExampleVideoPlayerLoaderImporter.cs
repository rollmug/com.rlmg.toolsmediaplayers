namespace rlmg.Tools.MediaPlayers.Examples.Editor
{
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    public static class ExampleVideoPlayerLoaderImporter
    {
        static ExampleVideoPlayerLoaderImporter()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {

            if (scene.name.Contains("ExampleVideoPlayerLoader"))
                ImportStreamingAssets();
        }
        private static void ImportStreamingAssets()
        {
            SampleStreamingAssetsImporterUtils.CopySampleFilesToStreamingAssets(
                importerScriptName: nameof(ExampleVideoPlayerLoaderImporter),
                destStreamingAssetsSubFolderName: "RLMG Media Players Samples/Video Player Loader");
        }
    }

}
