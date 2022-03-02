// Update cards showing
using UnityEngine;
public class SpitUpdateClient
{
    public static string UID = "220";
    
    // Cards both players flipped over
    
    public string[] player_cards;

    public string[] middle_cards;

    public int[,] cards_r;

    public string message;

    // RID/UID#p0c0,p0c1;c0,c1@cards_r@message<EOF>

    public SpitUpdateClient(string packet){
        string[] split = packet.Split('#')[1].Split('@');
        
        Debug.Log(split.Length);
        Debug.Log(packet);
        message = split[2];

        string[] card_names = split[0].Split(';');
        player_cards = card_names[0].Split(',');
        middle_cards = card_names[1].Split(',');


        string[] cards_r_str = split[1].Split(',');
        cards_r = new int[2,5];
        int i = 0;
        int x = 0;
        foreach (string str in cards_r_str){
            cards_r[i/5, x] = int.Parse(str);    

            i ++;
            x ++;
            if (i == 5){
                x -= 5;
            }
        }
    }

    public static string Build(string[,] player_cards, string[] middle_cards, int[,] cards_r, string message, int require_response){
        return PacketTools.Build(UID + "#" + player_cards[0,0] + "," + player_cards[0,1] + "," + player_cards[0,2] + "," + player_cards[0,3] + "," + player_cards[0,4] + "," + 
        player_cards[1,0] + "," + player_cards[1,1] + "," + player_cards[1,2] + "," + player_cards[1,3] + "," + player_cards[1,4] + ";" + 
        middle_cards[0] + "," + middle_cards[1] + 
        "@" + cards_r[0,0] + "," + cards_r[0,1] + "," + cards_r[0,2] + "," + cards_r[0,3] + "," + cards_r[0,4] + "," + 
        cards_r[1,0] + "," + cards_r[1,1] + "," + cards_r[1,2] + "," + cards_r[1,3] + "," + cards_r[1,4] + "@" + message, require_response);
    }
}
