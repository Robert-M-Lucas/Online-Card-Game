// Author: Robert Lucas
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Handles all UI in the main menu
public class MainMenuManager : MonoBehaviour
{

    [Header("Disclaimer")]
    public string DisclaimerVersion;
    public GameObject Disclaimer;
    public Text DisclaimerText;

    //public NetworkManager networkManager;

    [Space(2)]
    [Header("Screens")]

    public GameObject[] Screens;

    public bool Host;
    public bool InLobby;

    public Text LobbyText;

    public Text ErrorText;
    public Text FatalErrorText;

    [Space(2)]
    [Header("Txt boxes")]

    public Text IPEntry;
    public Text NameEntry;

    public Text ShowIPButton;
    private bool ShowIP = false;

    [Space(2)]
    [Header("IP")]

    public GameObject IP;
    public Text IPText;
    public GameObject IPHidden;

    /*
    [Space(2)]
    [Header("Player Cards")]

    public List<int> DisplayedPlayers = new List<int>();

    public GameObject PlayerCardPrefab;

    public List<GameObject> PlayerCards = new List<GameObject>();
    */

    [Space(2)]
    [Header("Other")]

    public Text SceneInput;
    public TMP_Dropdown SceneInputDropdown;

    // AsyncOperation asyncLoad;
    // public bool LoadingScene = false;
    // public int PracticeLevel;

    public Text VersionText;

    public bool QuitToLobby = false;
    public string Reason;

    public NetworkManager networkManager;

    public AudioControlScript AudioControl;

    public TMP_Text ServerInfoText;
    public TMP_Text ClientInfoText;

    public TMP_Text YouText; // Your name
    public TMP_Text OpponentText; // Opponent name

    void Start()
    {
        //networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        ShowScreen("0");

        // Hide disclaimer if already read
        if (PlayerPrefs.HasKey("DiscVer") & PlayerPrefs.GetString("DiscVer") == DisclaimerVersion)
        {
            Disclaimer.SetActive(false);
        }
        else
        {
            StreamReader reader = new StreamReader("Assets/Resources/Disclaimer.txt");
            DisclaimerText.text = reader.ReadToEnd();
            reader.Close();
            Disclaimer.SetActive(true);
        }

        VersionText.text = Application.version; // Show current version
    }

    // Show a screen
    public void ShowScreen(string ScreenString)
    {
        AudioControl.PlaySound("Button");

        string[] ScreenList = ScreenString.Split(';');
        for (int i = 0; i < Screens.Length; i++)
        {
            Screens[i].SetActive(false);
        }
        foreach (string screenName in ScreenList)
        {
            Screens[int.Parse(screenName)].SetActive(true);
        }
    }

    // Loads practice level - unused
    // public void StartPractice()
    // {
    //     SceneManager.LoadScene(PracticeLevel);
    // }

    // Disclaimer hide button
    public void DisclaimerHide()
    {
        AudioControl.PlaySound("Button");
        Disclaimer.SetActive(false);
        PlayerPrefs.SetString("DiscVer", DisclaimerVersion); // Update disclaimer version seen by client
    }

    // When player selects host
    public void HostClick(bool local)
    {
        ClientInfoText.text = "";
        ServerInfoText.text = "";
        
        // string test = networkManager.HostGame(NameEntry.text, local);
        // if (test != null)
        // {
        //     ErrorText.text = test;
        //     return;
        // }

        Host = true;
        InLobby = true;

        //ErrorText.text = "";

        //IP.GetComponent<Text>().text = networkManager.IP;

        ShowScreen("1;2");

        LobbyText.text = "Lobby (Host)";

        networkManager.StartServer();
        networkManager.StartClient(NameEntry.text);
    }

    // When player selects client
    public void JoinClick()
    {
        ClientInfoText.text = "";
        ServerInfoText.text = "Not hosting";
        // string test = networkManager.JoinGame(IPEntry.text, NameEntry.text);
        // if (test != null)
        // {
        //     ErrorText.text = test;
        //     return;
        // }

        Host = false;
        InLobby = true;

        //ErrorText.text = "";

        IP.GetComponent<Text>().text = IPEntry.text; // Get IP connecting to

        ShowScreen("1");

        LobbyText.text = "Lobby (Client)";

        networkManager.StartClient(NameEntry.text, IPEntry.text);
    }

    // Exiting a lobby
    public void ExitLobbyClick()
    {
        //networkManager.StopClientServer();
        ExitLobbyUI();
    }

    public void ExitLobbyUI()
    {
        Host = false;
        InLobby = false;

        ShowScreen("0");
        
        ErrorText.text = Reason;
    }

    // Close games
    public void ExitGame()
    {
        AudioControl.PlaySound("Button");
        Application.Quit();
    }

    // Show / hide IP
    public void IPToggle()
    {
        AudioControl.PlaySound("Button");
        //IP.GetComponent<Text>().text = networkManager.IP;
        ShowIP = !ShowIP;

        IP.SetActive(ShowIP);
        IPHidden.SetActive(!ShowIP);
        if (ShowIP)
        {
            ShowIPButton.text = "Hide IP";
        }
        else
        {
            ShowIPButton.text = "Show IP";
        }
    }

    // Start the match (host only)
    public void StartGame()
    {
        AudioControl.PlaySound("Button");

        if (networkManager.server.UsersConnected != 2){
            return;
        }

        int scene;
        if (int.TryParse(SceneInputDropdown.options[SceneInputDropdown.value].text.Substring(0, 1), out scene))
        {
            networkManager.server.SwitchGame(scene);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        IPText.text = networkManager.IPAddress;

        if (QuitToLobby)
        {
            ExitLobbyUI();
            QuitToLobby = false;
        }

        // Update info text
        if (networkManager.running){
            if (Host){
                ServerInfoText.text = networkManager.server.currentMessage;
            }
            ClientInfoText.text = networkManager.client.currentMessage;

            YouText.text = networkManager.Username;
            OpponentText.text = networkManager.OpponentName;
        }

        // Show fatal exception
        if (PlayerPrefs.HasKey("FatalException")){
            FatalErrorText.text = PlayerPrefs.GetString("FatalException");
        }
        
    }
}
