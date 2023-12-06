using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using LitJson;

public class userInfo
{
    public string userName { get; private set; }
    public string userPassword { get; private set; }
    public userInfo(string name, string password)
    {
        userName = name;
        userPassword = password;
    }
}
public class SQL_Manager : MonoBehaviour
{
    public userInfo info;

    //Connection
    public MySqlConnection connection;
    public MySqlDataReader dataReader;

    public string dbPath = string.Empty;

    public static SQL_Manager instace = null;
    public void Awake()
    {
        if (instace == null)
        {
            instace = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dbPath = Application.dataPath + "/Database";
        //string serverInfo = ServerSet(dbPath);      

        try
        {
            Debug.Log("SQL SErver Json Error");
            return;
        }
        catch(Exception e)
        {

        }
    }

    
}
