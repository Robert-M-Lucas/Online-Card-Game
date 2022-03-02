// Snap game over
public class OnGameOverSnapClient
{
    public static string UID = "140";
    
    // Cards both players flipped over
    public int winner;

    // RID/UID#snapper,winner<EOF>
    public OnGameOverSnapClient(string packet){
        string[] split = packet.Split('#');
        winner = int.Parse(split[1]);
    }

    public static string Build(int winner, int require_response){
        return PacketTools.Build(UID + "#" + winner, require_response);
    }
}