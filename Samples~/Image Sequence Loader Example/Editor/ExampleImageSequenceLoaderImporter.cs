namespace rlmg.Tools.MediaPlayers.Examples.Editor
{
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    public static class ExampleImageSequenceLoaderImporter
    {
        static ExampleImageSequenceLoaderImporter()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {

            if (scene.name.Contains("ExampleImageSequenceLoader"))
                ImportStreamingAssets();
        }
        private static void ImportStreamingAssets()
        {
            SampleStreamingAssetsImporterUtils.CopySampleFilesToStreamingAssets(
                importerScriptName: nameof(ExampleImageSequenceLoaderImporter),
                destStreamingAssetsSubFolderName: "RLMG Media Players Samples/Image Sequence Loader");
        }
    }

}
