using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientPacketResponses{

    Client client;

    public ClientPacketResponses(Client _client, Dictionary<string, Func<string, Client, bool>> PacketActions){
        PacketActions["20"] = ResponseConfirm;
        PacketActions["4"] = SceneSwitch;
        PacketActions["5"] = OpponentInfo;
        PacketActions["2"] = InitialPacket;

        client = _client;
    }
    
    // Confirm packet recieved
    public bool ResponseConfirm(string packet, Client client){
        int rid = new ResponseConfirmPacket(packet).rid;
        Debug.Log("CLIENT: CONFIRM PACKET RECIEVED (" + rid + ")");
        if (client.RequireResponse.ContainsKey(rid)){
            string _;
            if (client.RequireResponse.TryRemove(rid, out _)){
                Debug.Log("CLIENT: RECIEVED CONFIRMATION");
            }
            else{
                Debug.Log("CLIENT: FAILED RECIEVED CONFIRMATION");
            }
        }

        return true;
    }

    public bool InitialPacket(string packet, Client client){
        client.ClientID = new InitalPacketClient(packet).playerID;

        return true;
    }

    public bool SceneSwitch(string packet, Client client){
        ScenesSwitchPacket data = new ScenesSwitchPacket(packet);

        client.CurrentGame = SceneToGame.Client(data.scene, client);

        Debug.Log("CLIENT SCENE AND GAME SWITCH " + data.scene.ToString());

        client.networkManager.scene = data.scene;

        return true;
    }

    public bool OpponentInfo(string packet, Client client){
        OtherPlayerInfoPacketClient data = new OtherPlayerInfoPacketClient(packet);
        client.otherPlayerName = data.name;
        client.networkManager.OpponentName = client.otherPlayerName;
        return true;
    }
}