using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */


    void OnGUI()
    {

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 225, 0, 450, Screen.height));

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Select a scene");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();


        GUILayout.BeginVertical();
        if (GUILayout.Button("Tutorial 1 - Connect"))
        {
            Application.LoadLevel(1);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Tutorial 2A1 - Observe transform"))
        {
            Application.LoadLevel(2);
        }
        if (GUILayout.Button("Tutorial 2A2 - Observe code"))
        {
            Application.LoadLevel(3);
        }
        if (GUILayout.Button("Tutorial 2A3 - RPC"))
        {
            Application.LoadLevel(4);
        }
        if (GUILayout.Button("Tutorial 2B - Instantiating"))
        {
            Application.LoadLevel(5);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Tutorial 3 - Authoritative server"))
        {
            Application.LoadLevel(6);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Tutorial 4 - Allocate NetworkViewID"))
        {
            Application.LoadLevel(7);
        }
        GUILayout.EndVertical();

        GUILayout.Space(30);

        GUILayout.BeginVertical();
        if (GUILayout.Button("Example 1 - Chat"))
        {
            Application.LoadLevel(8);
        }
        if (GUILayout.Button("Example 2 - Masterserver"))
        {
            Application.LoadLevel(9);
        }
        if (GUILayout.Button("Example 3 - Lobby"))
        {
            Application.LoadLevel(11);
        }
        if (GUILayout.Button("Example 4 - FPS game"))
        {
            Application.LoadLevel(13);
        }
        if (GUILayout.Button("Example 5 - Auto matchmaking"))
        {
            Application.LoadLevel(15);
        }
        GUILayout.EndVertical();


        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();

    }
}