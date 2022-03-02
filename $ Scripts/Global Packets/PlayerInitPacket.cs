// Send initial data upon connect
class PlayerInitPacketClient
{
    public static string UID = "3";

    public string name;

    public PlayerInitPacketClient(string packet){
        string[] split = packet.Split('#');
        name = split[1];
    }

    public static string Build(string name, int require_response){
        return PacketTools.Build(UID + "#" + name, require_response);
    }
}