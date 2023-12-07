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

    public static SQL_Manager instance = null;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dbPath = Application.dataPath + "/Database";
        string serverInfo = ServerSet(dbPath);      

        try
        {
            if(serverInfo.Equals(string.Empty))
            {
                Debug.Log("SQL Server Json Error!");
                return;
            }
            connection = new MySqlConnection(serverInfo);
            connection.Open();  //DB ����
            Debug.Log("SQL Server Open!!");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private string ServerSet(string path)
    {
        if(!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string jsonString = File.ReadAllText(path + "/config.json");

        JsonData itemdata = JsonMapper.ToObject(jsonString);
        string serverInfo =
            $"Server= {itemdata[0]["IP"]};" +
            $"Database = {itemdata[0]["TableName"]};" +
            $"Uid = {itemdata[0]["ID"]};" +
            $"Pwd = {itemdata[0]["PW"]};" +
            $"Port = {itemdata[0]["PORT"]};" +
            $"CharSet =utf8;";

        return serverInfo;

    }

    private bool connectionCheck(MySqlConnection con)
    {
        //���� MySqlConnection Open ���°� �ƴ϶��
        if (con.State != System.Data.ConnectionState.Open)
        {
            con.Open();
            if(con.State != System.Data.ConnectionState.Open)
            {
                return false;
            }
        }
        return true;
    }

    public bool Login(string id, string password)
    {
        try
        {
            //1. connection open ��Ȳ���� Ȯ�� 
            if(!connectionCheck(connection))
            {
                return false;
            }
            string SQLCommand =     //���� �������� �κ�
                string.Format(@"SELECT ID, Password FROM catchmind
                                WHERE ID='{0}' AND Password ='{1}';",
                                id, password);
            MySqlCommand cmd = new MySqlCommand(SQLCommand, connection);
            dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                // ���� ������ �ϴϾ� ����
                while (dataReader.Read())
                {
                    //���׿�����
                    string name = (dataReader.IsDBNull(0)) ? string.Empty : (string)dataReader["ID"].ToString();
                    string Pass = (dataReader.IsDBNull(1)) ? string.Empty : (string)dataReader["Password"].ToString();
                   

                    if (!name.Equals(string.Empty) || !password.Equals(string.Empty))
                    {
                        //Data�� �ҷ��� ��Ȳ
                        info = new userInfo(name, password);
                        if (!dataReader.IsClosed) dataReader.Close();
                        return true;
                    }
                    else // �α��� ����
                    {
                        break;
                    }
                }//..while
            }//..if

            if (!dataReader.IsClosed) dataReader.Close();
            return false;
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            if (!dataReader.IsClosed) dataReader.Close();
            return false;
        }
    }
   
   

    public bool signUp(string id, string password)
    {
        try
        {
            //sql open
            if (!connectionCheck(connection))
            {
                return false;
            }


            // ���̵� �ߺ� Ȯ��
            string checkDuplicateSQL =
                string.Format("SELECT COUNT(*) FROM `catchmind` WHERE `ID` = '{0}'", id);
            MySqlCommand checkDuplicateCmd = new MySqlCommand(checkDuplicateSQL, connection);

            // �ߺ��� ���̵� �̹� �����ϴ��� Ȯ��
            int count = Convert.ToInt32(checkDuplicateCmd.ExecuteScalar());
            if (count > 0)
            {
                
                Debug.Log("�̹� �����ϴ� ���̵��Դϴ�.");
                return false;
            }

            //ȸ������
            string SQLCommand =
                  string.Format(@"INSERT INTO `catchmind`(`ID`, `Password`) VALUE 
                                ('{0}','{1}');",
                                id, password);
            MySqlCommand cmd = new MySqlCommand(SQLCommand, connection);

            cmd.ExecuteNonQuery();  //MySql ����

            return true;
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            if (!dataReader.IsClosed) dataReader.Close();
            return false;
        }
    }

    
}
