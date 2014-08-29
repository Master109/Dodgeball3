public static  var SP : BackToMainMenu;

function Awake () {
    if(SP!=null && SP!=this){
        Destroy(this);
        return;
    }
    SP = this;
    DontDestroyOnLoad(this);
}

function OnGUI(){
    if(Application.loadedLevel>=1){
        if(GUI.Button(new Rect(Screen.width-150, Screen.height-20, 150, 20), "Back to Main Menu")){
            QuitToMainMenu();
        }
    }
}

function QuitToMainMenu(){
    if(Network.isClient || Network.isServer){
        Network.Disconnect();
    }
    Application.LoadLevel(0);
}