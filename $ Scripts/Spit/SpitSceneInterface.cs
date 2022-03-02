using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Display information from Spit client in scene

public class SpitSceneInterface : MonoBehaviour
{
    SpitGameClient SpitClient;

    //public TMP_Text card_text;

    //public TMP_Text MyStackCount;
    //public TMP_Text MyCardCount;
    //public TMP_Text OpponentStackCount;
    //public TMP_Text OpponentCardCount;
    
    public List<TMP_Text> MyCards;
    public List<TMP_Text> OpponentCards;

    public List<TMP_Text> Numbers;

    public TMP_Text MiddleCard0;
    public TMP_Text MiddleCard1;

    public TMP_Text SpitText;

    public TMP_Text MyName;
    public TMP_Text OpponentName;

    public int selected = -1;

    // Start is called before the first frame update
    void Start()
    {
        SpitClient = (SpitGameClient) FindObjectOfType<NetworkManager>().client.CurrentGame;
        if (SpitClient == null){
            FindObjectOfType<NetworkManager>().FatalException("Current Game Not Found - Report This");
        }
        Debug.Log(SpitClient);
        MyName.text = SpitClient.client.networkManager.Username;
        OpponentName.text = SpitClient.client.networkManager.OpponentName;
    }

    public void Quit(){
        SpitClient.client.networkManager.FatalException("Quit");
    }

    void UpdateCardText(TMP_Text text, string str){
        if (str == ""){
            text.transform.parent.gameObject.SetActive(false);
            return;
        }
        text.transform.parent.gameObject.SetActive(true);
        text.text = str;
        if (text.text[text.text.Length-1] == '♥' | text.text[text.text.Length-1] == '♦'){
            text.color = Color.red;
        }
        else{
            text.color = Color.black;
        }
    }

    void GraphicsUpdate(){
        SpitText.text = SpitClient.message;

        UpdateCardText(MiddleCard0, SpitClient.middle_cards[0]);
        UpdateCardText(MiddleCard1, SpitClient.middle_cards[1]);

        for (int i = 0; i < MyCards.Count; i++){      
            UpdateCardText(MyCards[i], SpitClient.cards_showing[SpitClient.client.ClientID, i]);
        }

        for (int i = 0; i < OpponentCards.Count; i++){
            UpdateCardText(OpponentCards[i], SpitClient.cards_showing[Util.flipInt(SpitClient.client.ClientID), i]);
        }

        for (int i = 0; i < 5; i++){
            Numbers[i].text = (i+1) + " (" + SpitClient.cards_r[SpitClient.client.ClientID, i] + ")";
        }
    }

    void MoveCard(int from, int to){
        SpitClient.MoveCard(from-1, to-1);
    }

    // Update is called once per frame
    void Update()
    {
        GraphicsUpdate();

        int curr_select = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1)){
            curr_select = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)){
            curr_select = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)){
            curr_select = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)){
            curr_select = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5)){
            curr_select = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6)){
            curr_select = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7)){
            curr_select = 7;
        }
        
        if (curr_select != -1){
            if (curr_select == selected){
                Numbers[selected-1].fontStyle = FontStyles.Normal;
                Numbers[selected-1].color = Color.white;

                selected = -1;
            }
            else{
                if (selected == -1){
                    selected = curr_select;
                    Numbers[selected-1].fontStyle = FontStyles.Bold;
                    Numbers[selected-1].color = Color.red;
                }
                else{
                    MoveCard(selected, curr_select);
                    Numbers[selected-1].fontStyle = FontStyles.Normal;
                    Numbers[selected-1].color = Color.white;
                    selected = -1;
                }
            }
        }
    }
}
