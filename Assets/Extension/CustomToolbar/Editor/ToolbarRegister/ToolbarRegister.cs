using UnityEditor;
using UnityToolbarExtender;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NKStudio
{
    [InitializeOnLoad]
    public class ToolbarRegister
    {
        static ToolbarRegister()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUIRight);

            // Add play mode state change callback
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnToolbarGUILeft()
        {
            EnterPlayModeOptionToolbars.OnToolbarGUI();
#if USE_FMOD
            FMODDebugToolbars.OnToolbarGUI();
#endif
            FrameRateToolbar.OnToolbarGUI();
            TimeScaleToolbar.OnToolbarGUI();
            TargetFrameToolbar.OnToolbarGUI();
        }

        static void OnToolbarGUIRight()
        {
            RestartSceneToolbar.OnToolbarGUI();
            SceneSwitchRightButton.OnToolbarGUI();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Get the current active scene
                Scene currentScene = SceneManager.GetActiveScene();
                string sceneName = currentScene.name;

                // Display a colored debug message in the console
                Debug.Log($"<color=yellow> H.MOD </color> <color=white> : </color> <color=green> 해당 < </color> <color=white> {sceneName} </color> <color=green> > 시작 되었습니다. </color>");
            }
        }
    }
}