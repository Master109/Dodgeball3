using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public string thisName = "Bugged name";
    public NetworkView rigidBodyView;
    public int hp = 100;
    public scoreBoard theScoreBoard;
    public bool localPlayer = false;

    public Material metalMaterial;
    private Material orgMaterial;
    private float coloredUntill;
    private bool invincible;
    void Awake()
    {
        orgMaterial = renderer.material;

        theScoreBoard = GameObject.Find("Generalscripts").GetComponent<scoreBoard>() as scoreBoard;
    }

    void OnNetworkInstantiate(NetworkMessageInfo msg)
    {
        // This is our own player
        if (networkView.isMine)
        {
            //camera.main.enabled=false;


            localPlayer = true;
            networkView.RPC("setName", RPCMode.Others, thisName);

            Destroy(GameObject.Find("LevelCamera"));
            thisName = PlayerPrefs.GetString("playerName");

            Machinegun gun = transform.Find("CrateCamera/Weapon").GetComponent<Machinegun>();
            gun.localPlayer = true;



        }
        // This is just some remote controlled player, don't execute direct
        // user input on this. DO enable multiplayer controll
        else
        {
            thisName = "Remote" + Random.Range(1, 10);
            name += thisName;

            transform.Find("CrateCamera").gameObject.active = false;

            FPSWalker4 tmp2 = GetComponent<FPSWalker4>() as FPSWalker4;
            tmp2.enabled = false;
            MouseLook tmp5 = GetComponent<MouseLook>() as MouseLook;
            tmp5.enabled = false;

            networkView.RPC("askName", networkView.viewID.owner, Network.player);


        }
    }

    void OnGUI()
    {
        if (localPlayer)
        {
            GUILayout.Label("HP: " + hp);
        }
    }
    [RPC]
    IEnumerator StartInvincibility()
    {
        invincible = true;
        renderer.material = metalMaterial;

        yield return new WaitForSeconds(10);

        renderer.material = orgMaterial;
        invincible = false;
    }
    void ApplyDamage(string[] info)
    {
        float damage = float.Parse(info[0]);
        //string killerName = info[1];

        if (invincible)
        {
            return;
        }

        hp -= (int)damage;
        if (hp < 0)
        {
            theScoreBoard.LocalPlayerHasKilled();
            networkView.RPC("Respawn", RPCMode.All);
        }
        else
        {
            networkView.RPC("setHP", RPCMode.Others, hp);
        }
    }
    [RPC]
    void setHP(int newHP)
    {
        hp = newHP;
    }

    [RPC]
    void Respawn()
    {
        if (networkView.isMine)
        {
            theScoreBoard.LocalPlayerDied();

            // Randomize starting location
            GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
            Transform spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Length)].transform;

            transform.position = spawnpoint.position;
            transform.rotation = spawnpoint.rotation;
        }
        hp = 100;
    }

    [RPC]
    void setName(string name)
    {
        thisName = name;
    }

    [RPC]
    void askName(NetworkPlayer asker)
    {
        networkView.RPC("setName", asker, thisName);
    }
}