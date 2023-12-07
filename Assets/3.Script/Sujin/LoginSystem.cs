using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginSystem : MonoBehaviour
{
    public InputField idInput;
    public InputField passInput;

    [SerializeField] private Text log;

    public void LoginBotton()
    {
        if(idInput.text.Equals(string.Empty) || passInput.text.Equals(string.Empty))
        {
            log.text = "���̵� ��й�ȣ�� �Է��ϼ���.";
            return;
        }
        if (SQL_Manager.instance.Login(idInput.text, passInput.text))
        {
            //�α��� ����

            userInfo info = SQL_Manager.instance.info;
            Debug.Log(info.userName + " | " + info.userPassword);
            SceneManager.LoadScene("CatchMind_Main");
        }
        else
        {
            //�α��� ����
            Debug.Log("���̵� ��й�ȣ�� Ȯ���� �ּ���..");
        }


    }

}
