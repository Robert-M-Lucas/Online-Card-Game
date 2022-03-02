using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameParent
{
    public int MsBetweenUpdates = -1; // -1 = no updates

    public virtual void Update(){

    } 
}

public class GameParentServer : GameParent{
    public GameParentServer(Server server){

    }

    public virtual bool HandlePacket(string UID, string packet, int player){
        return false;
    }
}

public class GameParentClient: GameParent{
    public GameParentClient(Client client){
        
    }

    public virtual bool HandlePacket(string UID, string packet){
        return false;
    }
}