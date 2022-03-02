// Update the snap board
public class SnapUpdateClient
{
    public static string UID = "110";
    
    // Cards both players flipped over
    public string card0;
    public string card1;

    // Cards left in both players hands
    public int card_r0;
    public int card_r1;

    // Cards picked up on snap loss
    public int cards_in_stack;

    // RID/UID#card0,card1,card_r0,card_r1,cards_in_stack<EOF>
    public SnapUpdateClient(string packet){
        string[] split = packet.Split('#');
        string[] cards = split[1].Split(',');
        card0 = cards[0];
        card1 = cards[1];
        card_r0 = int.Parse(cards[2]);
        card_r1 = int.Parse(cards[3]);
        cards_in_stack = int.Parse(cards[4]);
    }

    public static string Build(string card0, string card1, int card_r0, int card_r1, int cards_in_stack, int require_response){
        return PacketTools.Build(UID + "#" + card0 + "," + card1 + "," + card_r0 + "," + card_r1 + "," + cards_in_stack, require_response);
    }
}