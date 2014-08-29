using UnityEngine;
using System.Collections;

public class MultiplayerMenu : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    private bool showMenu = false;
    private Rect myWindowRect;
    private MainMenu mainMenuScript;

    void Awake()
    {
        myWindowRect = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200);
    }
    void Start()
    {
        mainMenuScript = MainMenu.SP;
    }
    public void EnableMenu()
    {
        showMenu = true;
    }

    void OnGUI()
    {
        if (!showMenu)
        {
            return;
        }
        myWindowRect = GUILayout.Window(0, myWindowRect, windowGUI, "Multiplayer");
    }
    void windowGUI(int id)
    {

        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("");
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        if (GUI.Button(new Rect(50, 60, 200, 20), "Host a game"))
        {
            showMenu = false;
            mainMenuScript.OpenMenu("multiplayer-host");
        }

        if (GUI.Button(new Rect(50, 90, 200, 20), "Select a game to join"))
        {
            showMenu = false;
            mainMenuScript.OpenMenu("multiplayer-join");
        }

    }
}
