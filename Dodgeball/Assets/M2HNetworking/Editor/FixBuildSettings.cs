using UnityEngine;
using UnityEditor;

using System.Collections;

public class FixBuildSettings : MonoBehaviour
{

    [MenuItem("M2HNetworking/Reset buildsettings")]
    static void FixBSet()
    {
        //
        // GENERAL PROJECT SETTINGS
        //
        PlayerSettings.runInBackground = true;

        //
        //  SET SCENES
        //
        EditorBuildSettingsScene[] sceneAr = new EditorBuildSettingsScene[16];
        int i = 0;
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/_MenuScene.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 1/Tutorial_1.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 2/Tutorial_2A1.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 2/Tutorial_2A2.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 2/Tutorial_2A3.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 2/Tutorial_2B.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 3/Tutorial_3.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Tutorial 4/Tutorial_4.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example1/Example1_Chat.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example2/Example2_menu.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example2/Example2_game.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example3/Example3_lobby.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example3/Example3_game.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example4/Example4_Menu.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example4/Example4_Game.unity", true);
        sceneAr[i++] = new EditorBuildSettingsScene("Assets/M2HNetworking/Example5/Example5_Game.unity", true);

        EditorBuildSettings.scenes = sceneAr;
        Debug.Log("M2HNetworking, (re)set tutorial project build settings.");

        /*
         * Output current build settings
        string bl = "";
        int i = 0;
        foreach (EditorBuildSettingsScene sc in EditorBuildSettings.scenes)
        {
            bl += "sceneAr["+i+"] = new EditorBuildSettingsScene(\"" + sc.path + "\", true);\n";

            i++;
        }
        Debug.Log(bl);
         * */
    }

}
