using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class ServerPlayer
{
    public Socket Handler;

    public string name;

    public int ID;

    public byte[] buffer = new byte[1024];
    public StringBuilder sb = new StringBuilder();

    public ServerPlayer(Socket handler){
        Handler = handler;
    }

    public void Reset(){
        buffer = new byte[1024];
        sb = new StringBuilder();
    }
}