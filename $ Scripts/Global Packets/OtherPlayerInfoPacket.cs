
// Give information about the other connected player
class OtherPlayerInfoPacketClient
{
    public static string UID = "5";
    public string name;

    public OtherPlayerInfoPacketClient(string packet){
        string[] split = packet.Split('#');
        name = split[1];
    }

    public static string Build(string name, int require_response){
        return PacketTools.Build(UID + "#" + name, require_response);
    }
}