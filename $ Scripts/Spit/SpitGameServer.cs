using System.Collections;
using System;
using System.Threading;

using System.Collections.Generic;
using UnityEngine;

public class SpitGameServer : GameParentServer
{
    Dictionary<string, Func<string, SpitGameServer, int, bool>> PacketActions = new Dictionary<string, Func<string, SpitGameServer, int, bool>>();

    public Server server;

    public System.Random random = new System.Random();

    public int[,] cards_r; // Player 0 [0 - 4], Player 1 [0 - 4]
    public int cards_in_stack;

    int i = 0;

    public bool gameOver = false;

    public string[,] cards_showing; // 2 x 5 cards

    public string[] middle_cards;

    public int Init = 0;

    public Deck deck;

    public int Wait = -1;

    public string message = "";

    public SpitGameServer(Server _server) : base(_server){
        Debug.Log("Spit server started");
        base.MsBetweenUpdates = 1000;
        server = _server;
        SpitServerPacketHandler.Register(PacketActions);
        Reset();
    }

    public void Reset() {
        deck = new Deck();

        cards_r = new int[,] {{5, 4, 3, 2, 1}, {5, 4, 3, 2, 1}};

        middle_cards = new string[] {"", ""};
        Wait = -1;
        cards_in_stack = 0;
        cards_showing = new string[,] {{deck.RemoveRandomCard(), deck.RemoveRandomCard(), deck.RemoveRandomCard(), deck.RemoveRandomCard(), deck.RemoveRandomCard()},
        {deck.RemoveRandomCard(), deck.RemoveRandomCard(), deck.RemoveRandomCard(), deck.RemoveRandomCard(), deck.RemoveRandomCard()}};
    }

    public override bool HandlePacket(string UID, string packet, int player){
        if (PacketActions.ContainsKey(UID)){
            PacketActions[UID](packet, this, player);
            return true;
        }
        return false;
    }

    public void SendUpdate(){
        server.SendMessage(0, SpitUpdateClient.Build(cards_showing, middle_cards, cards_r, message, server.RID), true);
        server.SendMessage(1, SpitUpdateClient.Build(cards_showing, middle_cards, cards_r, message, server.RID), true);
        
    }

    public void NewMidCards(){
        if (deck.AllCardsRemaining.Count - (2 + cards_r[0,0] + cards_r[0,1] + cards_r[0,2] + cards_r[0,3] + cards_r[0,4] + 
        cards_r[1,0] + cards_r[1,1] + cards_r[1,2] + cards_r[1,3] + cards_r[1,4]) >= 0){
            middle_cards = new string[] {deck.RemoveRandomCard(), deck.RemoveRandomCard()};
        }
        else{
            middle_cards = new string[] {CardTools.GetRandomCard(random), CardTools.GetRandomCard(random)};
            message = "No cards remaining in deck, middle cards are now random!";
        }

        
    }

    public override void Update()
    {
        if (Init != 2){
            if (Init < 1){
                Init++;
            }
            else if (Init == 1) {
                Init = 2; 
                NewMidCards();
                SendUpdate();
            }
        }
        else{
            if (Wait > -1){
                Wait++;
            }
            if (Wait == 5){
                Wait = -1;
                CheckIfMoveAvailable();
                if (Wait == -1) { return; }
                Wait = -1;

                NewMidCards();
                SendUpdate();
                CheckIfMoveAvailable();
                
            }
        }
    }

    public bool CanStack(string card_1, string card_2){
        if (card_1 == "" | card_2 == ""){
            return false;
        }

        card_1 = card_1.Substring(0, card_1.Length-1);
        card_2 = card_2.Substring(0, card_2.Length-1);

        int card_1Index = -1;
        int card_2Index = -1;

        for (int i = 0; i < CardTools.Level.Length; i++){
            if (card_1 == CardTools.Level[i]){
                card_1Index = i;
            }
            if (card_2 == CardTools.Level[i]){
                card_2Index = i;
            }
        }

        if (card_1Index == card_2Index) { return false; }
        if (card_1Index == CardTools.Level.Length - 1 & card_2Index == 0) {return true;}
        if (card_2Index == CardTools.Level.Length - 1 & card_1Index == 0) {return true;}
        if (card_1Index ==  card_2Index + 1) {return true;}
        if (card_2Index ==  card_1Index + 1) {return true;}
        return false;
    }

    public void CheckIfMoveAvailable() {
        for (int player = 0; player < 2; player++){
            for (int player_card = 0; player_card < 5; player_card++){
                for (int middle_card = 0; middle_card < 2; middle_card++){
                    if (CanStack(cards_showing[player, player_card], middle_cards[middle_card])){
                        return;
                    }
                }
            }
        }

        Wait = 0;
    }
}

public static class SpitServerPacketHandler{
    public static void Register(Dictionary<string, Func<string, SpitGameServer, int, bool>> PacketActions){
        //PacketActions["20"] = ResponseConfirm;
        PacketActions["210"] = MoveCard;

    }
    
    public static bool MoveCard(string packet, SpitGameServer server, int player){
        if (server.gameOver){ return true; }

        Debug.Log("SERVER: Spit try move card");

        MoveCardSpitServer data = new MoveCardSpitServer(packet);

        // TODO: More server-side checks to match client
        if (data.card_from == "" | data.player_pile_from > 4){
            return true;
        }

        if (data.card_from != server.cards_showing[player, data.player_pile_from] | server.cards_r[player, data.player_pile_from] == 0){
            return true; // Source card changed
        }

        if (data.pile_to < 5){
            if (data.card_to != "" | server.cards_showing[player, data.pile_to] != ""){
                return true; // Destination not empty (Moving player cards)
            }
            else{
                server.cards_r[player, data.pile_to] = 1;
                server.cards_showing[player, data.pile_to] = server.cards_showing[player, data.player_pile_from];

                server.cards_r[player, data.player_pile_from]--;

                if (server.cards_r[player, data.player_pile_from] > 0) {
                    server.cards_showing[player, data.player_pile_from] = server.deck.RemoveRandomCard();
                }
                else{
                    server.cards_showing[player, data.player_pile_from] = "";
                }
            }
        }
        else {
            if (data.card_to != server.middle_cards[data.pile_to-5] | server.middle_cards[data.pile_to-5] == ""){
                return true; // Destination changed or middle pile is empty
            }
            else{
                if (!server.CanStack(data.card_from, data.card_to)){ return true; }

                server.middle_cards[data.pile_to - 5] = server.cards_showing[player, data.player_pile_from];

                server.cards_r[player, data.player_pile_from]--;

                if (server.cards_r[player, data.player_pile_from] > 0) {
                    server.cards_showing[player, data.player_pile_from] = server.deck.RemoveRandomCard();
                }
                else{
                    server.cards_showing[player, data.player_pile_from] = "";
                }
            }
        }

        server.CheckIfMoveAvailable();

        for (int p = 0; p < 2; p++){
            bool win = true;
            for (int c = 0; c < 5; c++){
                if (server.cards_r[p, c] != 0){
                    win = false;
                    break;
                }
            }

            if (win){
                server.gameOver = true;
                server.message = server.server.Players[p].name + " wins!";
                server.SendUpdate();
                return true;
            }
        }

        server.SendUpdate();
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