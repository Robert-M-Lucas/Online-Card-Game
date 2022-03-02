using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Client
{
    public Thread mainThread;
    public Thread RecieveThread;
    public Thread SendThread;

    public string hostIp;
    public IPAddress HostIpA;
    public IPEndPoint RemoteEP;
    public Socket Handler;
    byte[] buffer = new byte[1024];
    StringBuilder sb = new StringBuilder();

    public string currentMessage; // Current state output

    public string otherPlayerName = "";

    public NetworkManager networkManager;

    ConcurrentQueue<string> ContentQueue = new ConcurrentQueue<string>();
    ConcurrentQueue<string> SendQueue = new ConcurrentQueue<string>();

    public ConcurrentDictionary<int, string> RequireResponse = new ConcurrentDictionary<int, string>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();

    public int RID = 1; // Response ID

    Dictionary<string, Func<string, Client, bool>> PacketActions = new Dictionary<string, Func<string, Client, bool>>();
    ClientPacketResponses packetResponses;

    public int ClientID = -1;

    public GameParentClient CurrentGame = null;
    Stopwatch CurrentGameStopwatch = new Stopwatch();

    public CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    public Client(NetworkManager _netManager){
        networkManager = _netManager;
    }

    public string IdToPlayerName(int id){
        if (ClientID == id) { return networkManager.Username; }
        else { return otherPlayerName; }
    }

    public void Start(string host_ip = "127.0.0.1"){
        packetResponses = new ClientPacketResponses(this, PacketActions);
        Debug.Log(host_ip);
        hostIp = host_ip;
        mainThread = new Thread(Join);
        mainThread.Start();
        RecieveThread = new Thread(RecieveLoop);
        RecieveThread.Start();
        SendThread = new Thread(SendLoop);
        SendThread.Start();

    }

    public void Join()
    {

        Debug.Log("CLIENT: Connecting");

        currentMessage = "Connecting...";

        byte[] bytes = new byte[1024];

        try
        {
            HostIpA = IPAddress.Parse(hostIp);
            RemoteEP = new IPEndPoint(HostIpA, 8108);

            Handler = new Socket(HostIpA.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Handler.Connect(RemoteEP);

            Debug.Log("CLIENT: Socket connected to {0}" +
                Handler.RemoteEndPoint.ToString());

            currentMessage = "Socket connected to {0}" +
                Handler.RemoteEndPoint.ToString();
  
            Handler.Send(PacketTools.Encode(InitialPacketServer.Build(networkManager.Username, networkManager.version, 0)));

            int bytesRec = Handler.Receive(bytes);

            string rec = PacketTools.DeBuild(Encoding.ASCII.GetString(bytes, 0, bytesRec));
            Debug.Log("CLIENT: Echoed test = {0}" +
                rec);

            currentMessage = "CLIENT: Echoed test = {0}" + rec;

            #region Unused
            // // Error code for version mismatch
            // if (rec.Substring(0, 4) == "9997")
            // {
            //     netManager.QuitLobby("Version doesn't match host: " + rec.Substring(5, rec.Length - 5));
            //     Stop(false);
            //     return;
            // }
            // // Error code for host refusing connection
            // if (rec[4] != '1')
            // {
            //     netManager.QuitLobby("Host server refused connection");
            //     Stop(false);
            //     return;
            // }

            // Creates all other players on the network
            // string players_string = rec.Substring(5, rec.Length - 10); // Remove EOF

            // foreach (string player_data_string in players_string.Split(';'))
            // {
            //     if (player_data_string.Length < 1) { break; }

            //     Players.Add(new PlayerNetworkData(player_data_string.Split(':')[0], int.Parse(player_data_string.Split(':')[1]), null, false));
            // }

            // // Gets UID from server
            // UID = Players[0].UID;
            #endregion

            if (PacketTools.PacketToUID(rec) == InitalPacketClient.UID){
                Debug.Log("CLIENT: connection successful");
                currentMessage = "Connection successful";

                Handler.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null);
            }
            else
            {
                Debug.Log("CLIENT: Kicked");
                currentMessage = "Kicked";
                networkManager.QuitLobby(new KickPacketClient(rec).reason);
            }


        }
        catch (ThreadAbortException){

        }
        catch (Exception e)
        {
            networkManager.QuitLobby(e.ToString());
            Stop();
        }

        // Starts recieving data from server
        //Handler.BeginReceive(buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), Players[0]);
    }

    public void SendMessage(string message, bool require_response){
        SendQueue.Enqueue(message);

        if (require_response){
            RequireResponse[RID] = message;
            RequiredResponseQueue.Enqueue(RID);
            RID++;
        }
    }

    void SendLoop()
    {
        try{
        while (true){
            // Send messages
            if (!SendQueue.IsEmpty){
                string to_send;

                if (SendQueue.TryDequeue(out to_send)){
                    Debug.Log("CLIENT: Sent " + to_send);
                    Handler.Send(PacketTools.Encode(to_send));
                }
            }
            // Resend messages not confirmed to be recieved by server
            else if (!RequiredResponseQueue.IsEmpty){
                int rid;
                if (RequiredResponseQueue.TryDequeue(out rid)){
                    if (RequireResponse.ContainsKey(rid)){
                        string to_send = RequireResponse[rid];
                        Debug.Log("CLIENT: Sent " + to_send);
                        Handler.Send(PacketTools.Encode(to_send));
                        RequiredResponseQueue.Enqueue(rid);
                    }
                }
            }

            Thread.Sleep(5);
        }
        }
        catch (Exception e){
            networkManager.FatalException(e.ToString());
        }
    }

    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;
 
        int bytesRead = Handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            sb.Append(Encoding.UTF8.GetString(
                buffer, 0, bytesRead));

            content = sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {

                foreach (string subcontent in content.Split(new[] { "<EOF>" }, StringSplitOptions.None))
                {
                    if (subcontent.Length == 0) { continue; }

                    ContentQueue.Enqueue(subcontent);

                    Debug.Log("CLIENT: Recieved: " + subcontent);
                }
                buffer = new byte[1024];
                sb = new StringBuilder(); // Reset buffers

                Handler.BeginReceive(buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), null); // Listen again
            }
            else
            {
                // Not all data received. Get more.  
                 Handler.BeginReceive(buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), null); // Listen again
            }
        }
        else
        {
             Handler.BeginReceive(buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), null); // Listen again
        }
    }

    void RecieveLoop()
    {
        try{
            while (true)
            {
                // Update game every x ms
                if (CurrentGame != null){
                    if (CurrentGame.MsBetweenUpdates != -1){
                        if (!CurrentGameStopwatch.IsRunning){
                            CurrentGameStopwatch.Start();
                        }
                        else if (CurrentGameStopwatch.ElapsedMilliseconds > CurrentGame.MsBetweenUpdates)
                        {
                            CurrentGameStopwatch.Restart();
                            CurrentGame.Update();
                        }
                    }
                }

                // If nothing to recieve
                if (ContentQueue.IsEmpty){Thread.Sleep(5); continue;}

                string content;
                if (!ContentQueue.TryDequeue(out content)){ continue; }

                int rid = PacketTools.RequireResponse(content);

                // Confirm recieved
                if (rid != 0) { 
                    SendMessage(ResponseConfirmPacket.Build(rid), false); Debug.Log("CLIENT: CONFIRMED MESSAGE"); 
                    if (RecievedRIDs.Contains(rid) != -1){
                        continue; // Already recieved this message
                    }
                    else{
                        RecievedRIDs.Add(rid);
                    }
                }

                string UID =  PacketTools.PacketToUID(content);

                // Check if packet can be handled by current game
                if (CurrentGame != null){
                    if (CurrentGame.HandlePacket(UID, content)){
                        continue;
                    }
                }

                // Handle packet
                if (PacketActions.ContainsKey(UID)){
                    PacketActions[UID](content, this);
                }
                else{
                    Debug.LogError("CLIENT: Unhandled packet: " + content + " " + UID + " " + rid);
                }

            }
        }
        catch (Exception e){
            networkManager.FatalException(e.ToString());
        }
    }

    public void Stop(){
        try{Handler.Shutdown(SocketShutdown.Both);}catch{}
        try{mainThread.Abort();}catch{}
        try{RecieveThread.Abort();}catch{}
        try{SendThread.Abort();}catch{}
    }
}
