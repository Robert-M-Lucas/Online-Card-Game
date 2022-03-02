using System.Collections;
using System;
using System.Threading;

using System.Collections.Generic;
using UnityEngine;

public class SnapGameServer : GameParentServer
{
    Dictionary<string, Func<string, SnapGameServer, int, bool>> PacketActions = new Dictionary<string, Func<string, SnapGameServer, int, bool>>();

    public Server server;

    public Deck deck = new Deck();

    public int[] cards_r; // Cards remaining in both players hands
    public int cards_in_stack; // Total cards in middle

    public bool snapped = false; // Have these cards been snapped
    int i = 0;

    public bool pauseUpdate = false;

    public bool gameOver = false;

    public string[] cards_showing; // Cards showing in the middle

    public SnapGameServer(Server _server) : base(_server){
        Debug.Log("Snap server started");
        base.MsBetweenUpdates = 4500;
        server = _server;
        SnapServerPacketHandler.Register(PacketActions);
        Reset();
    }

    public void Reset() {
        cards_r = new int[] {26, 26};
        cards_in_stack = 0;
        cards_showing = new string[] {"", ""};
        snapped = false;
        pauseUpdate = false;
    }

    public void SoftReset() {
        cards_in_stack = 0;
        cards_showing = new string[] {"", ""};
        snapped = false;
        pauseUpdate = false;
    }

    public override bool HandlePacket(string UID, string packet, int player){
        if (PacketActions.ContainsKey(UID)){
            PacketActions[UID](packet, this, player);
            return true;
        }
        return false;
    }

    public override void Update() {
        if (gameOver){ return; }

        if (snapped & i < 1) {i++; return;} // Delay after snap

        if (i >= 1) { 
            i = 0; SoftReset(); 
        }
        else{
            cards_showing[0] = deck.RemoveRandomCard();
            cards_showing[1] = deck.RemoveRandomCard();

            cards_in_stack += 2;

            cards_r[0] -= 1;
            cards_r[1] -= 1;
        }

        snapped = false;
        

        string packet2 = SnapUpdateClient.Build(cards_showing[0], cards_showing[1], cards_r[0], cards_r[1], cards_in_stack, server.RID);
        server.SendMessage(0, packet2, true);
        packet2 = SnapUpdateClient.Build(cards_showing[0], cards_showing[1], cards_r[0], cards_r[1], cards_in_stack, server.RID);
        server.SendMessage(1, packet2, true);

        // Check for winner
        int winner = -1;
        if (cards_r[0] <= 0){
            winner = 0;
            cards_r[0] = 0;
            if (cards_r[1] <= 0){
                cards_r[1] = 0;
            }
        }
        else if (cards_r[1] <= 0){
            winner = 1;
            cards_r[1] = 0;
            if (cards_r[0] <= 0){
                cards_r[0] = 0;
            }
        }

        if (winner != -1){
            gameOver = true;

            string packet = OnGameOverSnapClient.Build(winner, server.RID);
            server.SendMessage(0, packet, true);
            packet = OnGameOverSnapClient.Build(winner, server.RID);
            server.SendMessage(1, packet, true);

            return;
        }

        Debug.Log("Snap server update");
    }
}

public static class SnapServerPacketHandler{
    public static void Register(Dictionary<string, Func<string, SnapGameServer, int, bool>> PacketActions){
        //PacketActions["20"] = ResponseConfirm;
        PacketActions["120"] = RecieveSnap;

    }
    
    public static bool RecieveSnap(string _packet, SnapGameServer server, int player){
        if (server.snapped) {return true;}

        TrySnapServer data = new TrySnapServer(_packet);
        if (data.card0 == "" | data.card1 == "") { return true; } // Trying to snap when there are no cards

        if (data.card0 != server.cards_showing[0] | data.card1 != server.cards_showing[1]) { return true; } // Trying to snap on cards no longer showing

        Debug.Log("SERVER: SNAP");

        server.snapped = true;
        server.pauseUpdate = true;

        // Who won the snap
        int winner;
        int notWinner;
        if (server.cards_showing[0][0] == server.cards_showing[1][0]){
            winner = player;
            notWinner = Util.flipInt(player);
        }
        else{
            winner = Util.flipInt(player);
            notWinner = player;
        }

        server.cards_r[notWinner] += server.cards_in_stack;
        server.deck.ReturnCards(); // Make cards previously shown reusable

        string packet = OnSnapClient.Build(player, winner, server.server.RID);
        server.server.SendMessage(0, packet, true);
        packet = OnSnapClient.Build(player, winner, server.server.RID);
        server.server.SendMessage(1, packet, true);

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