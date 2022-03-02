using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpitGameClient : GameParentClient
{
    Dictionary<string, Func<string, SpitGameClient, bool>> PacketActions = new Dictionary<string, Func<string, SpitGameClient, bool>>();

    public Client client;

    public int[,] cards_r; // Cards Remaining Player 0 [0 - 4], Player 1 [0 - 4]
    public int cards_in_stack;

    public bool gameOver = false;

    public string[,] cards_showing; // 2 x 5 cards

    public string[] middle_cards;

    public bool TryMove = false;

    public string message = "";
    public Tuple<int, string>[] Move; //From, to

    public SpitGameClient(Client _client) : base(_client){
        Debug.Log("Spit Game Client Start");
        client = _client;
        base.MsBetweenUpdates = -1;
        SpitClientPacketHandler.Register(PacketActions);
        Reset();
    }

    public void Reset() {
        cards_r = new int[,] {{0, 0, 0, 0, 0}, {0, 0, 0, 0, 0}};

        middle_cards = new string[] {"", ""};

        cards_in_stack = 0;
        cards_showing = new string[,] {{"", "", "", "", ""}, {"", "", "", "", ""}};
    }

    public override bool HandlePacket(string UID, string packet){
        if (PacketActions.ContainsKey(UID)){
            PacketActions[UID](packet, this);
            return true;
        }
        return false;
    }

    public void MoveCard(int from, int to){
        Debug.Log("CLIENT: Try spit move card");
        if (from > 4){
            return; // Trying to take card from middle
        }
        string card_to = "";
        if (to > 4){
            card_to = middle_cards[to - 5];
            if (card_to == "") { return; } // Trying to place card on empty middle pile
        }
        else{
            card_to = cards_showing[client.ClientID, to];
            if (card_to != ""){
                return; // Trying to place card on non empty pile
            }
        }

        Debug.Log("CLIENT: Spit move card");
        client.SendMessage(MoveCardSpitServer.Build(client.ClientID, from, to, cards_showing[client.ClientID, from], card_to, client.RID), true);
    }
}

public static class SpitClientPacketHandler{
    public static void Register(Dictionary<string, Func<string, SpitGameClient, bool>> PacketActions){
        //PacketActions["20"] = ResponseConfirm;
        PacketActions["220"] = OnUpdate;
    }

    public static bool OnUpdate(string packet, SpitGameClient client){
        SpitUpdateClient data = new SpitUpdateClient(packet);

        client.cards_showing = new string[,] {{data.player_cards[0], data.player_cards[1], data.player_cards[2], data.player_cards[3], data.player_cards[4]},
        {data.player_cards[5], data.player_cards[6], data.player_cards[7], data.player_cards[8], data.player_cards[9]}};

        client.cards_r = data.cards_r;

        client.middle_cards = data.middle_cards;

        client.message = data.message;

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