// Confirm packet recieved
public class ResponseConfirmPacket
{
    public static string UID = "20";
    public int rid;

    public ResponseConfirmPacket(string packet){
        string[] split = packet.Split('#');
        rid = int.Parse(split[1]);
    }

    public static string Build(int rid)
    {
        return PacketTools.Build(UID + "#" + rid.ToString(), 0);
    }
}