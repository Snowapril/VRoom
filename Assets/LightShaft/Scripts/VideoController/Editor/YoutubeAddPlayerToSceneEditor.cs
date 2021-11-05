using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class YoutubeAddPlayerToSceneEditor : EditorWindow
{

    [MenuItem("Window/Youtube/Add Youtube Video Player")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(YoutubeAddPlayerToSceneEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Add a video Player to your scene", EditorStyles.boldLabel);
        if (GUILayout.Button("Add youtube video Player"))
        {
            AddPlayer();
        }
    }

    void AddPlayer()
    {
        GameObject.Instantiate((GameObject)Resources.Load("Prefabs/YoutubePlayer", typeof(GameObject)));
    }
}
