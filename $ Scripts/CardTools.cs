using System;
using System.Linq;
using System.Collections.Generic;

public static class CardTools{
    public static string[] Suits = new string[] {"♣", "♠", "♥", "♦"};
    public static string[] Level = new string[] {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};

    public static string GetRandomCard(Random r){
        return Level[r.Next(13)] + Suits[r.Next(4)];
    }
}

public class Deck{

    Random r = new Random();
    public List<string> AllCardsRemaining = new List<string>();

    public List<string> RemovedCards = new List<string>();

    public Deck(int decks = 1){
        for (int i = 0; i < decks; i++){
            foreach (string suit in CardTools.Suits){
                foreach (string level in CardTools.Level){
                    AllCardsRemaining.Add(level + suit);
                }
            }
        }
    }

    public string RemoveRandomCard(){
        int index = r.Next(AllCardsRemaining.Count);
        string card = AllCardsRemaining[index];

        AllCardsRemaining.RemoveAt(index);
        RemovedCards.Add(card);

        return card;
    }

    public void ReturnCards(int num = -1){
        if (num == -1){
            while (RemovedCards.Count > 0){
                AllCardsRemaining.Add(RemovedCards[RemovedCards.Count-1]);
                RemovedCards.RemoveAt(RemovedCards.Count-1);
            }
        }
        else{
            for (int i = 0; i < num; i++){
                AllCardsRemaining.Add(RemovedCards[RemovedCards.Count-1]);
                RemovedCards.RemoveAt(RemovedCards.Count-1);
            }
        }
    }
}