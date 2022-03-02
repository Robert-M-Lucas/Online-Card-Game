using System.Collections;

// Initial packets ensuring version match and giving the player its ID

class InitialPacketServer
{
    public static string UID = "1";
    public string name;
    public string version;

    public InitialPacketServer(string packet){
        string[] split = packet.Split("#")[1].Split(';');
        name = split[0];
        version = split[1];
    }

    public static string Build(string name, string version, int require_response)
    {
        return PacketTools.Build(UID + "#" + name + ";" + version, require_response);
    }
}

class InitalPacketClient
{
    public static string UID = "2";

    public int playerID;

    public InitalPacketClient(string packet){
        playerID = int.Parse(packet.Split('#')[1]);
    }

    public static string Build(int require_response, int player_id){
        return PacketTools.Build(UID+"#"+player_id.ToString(), require_response);
    }
}