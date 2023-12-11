using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using LitJson;
using System;
using System.IO;

public enum Type
{
    Empty = 0,
    Server,
    Client,
}

public class Item
{
    public string License;
    public string serverIP;
    public string port;

    public Item(string L_index, string IPValue, string Port)
    {
        License = L_index;
        serverIP = IPValue;
        port = Port;
    }
}

public class ServerCheck : NetworkBehaviour
{

    public Type type;

    private NetworkManager manager;
    private KcpTransport kcp;

    private string Path = string.Empty;
    public string server_IP { get; private set; }
    public string server_Port { get; private set; }

    private void Awake()
    {
        if (Path.Equals(string.Empty))
        {
            Path = Application.dataPath + "/License";
        }

        if (!File.Exists(Path))       //폴더 검사
        {
            Directory.CreateDirectory(Path);
        }

        if (!File.Exists(Path + "/License.json"))    //파일 검사
        {
            Default_Data(Path);
        }

        manager = GetComponent<NetworkManager>();
        kcp = (KcpTransport)manager.transport;

    }

    private void Default_Data(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item("2", "127.0.0.1", "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private Type License_Type()
    {
        Type type = Type.Empty;

        try
        {
            string Json_String = File.ReadAllText(Path + "/License.json");
            JsonData itemdata = JsonMapper.ToObject(Json_String);
            string string_Type = itemdata[0]["License"].ToString();
            string string_serverIP = itemdata[0]["serverIP"].ToString();
            string string_Port = itemdata[0]["port"].ToString();

            server_IP = string_serverIP;
            server_Port = string_Port;
            type = (Type)Enum.Parse(typeof(Type), string_Type);

            manager.networkAddress = server_IP;
            kcp.port = ushort.Parse(server_Port);

            return type;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return Type.Empty;
        }
    }

    private void Start()
    {

        type = License_Type();

        if (type.Equals(Type.Server))
        {
            Start_Server();
        }
        else
        {
            Start_Client();
        }
    }

    public void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
        }
        else
        {
            manager.StartServer();
            Debug.Log($"{manager.networkAddress} Start Server");
            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"New Client Connect : {NetworkConnectionToClient.address}");
            };

            NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"New Client Disconnect : {NetworkConnectionToClient.address}");
            };
        }
    }

    public void Start_Client()
    {
        Debug.Log($"{manager.networkAddress} : Start Client");
        manager.StartClient();
    }

    private void OnApplicationQuit()
    {
        if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }

        if (NetworkClient.active)
        {
            manager.StopServer();
        }
    }
}
