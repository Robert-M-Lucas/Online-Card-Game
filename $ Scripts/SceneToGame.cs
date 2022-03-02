// Ascociates scene with Game class
public static class SceneToGame{
    public static GameParentServer Server(int scene, Server server){
        switch (scene){
            case 1:
                return new SnapGameServer(server);
            case 2:
                return new SpitGameServer(server);
            default:
                return null;
        }
    }

    public static GameParentClient Client(int scene, Client client){
        switch (scene){
            case 1:
                return new SnapGameClient(client);
            case 2:
                return new SpitGameClient(client);
            default:
                return null;
        }
    }
}