using UnityEngine;
using System.Collections;

public class MakePersistent : MonoBehaviour
{

    private MakePersistent thisScript;

    void Awake()
    {
        //Only allow ONE instance of this script, ever(!)
        if (thisScript != null && thisScript != this)
        {
            Destroy(this.gameObject);
            return;
        }
        thisScript = this;

        DontDestroyOnLoad(this);
    }


}
