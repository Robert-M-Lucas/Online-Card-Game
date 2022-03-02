using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Allow the snap client to communicate with the scene

public class SnapSceneInterface : MonoBehaviour
{
    SnapGameClient SnapClient;

    //public TMP_Text card_text;

    public TMP_Text MyStackCount;
    public TMP_Text OpponentStackCount;
    public TMP_Text MiddleCard0;
    public TMP_Text MiddleCard1;
    public TMP_Text MiddleCardCount;

    public TMP_Text SnapText;

    public TMP_Text MyName;
    public TMP_Text OpponentName;

    // Start is called before the first frame update
    void Start()
    {
        SnapClient = (SnapGameClient) FindObjectOfType<NetworkManager>().client.CurrentGame;
        MyName.text = SnapClient.client.networkManager.Username;
        OpponentName.text = SnapClient.client.networkManager.OpponentName;
    }

    public void Quit(){
        SnapClient.client.networkManager.FatalException("Quit");
    }


    public void Snap(){
        SnapClient.Snap();
        Debug.Log("SNAP!");
    }

    // Update is called once per frame
    void Update()
    {
        MyStackCount.text = SnapClient.CardsRemaining[0].ToString();
        OpponentStackCount.text = SnapClient.CardsRemaining[1].ToString();

        MiddleCard0.text = SnapClient.CurrentCards[0];
        MiddleCard1.text = SnapClient.CurrentCards[1];

        if (SnapClient.CurrentCards[0].Length == 2 & SnapClient.CurrentCards[1].Length == 2){
            if (SnapClient.CurrentCards[0][1] == '♥' | SnapClient.CurrentCards[0][1] == '♦'){
                MiddleCard0.color = Color.red;
            }
            else{
                MiddleCard0.color = Color.black;
            }

            
            if (SnapClient.CurrentCards[1][1] == '♥' | SnapClient.CurrentCards[1][1] == '♦'){
                MiddleCard1.color = Color.red;
            }
            else{
                MiddleCard1.color = Color.black;
            }
        }

        MiddleCardCount.text = SnapClient.CardsRemaining[2].ToString();

        SnapText.text = SnapClient.SnapText;

        if (Input.GetKeyDown(KeyCode.Space)){
            Snap();
        }
    }
}
