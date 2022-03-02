// Send information about a snap (who snapped, who won)
public class OnSnapClient
{
    public static string UID = "130";
    
    // Cards both players flipped over
    public int snapper;
    public int winner;

    // RID/UID#snapper,winner<EOF>
    public OnSnapClient(string packet){
        string[] split = packet.Split('#');
        string[] cards = split[1].Split(',');
        snapper = int.Parse(cards[0]);
        winner = int.Parse(cards[1]);
    }

    public static string Build(int snapper, int winner, int require_response){
        return PacketTools.Build(UID + "#" + snapper + "," + winner, require_response);
    }
}