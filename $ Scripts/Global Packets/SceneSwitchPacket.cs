// Switch Scenes
public class ScenesSwitchPacket{
    public static string UID = "4";
    
    public int scene;

    public ScenesSwitchPacket(string packet){
        scene = int.Parse(packet.Split("#")[1]);
    }

    public static string Build(int scene, int require_response)
    {
        return PacketTools.Build(UID + "#" + scene.ToString(), require_response);
    }

}