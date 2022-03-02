using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServerPacketResponses{
    public static void Register(Dictionary<string, Func<string, Server, int, bool>> PacketActions){
        PacketActions["20"] = ResponseConfirm;
    }
    
    // Confirm packet recieved
    public static bool ResponseConfirm(string packet, Server server, int player){
        int rid = new ResponseConfirmPacket(packet).rid;
        if (server.RequireResponse.ContainsKey(rid)){
            Tuple<int, string> _;
            if (server.RequireResponse.TryRemove(rid, out _)){
                Debug.Log("SERVER: RECIEVED CONFIRMATION");
            }
            else{
                Debug.Log("SERVER: FAILED RECIEVED CONFIRMATION");
            }
        }

        return true;
    }
}