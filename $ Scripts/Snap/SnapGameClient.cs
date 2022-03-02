using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SnapGameClient : GameParentClient
{
    Dictionary<string, Func<string, SnapGameClient, bool>> PacketActions = new Dictionary<string, Func<string, SnapGameClient, bool>>();

    public Client client;

    public string[] CurrentCards = new string[] {"", ""}; // Current cards in the middle
    public int[] CardsRemaining = new int[] {0, 0, 0}; // Cards in my hand, opponents hand and total in middle

    // Cards in middle
    public string card0 = "";
    public string card1 = "";

    // Snap information to be shown
    public string SnapText = "";

    // Has a snap been attempted for these cards
    public bool TrySnap = false;

    public SnapGameClient(Client _client) : base(_client){
        client = _client;
        base.MsBetweenUpdates = -1;
        SnapClientPacketHandler.Register(PacketActions);
    }

    public override bool HandlePacket(string UID, string packet){
        if (PacketActions.ContainsKey(UID)){
            PacketActions[UID](packet, this);
            return true;
        }
        return false;
    }

    public void Snap(){
        //if (TrySnap == false){
            client.SendMessage(TrySnapServer.Build(card0, card1, client.RID), true);
            TrySnap = true;
        //}
    }

}

public static class SnapClientPacketHandler{
    public static void Register(Dictionary<string, Func<string, SnapGameClient, bool>> PacketActions){
        //PacketActions["20"] = ResponseConfirm;
        PacketActions["110"] = OnCardReveal;
        PacketActions["130"] = SnapServerConfirm;
        PacketActions["140"] = SnapGameOver;
    }

    // When new cards are shown
    public static bool OnCardReveal(string packet, SnapGameClient client){
        client.SnapText = "";
        Debug.Log("CARDS: " + packet);
        SnapUpdateClient data = new SnapUpdateClient(packet);
        client.card0 = data.card0;
        client.card1 = data.card1;
        client.TrySnap = false;

        if (client.client.ClientID == 0) { client.CurrentCards[0] = data.card0; client.CurrentCards[1] = data.card1;
        client.CardsRemaining[0] = data.card_r0; client.CardsRemaining[1] = data.card_r1;  }
        else { client.CurrentCards[0] = data.card1; client.CurrentCards[1] = data.card0;
        client.CardsRemaining[0] = data.card_r1; client.CardsRemaining[1] = data.card_r0; }
        client.CardsRemaining[2] = data.cards_in_stack;

        //client.CurrentCards = data.card0 + " | " + data.card1 + "\n" + data.card_r0 + " | " + data.card_r1 + "\n" + data.cards_in_stack;

        return true;
    }

    // When server has confirmed a snap
    public static bool SnapServerConfirm(string packet, SnapGameClient client){
        OnSnapClient data = new OnSnapClient(packet);
        client.TrySnap = false;
        client.SnapText = client.client.IdToPlayerName(data.snapper) + " Snapped!\n" + client.client.IdToPlayerName(data.winner) + " Wins!";
        return true;
    }

    // On game over
    public static bool SnapGameOver(string packet, SnapGameClient client){
        OnGameOverSnapClient data = new OnGameOverSnapClient(packet);
        client.SnapText = client.client.IdToPlayerName(data.winner) + " has run out of cards and won!";

        return true;
    }
    
    /*public static bool ResponseConfirm(string packet, SnapGameServer server, int player){
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
    }*/
}