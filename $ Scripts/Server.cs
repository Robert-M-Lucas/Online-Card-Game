using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Server
{
    public Socket Handler;
    Socket listener;

    public NetworkManager networkManager;

    public int scene = -1;

    public Thread mainThread;
    public Thread RecieveThread;
    public Thread SendThread;

    public ServerPlayer[] Players = new ServerPlayer[2];

    public string currentMessage; // Current info message

    public int UsersConnected = 0;

    ConcurrentQueue<Tuple<int, string>> ContentQueue = new ConcurrentQueue<Tuple<int, string>>();
    ConcurrentQueue<Tuple<int, string>> SendQueue = new ConcurrentQueue<Tuple<int, string>>();

    public ConcurrentDictionary<int, Tuple<int, string>> RequireResponse = new ConcurrentDictionary<int, Tuple<int, string>>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();

    public int RID = 1;

    Dictionary<string, Func<string, Server, int, bool>> PacketActions = new Dictionary<string, Func<string, Server, int, bool>>();
    
    public CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    public GameParentServer CurrentGame = null;
    public Stopwatch CurrentGameStopwatch = new Stopwatch();

    public Server(NetworkManager netManager){
        networkManager = netManager;

        ServerPacketResponses.Register(PacketActions);
    }

    public void Start() {
        mainThread = new Thread(AcceptClients);
        mainThread.Start();
        RecieveThread = new Thread(RecieveLoop);
        RecieveThread.Start();
        SendThread = new Thread(SendLoop);
        SendThread.Start();
    }

    public void SwitchGame(int Scene){
        CurrentGame = SceneToGame.Server(Scene, this);
        SwitchScene(Scene);
    }

    public void SwitchScene(int new_scene){
        SendMessage(0, ScenesSwitchPacket.Build(new_scene, RID), true);
        SendMessage(1, ScenesSwitchPacket.Build(new_scene, RID), true);
        scene = new_scene;
    }

    // Accept client
    void AcceptClients()
    {
        Debug.Log("SERVER: Server Thread Start");

        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8108);

        try
        {
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);

            listener.Listen(100);

            currentMessage = "Waiting for players";

            while (UsersConnected < 2)
            {
                Debug.Log("SERVER: Waiting for a connection...");
                Socket Handler = listener.Accept();

                // Incoming data from the client.    
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = Handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                Debug.Log("SERVER: Text received : {0}" + data);

                InitialPacketServer initPacket = new InitialPacketServer(PacketTools.DeBuild(data));

                // Version mismatch
                if (initPacket.version != networkManager.version){
                    Handler.Send(PacketTools.Encode(KickPacketClient.Build("Wrong Version:\nServer: " + networkManager.version + "   Client (You): " + initPacket.version, 0)));
                    continue;
                }
                
                int p = 0;
                if (Players[0] != null){
                    p++;
                }
                Players[p] = new ServerPlayer(Handler);
                Players[p].name = initPacket.name;
                Players[p].ID = p;
                UsersConnected++;

                if (Players[p].name != networkManager.Username) { networkManager.OpponentName = Players[p].name; }
                
                SendMessage(p, InitalPacketClient.Build(RID, p), true);

                currentMessage = "Player " + p.ToString() + " (" + initPacket.name + ")" + " connected";
                Debug.Log(currentMessage);

                Handler.BeginReceive(Players[p].buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), Players[p]);
            }
            Debug.Log("SERVER: Both clients connected");
            // SendMessage(0, ScenesSwitchPacket.Build(1, RID), true);
            // SendMessage(1, ScenesSwitchPacket.Build(1, RID), true);

            // Send information about clients to eachother
            SendMessage(0, OtherPlayerInfoPacketClient.Build(Players[1].name, RID), true);
            SendMessage(1, OtherPlayerInfoPacketClient.Build(Players[0].name, RID), true);
            
        }
        catch (ThreadAbortException){}
        catch (Exception e)
        {
            Debug.Log("ERROR");
            Debug.Log(e.ToString());
            networkManager.QuitLobby(e.ToString());
        }
    }

    public void SendMessage(int ID, string message, bool require_response){
        SendQueue.Enqueue(new Tuple<int, string>(ID, message));

        if (require_response){
            RequireResponse[RID] = new Tuple<int, string>(ID, message);
            RequiredResponseQueue.Enqueue(RID);
            RID++;
        }
    }

    void SendLoop()
    {
        try{
            while (true){
                if (!SendQueue.IsEmpty){
                    Tuple<int, string> to_send;
                    if (SendQueue.TryDequeue(out to_send)){
                        Debug.Log("SERVER: Sent " + to_send.Item2);
                        Players[to_send.Item1].Handler.Send(PacketTools.Encode(to_send.Item2));
                    }
                }
                else if (!RequiredResponseQueue.IsEmpty){
                    int rid;
                    if (RequiredResponseQueue.TryDequeue(out rid)){
                        if (RequireResponse.ContainsKey(rid)){
                            Tuple<int, string> to_send = RequireResponse[rid];
                            Debug.Log("SERVER: Sent " + to_send.Item2);
                            Players[to_send.Item1].Handler.Send(PacketTools.Encode(to_send.Item2));
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
 
        ServerPlayer CurrentPlayer = (ServerPlayer)ar.AsyncState;
        Socket handler = CurrentPlayer.Handler;

        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            CurrentPlayer.sb.Append(Encoding.UTF8.GetString(
                CurrentPlayer.buffer, 0, bytesRead));

            // Check for EOF
            content = CurrentPlayer.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {

                foreach (string subcontent in content.Split(new[] { "<EOF>" }, StringSplitOptions.None))
                {
                    if (subcontent.Length == 0) { continue; }

                    Debug.Log("SERVER: Recieved " + subcontent);

                    ContentQueue.Enqueue(new Tuple<int, string>(CurrentPlayer.ID, subcontent));
                }
                CurrentPlayer.Reset(); // Reset buffers

                handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), CurrentPlayer); // Listen again
            }
            else
            {
                // Not all data received. Get more.  
                handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), CurrentPlayer);
            }
        }
        else
        {
            handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), CurrentPlayer);
        }
    }

    void RecieveLoop()
    {
        try{
            while (true)
            {
                // Update current game every x ms
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

                if (ContentQueue.IsEmpty){Thread.Sleep(2); continue;} // Nothing recieved

                Tuple<int, string> content;
                if (!ContentQueue.TryDequeue(out content)){ continue; }

                int rid = PacketTools.RequireResponse(content.Item2);

                // Confirm recieved
                if (rid != 0) { 
                    SendMessage(content.Item1, ResponseConfirmPacket.Build(rid), false); Debug.Log("CLIENT: CONFIRMED MESSAGE"); 
                    if (RecievedRIDs.Contains(rid) != -1){
                        continue; // Already recieved this message
                    }
                    else{
                        RecievedRIDs.Add(rid);
                    }
                }

                string UID = PacketTools.PacketToUID(content.Item2);

                // Try to get current game to handle packet
                if (CurrentGame != null){
                    if (CurrentGame.HandlePacket(UID, content.Item2, content.Item1)){
                        continue;
                    }
                }

                // Handle packet
                if (PacketActions.ContainsKey(UID)){
                    PacketActions[UID](content.Item2, this, content.Item1);
                }
                else{
                    Debug.LogError("SERVER: Unhandled packet: " + content.Item2);
                }

            }
        }
        catch (Exception e){
            networkManager.FatalException(e.ToString());
        }
    }

    public void Stop(){
        try{Handler.Shutdown(SocketShutdown.Both);}catch{}
        try{listener.Shutdown(SocketShutdown.Both); }catch{}
        try{mainThread.Abort();}catch{}
        try{RecieveThread.Abort();}catch{}
        try{SendThread.Abort();}catch{}
        try{Players[0].Handler.Shutdown(SocketShutdown.Both);}catch{}
        try{Players[1].Handler.Shutdown(SocketShutdown.Both);}catch{}
    }
}
