using UnityEditor;
using UnityToolbarExtender;
using UnityEngine;

namespace NKStudio
{
    public static class TargetFrameToolbar
    {
        public static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label("프레임 목표치", GUILayout.Width(80));
            Application.targetFrameRate = EditorGUILayout.IntField(Application.targetFrameRate, GUILayout.Width(50));
        }
    }
}