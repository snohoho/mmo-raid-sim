using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using NativeWebSocket;
using UnityEngine.UI;
using System;
using static UnityEngine.UI.GridLayoutGroup;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Net;


public class ExclusiveDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
{
    private Dictionary<K, V> dictionary;
    public int Count
    {
        get
        {
            lock (DLock)
            {
                return dictionary.Count;
            }
        }
    }
    object DLock;
    public bool ContainsKey(K key)
    {
        lock (DLock)
        {
            return dictionary.ContainsKey(key);
        }
    }
    public ExclusiveDictionary()
    {
        dictionary = new Dictionary<K, V>();
        DLock = new System.Object();
    }
    public V this[K key]
    {
        get
        {
            lock (DLock)
            {
                if (this.dictionary.ContainsKey(key))
                {
                    return this.dictionary[key];
                }
                return default(V);
            }
        }
        set
        {
            lock (DLock)
            {
                this.dictionary[key] = value;
            }
        }
    }
    public bool Remove(K key)
    {
        lock (DLock)
        {
            if (dictionary.ContainsKey(key))
            {

                dictionary.Remove(key);
                return true;
            }
            return false;
        }
    }
    public void Clear()
    {
        lock (DLock)
        {
            this.dictionary.Clear();
        }
    }
    public void Add(K key, V value)
    {
        lock (DLock)
        {
            dictionary.Add(key, value);
        }
    }
    public ExclusiveDictionary<K, V> Copy()
    {
        lock (DLock)
        {
            ExclusiveDictionary<K, V> temp = new ExclusiveDictionary<K, V>();
            foreach (KeyValuePair<K, V> x in dictionary)
            {
                temp[x.Key] = x.Value;
            }
            return temp;
        }
    }
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        lock (DLock)
        {
            foreach (KeyValuePair<K, V> x in dictionary)
            {
                yield return x;
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
public class ProducerConsumerQueue<T>
{
    List<T> data;
    object ListLock;
    public int Count
    {
        get
        {
            lock (ListLock)
            { return data.Count; }
        }
    }

    public void Append(T value)
    {
        lock (ListLock)
        {
            data.Add(value);
        }
    }

    public T Consume()
    {
        if (Count > 0)
        {
            lock (ListLock)
            {
                T temp = data[0];
                data.RemoveAt(0);
                return temp;
            }
        }
        return default(T);
    }



    public bool IsEmpty()
    {
        lock (ListLock)
        {
            return data.Count == 0;
        }
    }
    public T Peek()
    {
        lock (ListLock)
        {
            return data[data.Count - 1];
        }
    }
    public ProducerConsumerQueue()
    {
        data = new List<T>();
        ListLock = new object();
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="V"></typeparam>


public class ExclusiveString : IEnumerable<char>
{
    public bool IsDirty = false;
    private string data;
    private object SLock;
    public char this[int k]
    {
        get { lock (SLock) { return data[k]; } }
    }
    public string Str
    {
        get { lock (SLock) { IsDirty = false; return data; } }

    }
    public int Length
    {
        get { lock (SLock) { return data.Length; } }
    }

    public string GetData()
    {
        lock (SLock)
        {
            return data.Clone().ToString();
        }
    }
    public void SetData(string s)
    {
        lock (SLock)
        {
            IsDirty = true;
            data = s;
        }
    }

    public ExclusiveString()
    {
        data = "";
        SLock = new System.Object();
    }
    public static ExclusiveString Parse(string s)
    {
        ExclusiveString temp = new ExclusiveString();
        temp.SetData(s);
        return temp;
    }

    public static ExclusiveString operator +(ExclusiveString a, ExclusiveString b)
    {
        ExclusiveString temp = new ExclusiveString();
        temp.SetData(a.GetData() + b.GetData());
        return temp;
    }
    public static ExclusiveString operator +(ExclusiveString a, string b)
    {
        ExclusiveString temp = new ExclusiveString();
        lock (a.SLock)
        {
            temp.SetData(a.GetData() + b);
        }
        return temp;
    }
    public override string ToString()
    {
        lock (SLock)
        {
            return data;
        }
    }
    public void Append(string s)
    {
        lock (SLock)
        {
            data += s;
        }
    }
    public string ReadAndClear()
    {
        string temp = "";
        lock (SLock)
        {
            temp = string.Copy(data);
            data = "";
            return temp;
        }
    }
    public void Trim()
    {
        lock (SLock)
        {
            data = data.Trim();
        }
    }
    public IEnumerator<char> GetEnumerator()
    {
        lock (SLock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                yield return data[i];
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

//GenCore is the new Connection 2
//We no longer need a seperate threaded socket + connection class.
//GenCore has a seperate variable for server (wss) and for client (clientWS)
public class GenCore : WebSocketBehavior
{
    GenericCore_Web owner;
    public int ConnectionID;
    public bool Confirmed;
    public System.DateTime StartCall;
    public float Latency;
    public bool Closing;
    public NativeWebSocket.WebSocket clientWS;
    public ProducerConsumerQueue<string> msg = new ProducerConsumerQueue<string>();
    public bool FailedToConnect = false;

    public async void StartClient()
    {
            FailedToConnect = false;
            //owner.IsServer = false;
            //owner.IsClient = true;
            owner.ScreenConsole("ws://" + owner.IP + ":" + owner.PortNumber + "/");
            clientWS = new NativeWebSocket.WebSocket("ws://"+owner.IP+":"+owner.PortNumber+"/");
            

            clientWS.OnOpen += () =>
            {
                Debug.Log("Starting Connection!");
            };

            clientWS.OnError += (e) =>
            {
                Debug.Log("Error! " + e);
                FailedToConnect = true;
             
            };

            clientWS.OnClose += (e) =>
            {
                Closing = true;
                Debug.Log("Connection closed!");
            };

            clientWS.OnMessage += (bytes) =>
            {
                // Reading a plain text message
                var message = System.Text.Encoding.UTF8.GetString(bytes);   
                //owner.TCPHandleMessage(this, message);
                if (message.Length > 0)
                {
                    this.msg.Append((String)message.Clone());
                }
            };
        await clientWS.Connect();
    }

    public GenCore(GenericCore_Web o)
    {
        Confirmed = false;
        Closing = false;
        owner = o;
    }
    protected override void OnOpen()
    {
        if (owner.IsListening && (owner.MaxConnections ==0 || owner.Connections.Count < owner.MaxConnections))
        {      
            //base.OnOpen();
            this.Send("ID#" + owner.conCounter + "#" + owner.AppNumber + "\n");
            ConnectionID = owner.conCounter;
            owner.conCounter++;
            StartCall = System.DateTime.Now;
            owner.conLimbo.Append(this);
        }
        else
        {
            this.SendMsg("DISCON");
        }
    }
    protected override void OnError(ErrorEventArgs e)
    {
        base.OnError(e);
        Debug.Log("ERROR: " + e.Message);
    }
    protected override void OnClose(CloseEventArgs e)
    {   base.OnClose(e);
        Closing = true;
        owner.ScreenConsole("Closing? ");
        
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            if (e.Data.StartsWith("ID#"))
            {
                if (int.Parse(e.Data.Split('#')[2]) == owner.AppNumber)
                {
                    ConnectionID = int.Parse(e.Data.Split('#')[1]);
                    Confirmed = true;
                    owner.Connections.Add(ConnectionID, this);
                }
            }
            //this is necessary to communicate between threads.
            msg.Append(e.Data);
        }
        catch (Exception ex)
        {
           Debug.Log("OnMessage triggered following exception: " + ex.ToString());
        }
    }
    public void SendMsg(string msg)
    {
        if (owner.IsServer)
        {
            try
            {
                SendAsync((string)msg.Clone(), (x) => { });
            }
            catch(InvalidOperationException e)
            {
                Debug.Log("Warning: Web Socket already closed: " + e.ToString());
            }
        }
        else
        {
            clientWS.SendText((string)msg.Clone()) ;
        }
    }
}

public class GenericCore_Web : MonoBehaviour
{
    public Text OutputConsole;
    public int MaxConsoleLogSize = 1024;
    public bool UsingUDP = false;
    public bool IsListening;
    public bool IsServer;
    public bool IsClient;
    public bool IsConnected;
    public ExclusiveDictionary<int, GenCore> Connections;
    public int AppNumber;
    public int conCounter;
    public string IP;
    public int PortNumber;
    public int MaxConnections;
    public static string SystemLog = "";
    public float MasterTimer = .05f;
    public int LocalConnectionID = -10;
    public int CycleTrigger = 3;
    int CycleCounter = 0;
    protected WebSocketServer wss;
    NativeWebSocket.WebSocket websocket;
    public ProducerConsumerQueue<GenCore> conLimbo = new ProducerConsumerQueue<GenCore>();


    // Start is called before the first frame update
    public void Start()
    {
        conCounter = 0;
        Connections = new ExclusiveDictionary<int, GenCore>();
        if (IP == "")
        {
            IP = "127.0.0.1";
        }
        if (PortNumber == 0)
        {
            PortNumber = 9000;
        }
        if (CycleTrigger == 0)
        {
            CycleTrigger = 3;
        }
    }

    public void ScreenConsole(String s)
    {
        /*if (OutputConsole != null)
        {
            OutputConsole.text += s + "\n";
        }*/
    }
    private void FixedUpdate()
    {
        //Get the message from the websocket thread to the unity side.
        //Uses Consumer/Producer Queue.
        foreach (KeyValuePair<int, GenCore> pair in Connections)
        {
            string temp = "";
            while (pair.Value.msg.Count > 0)
            {
                temp += pair.Value.msg.Consume();
            }
            TCPHandleMessage(pair.Value, temp);
        }
        //Did any connections close?
        //Server Only
        List<int> badC = new List<int>();
        if (IsServer)
        {
            foreach (KeyValuePair<int, GenCore> pair in Connections)
            {
                if (pair.Value.Closing)
                {
                    badC.Add(pair.Key);
                }
            }
            foreach (int i in badC)
            {
                Disconnect(i);
            }
        }
        //Are you the client?  Are you closing?
        if (IsClient && Connections.ContainsKey(0) && Connections[0].Closing)
        {
            Disconnect(0);
        }
        //Necessary for non-webgl to manage the websocket.
        if (IsClient && Connections.ContainsKey(0) && !Connections[0].Closing)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            Connections[0].clientWS.DispatchMessageQueue();
#endif
        }
        //Every 100 udpates send heartbeat. 
        if (CycleCounter % 100 == 0)
        {
            if (IsServer)
            {
                wss.WebSocketServices.Broadcast("OK\n");
            }
        }
        //Call On Slow Update for Netcore and LobbyManager
        OnSlowUpdate();
    }

    // Update is called once per frame
    protected void Update()
    {
    }
    private async void OnApplicationQuit()
    {
        if (IsClient && IsConnected)
        {   //Close client
            await Connections[0].clientWS.Close();
        }
        if (IsServer)
        {
            try
            {
                //Close server
                StartCoroutine(DisconnectServer());
            }
            catch
            { }
        }
    }



    //UI functions
    public void StartServer()
    {
        ScreenConsole("STARTING SERVER");
        if(IsConnected)
        {
            ScreenConsole("ABORTING SERVER START!!! WE think we are already connected!");
            return;
        }
        try
        {
            //Server uses the websocket shart wss variable.
            IsServer = true;
            Debug.Log("Started server @" + "ws://" + IP + ":" + PortNumber);
            ScreenConsole("Started server @" + "ws://" + IP + ":" + PortNumber);
#if PLATFORM_STANDALONE_LINUX
            //wss = new WebSocketServer(PortNumber, false);
            wss = new WebSocketServer(IPAddress.Parse(IP), PortNumber, false);
            //IP = "127.0.0.1";
            //wss = new WebSocketServer("ws://" + IP + ":" + PortNumber);
#else
            wss = new WebSocketServer("ws://" + IP + ":" + PortNumber);
#endif
            wss.Start();
            Debug.Log("Server Started!");
            IsServer = true;
            IsConnected = true;
            IsListening = true;
            LocalConnectionID = -1;
#if !PLATFORM_STANDALONE_LINUX
            wss.ReuseAddress = true;
#endif
            wss.AddWebSocketService<GenCore>("/", () => new GenCore(this) );
            Application.targetFrameRate = 30;
            StartCoroutine(SlowUpdate());
        }
        catch (Exception e )
        {
            Debug.Log("ERROR OCCURED ON START SERVER: "+e.ToString());
            ScreenConsole("ERROR OCCURED ON START SERVER: " + e.ToString());
        }
    }
    public IEnumerator StartClient()
    {
        //Client uses the native websocket client 
        //Gencore has two different types of websockets init.
        Application.targetFrameRate = 60;
        if (!IsConnected | !IsClient)
        {
            this.IsClient = true;
            GenCore temp = new GenCore(this);
            try
            {
                 temp.StartClient();
            }
            catch (Exception e)
            {
                temp = null;
                ScreenConsole("Starting client threw: " + e.Message);
            }
            int count = 0;
            while(temp!=null&& temp.clientWS != null && temp.clientWS.State != NativeWebSocket.WebSocketState.Open)
            {     
                yield return new WaitForSeconds(.1f);
                count++;
                if (temp.FailedToConnect || count > 15)
                {
                    temp = null;
                    break;
                }
            }
            if (temp != null)
            {
                IsClient = true;
                IsConnected = true;
                IsServer = false;
                Connections.Add(0, temp);
                StartCoroutine(SlowUpdate());
                yield return new WaitUntil(() => LocalConnectionID != -10);
                StartCoroutine(OnClientConnect(LocalConnectionID));
            }
            else if(FindObjectOfType<LobbyManager2>()== null)
            {
                Disconnect(0);
            }
        }   
    }

    public void Send(String msg, int id)
    {
        if(Connections.ContainsKey(id))
        {
            Connections[id].SendMsg(msg);
        }
    }

    //Handles all of the functionality for Generic core recieving a message.
    public async void TCPHandleMessage(GenCore Con, String Data)
    {
        string temp = Data;
        char[] BadChars = { '<', '>' };
        temp = temp.Trim(BadChars);
        string[] commands = temp.Split('\n');
        foreach (string c in commands)
        {
            if (c.StartsWith("ID#"))
            {  
                if (int.Parse(c.Split('#')[2]) == AppNumber)
                {
                    Con.ConnectionID = int.Parse(c.Split('#')[1]);
                    Con.Confirmed = true;
                    if (IsServer)
                    {                  
                        //Connections.Add(Con.ConnectionID, Con);                 
                        StartCoroutine(OnClientConnect(Con.ConnectionID));
                    }
                    if (IsClient)
                    {
                        LocalConnectionID = Con.ConnectionID;
                        Con.SendMsg(c+"\n");

                    }
                }
            }
            if (c.StartsWith("DISCON") && IsClient)
            {
                
                if (!Con.Closing)
                {                 
                    Con.Closing = true;
                    await Con.clientWS.Close();     
                }
            }
            else if (c.StartsWith("OK"))
            {
                if (IsServer)
                {
                    Con.Latency = (float)(System.DateTime.Now - Con.StartCall).TotalSeconds;
                    Con.StartCall = System.DateTime.Now;
                    Con.SendMsg("LAT#" + Con.Latency + "\n");
                }
                if(IsClient)
                {
                    Con.SendMsg("OK\n");
                }
            }
            if(c.StartsWith("LAT#"))
            {
                Con.Latency = float.Parse(c.Split('#')[1].Trim());
            }
            else
            {
                //Pass it to netcore or lobby manager.
                OnHandleMessages(c);
            }
        }
    }
    public async void  Disconnect(int id)
    {
        StartingDisconnect(id);
        
        

        if(IsClient)
        {
            if (Connections.ContainsKey(0) && !Connections[0].Closing)
            {
                await Connections[0].clientWS.Close();
            }
            Connections.Clear();
            IsConnected = false;
            IsClient = false;
            try
            {
                StartCoroutine(OnClientDisconnect(id));
                OnClientDisconnectCleanup(id);
            }
            catch (Exception ex) 
            {
                Debug.Log("Lobby Manager Already Close! " + ex.ToString());
            }
        }
        if(IsServer)
        {
           if(Connections.ContainsKey(id))
            {
                if (!Connections[id].Closing)
                {
                    ScreenConsole("Sending command to close!");
                    Connections[id].SendMsg("DISCON\n");

                }
                else
                {
                    Connections.Remove(id);
                    StartCoroutine(OnClientDisconnect(id));
                    OnClientDisconnectCleanup(id);
                }
            }
        }
    }


    private IEnumerator SlowUpdate()
    {
       /* while(IsServer)
        {
            while(conLimbo.Count>0)
            {
                GenCore temp = conLimbo.Consume();
                temp.SendMsg("ID#" + temp.ConnectionID + "#" + AppNumber + "\n");
            }
            yield return new WaitForSeconds(.1f);
        }*/
        yield return new WaitForSeconds(1f);
        //Slow update no longer used.
    }
    public IEnumerator DisconnectServer()
    {
        StartCoroutine(OnServerDisconnect());
        yield return new WaitForSeconds(.1f);
        foreach (KeyValuePair<int,GenCore> pair in Connections)
        {
            Disconnect(pair.Key);
        }
        yield return new WaitForSeconds(1);
        if (IsServer && IsConnected)
        {
            wss.Stop();
        }
        IsServer = false;
        IsConnected = false;
        OnServerDisconnectCleanup();
    }


    //UI FUNCTIONS
    /// <summary>
    /// UI call back-  Needs to be dynamic string from input field
    /// Will ensure it is a good IP address then set it.
    /// Otherwise, default is home IP address.
    /// </summary>
    /// <param name="ipAddr"></param>
    public void UI_SetIP(string ipAddr)
    {
        try
        {
            IPAddress.Parse(ipAddr.Trim());
            IP = ipAddr;
        }
        catch
        {
            IP = "127.0.0.1";
        }
    }
    /// UI callback - Needs to by dynamic string from an input field.
    /// Will ensure it is a good integer, then set the Port number.
    /// Otherwise, default is 9000!
    /// </summary>
    /// <param name="n"></param>
    public void UI_SetPort(string n)
    {
        try
        {
            PortNumber = int.Parse(n.Trim());
        }
        catch
        {
            PortNumber = 9000;
        }
    }
    /// <summary>
    /// UI callback to start the client.
    /// </summary>
    public void UI_StartClient()
    {
        StartCoroutine(StartClient());
    }
    /// <summary>
    /// UI Callback to Start the server.
    /// </summary>
    public void UI_StartServer()
    {
        StartServer();
    }

    public virtual void UI_Quit()
    {
        if(IsClient)
        {
            Disconnect(0);
        }
        else if(IsServer)
        {
            StartCoroutine(DisconnectServer());       
            if (GameObject.FindObjectsOfType<LobbyManager2>() != null)
            {
                LobbyAgentManager [] lbm = FindObjectsOfType<LobbyAgentManager>(default);
                foreach(LobbyAgentManager l in lbm)
                {
                    if(l.IsLocalPlayer)
                    {
                        l.SendCommand("GSTARTED", true.ToString());
                        l.SendCommand("GAMEOVER", "1");
                        StartCoroutine(l.slowProcessKill());
                    }
                }
                //GameObject.FindAnyObjectByType<LobbyManager2>().Disconnect(0);
                //Application.Quit();
            }
            else
            {
              //Do nothing... DisconnectServer kicks back to Scene 0.
            }
        }
    }


    //------------------------------------------------------------Virtual Functions ------------------------------------

    /// <summary>
    /// Virtual functions OnClientConnect
    /// Sends in the ID of the new client
    /// Gives the programmer an option to 
    /// perform data.
    /// </summary>
    /// <param name="id"></param>
    ///<returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnClientConnect(int id)
    {
        yield return new WaitForSeconds(.1f);
    }

    /// <summary>
    /// Virtual function to read messages coming from the 
    /// Socket.
    /// Will be called after the GenericNetworkCore 
    /// Has read through them first.
    /// Will already be parsed to individual commands
    /// (Therefore, called multiple times)
    /// </summary>
    /// <param name="commands"></param>
    public virtual void OnHandleMessages(string commands)
    {
        Logger("Received a message: " + commands);
    }

    /// <summary>
    /// Allows the programmer to insert code on SlowUpdate
    /// NOTE!  You cannot while true inside this function!
    /// </summary>
    public virtual void OnSlowUpdate()
    {

    }

    /// <summary>
    /// Allows the programmer to insert code when the server is started.
    /// Sever values shoudl be initialized and set.
    /// </summary>
    /// <returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnServerStart()
    {
        yield return new WaitForSeconds(.1f);
    }
    /// <summary>
    /// Allows the programmer to insert code when a client disconnects.
    /// This will be called on both the client and the server.
    /// Note this will be called before disconnect happens.
    /// </summary>
    /// <param name="ID"></param>
    /// <returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnClientDisconnect(int id)
    {
        ScreenConsole("Before");
        yield return new WaitForSeconds(.1f);
        ScreenConsole("After");
    }

    /// <summary>
    /// Will be called when the server disconnects. 
    /// Allows the programmer to inject cleanup code.
    /// Note this will be called before disconnect happens.
    /// </summary>
    /// /// <returns>IEnumerator in case you have to wait for something to initialize.</returns>
    public virtual IEnumerator OnServerDisconnect()
    {
        yield return new WaitForSeconds(.1f);
    }

    /// <summary>
    /// This funcion is called after the disconnect has occured for a client.
    /// </summary>
    /// <param name="id">The id of the connection that was deleted.  Note the connection is already deleted.</param>
    public virtual void OnClientDisconnectCleanup(int id)
    {

    }

    /// <summary>
    /// This function is called after the server has disonnected and shut down.
    /// </summary>
    public virtual void OnServerDisconnectCleanup()
    {

    }

    /// <summary>
    /// This function will log all output.  
    /// All logs are added to a static public variable SystemLog that can be sent out to an output.
    /// </summary>
    /// <param name="msg">The messge to add to the log.</param>
    public static void Logger(string msg)
    {
     
        /*if (SystemLog.Length > GenericCore_Web.MaxConsoleLogSize)
        {
            SystemLog = "";
        }
        //ScreenConsole(System.DateTime.Now.ToString() + ": " + msg);
        SystemLog += System.DateTime.Now.ToString() + ": " + msg + "\n";*/
    }

    public virtual IEnumerator MenuManager()
    {
        yield break;
    }

    /// <summary>
    /// This function will be called when IsDisconnecting is set to true.
    /// There can be different elements that cause this, error, player quitting, etc.
    /// This is an oppurtunity for the programmmer to playce a UI or Panel screen to hide any messy
    /// visual artifacts of the disconnect.
    /// </summary>
    public virtual void StartingDisconnect(int id)
    {

    }


}
