// Try to snap
public class TrySnapServer
{
    public static string UID = "120";
    
    // Cards both players flipped over
    public string card0;
    public string card1;

    // RID/UID#card0,card1<EOF>
    public TrySnapServer(string packet){
        string[] split = packet.Split('#');
        string[] cards = split[1].Split(',');
        card0 = cards[0];
        card1 = cards[1];
    }

    public static string Build(string card0, string card1, int require_response){
        return PacketTools.Build(UID + "#" + card0 + "," + card1, require_response);
    }
}