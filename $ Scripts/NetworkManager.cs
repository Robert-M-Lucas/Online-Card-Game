using System.Collections;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public string[] BannedChars = new string[] { ";", ":", "-", "#", "@", "<", ">" };
    public string DefaultName = "default_name";

    public string Username = "";

    public string OpponentName = "";

    public string IPAddress = "IP Address not found";

    public Server server;

    public Client client;

    public string version;

    public MainMenuManager mainMenu;

    public bool InMainMenu = true;

    public bool running = false;

    public int scene = 0;
    int prev_scene = 0;

    bool fatalException = false;
    string fatalExceptionString = "";

    void GetIPAddress()
    {
        string address = "";
        WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
        using (WebResponse response = request.GetResponse())
        using (StreamReader stream = new StreamReader(response.GetResponseStream()))
        {
            address = stream.ReadToEnd();
        }

        int first = address.IndexOf("Address: ") + 9;
        int last = address.LastIndexOf("</body>");
        address = address.Substring(first, last - first);

        IPAddress = address;
    }

    // Check if name can be used
    public string CheckName(string name)
    {
        if (name.Length < 1) // If no name, return default
        {
            return DefaultName + " [" + ((int)UnityEngine.Random.Range(1, 10000)).ToString() + "]";
        }

        foreach (string banned in BannedChars)
        {
            if (name.Contains(banned))
            {
                QuitLobby("Name can't contain '" + banned + "'");
                return "";
            }
        }

        return name;
    }

    void Start()
    {
        new Thread(GetIPAddress).Start();
        //PlayerPrefs.SetString("FatalException", "");
        DontDestroyOnLoad(this);
        version = Application.version;
    }

    public void StartServer() {
        running = true;
        server = new Server(this);
        server.Start();
    }

    public void StartClient(string username, string ip = "127.0.0.1"){
        Username = CheckName(username);
        if (ip == "") {ip = "127.0.0.1"; }
        if (Username == "") { return; }
        running = true;
        client = new Client(this);
        client.Start(ip);
    }

    public void QuitLobby(string reason){
        if (InMainMenu) { mainMenu.QuitToLobby = true; mainMenu.Reason = reason;}
        
        this.OnApplicationQuit();
    }

    public void FatalException(string exception)
    {
        if (!running) { return; }

        Debug.LogError(exception);
        fatalExceptionString = exception;
        OnApplicationQuit();
        fatalException = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (scene != prev_scene){
            Debug.Log("Net Manager Scene Load");
            SceneManager.LoadScene(scene);
            prev_scene = scene;
        }
        if (fatalException){
            Debug.Log("Net Manager Fatal Exception");
            PlayerPrefs.SetString("FatalException", fatalExceptionString); // Store fatal exception so it persists after this object is destroyed and scene changes
            Destroy(this.gameObject);
            SceneManager.LoadScene(0);
        }
    }

    private void OnApplicationQuit() {
        running = false;

        try{server.Stop();}catch{}

        try{client.Stop();}catch{}
    }
}
