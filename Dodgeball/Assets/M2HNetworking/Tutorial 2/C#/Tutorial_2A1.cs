using UnityEngine;
using System.Collections;

public class Tutorial_2A1 : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */


    void Update()
    {

        //This is only run on the Server
        if (Network.isServer)
        {
            Vector3 moveDirection = new Vector3(-1 * Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
            float speed = 5;
            transform.Translate(speed * moveDirection * Time.deltaTime);//now really move!
        }

    }
}