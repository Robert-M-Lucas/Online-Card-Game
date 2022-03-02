// Try to move a card
public class MoveCardSpitServer
{
    public static string UID = "210";
    
    // Cards both players flipped over
    public int player;
    public int player_pile_from;

    public string card_from;

    public int pile_to; //0 - 3 are player piles, 4 - 5 are middle piles

    public string card_to;

    // RID/UID#player,player_pile_from,pile_to,card_from,card_to<EOF>
    public MoveCardSpitServer(string packet){
        string[] split = packet.Split('#')[1].Split(',');
        player = int.Parse(split[0]);
        player_pile_from = int.Parse(split[1]);
        pile_to = int.Parse(split[2]);
        card_from = split[3];
        card_to = split[4];
        
    }

    public static string Build(int player, int player_pile_from, int player_pile_to, string card_from, string card_to, int require_response){
        return PacketTools.Build(UID + "#" + player + "," + player_pile_from + "," + player_pile_to + "," + card_from + "," + card_to, require_response);
    }
}
