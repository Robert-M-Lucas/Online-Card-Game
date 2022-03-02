using UnityEngine;

// Kick a client with a reason e.g. version mismatch

class KickPacketClient
{
    public static string UID = "10";
    public string reason;

    public KickPacketClient(string packet){
        string[] split = packet.Split('#');
        reason = split[1];
    }

    public static string Build(string reason, int require_response){
        return PacketTools.Build(UID + "#" + reason, require_response);
    }
}