using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    public InputField idInput;
    public InputField passInput;

    [SerializeField] private Text log;

    private void LoginBotton()
    {
        if(idInput.text.Equals(string.Empty) || passInput.text.Equals(string.Empty))
        {
            log.text = "���̵� ��й�ȣ�� �Է��ϼ���.";
            return;
        }
        //SQL_Manager ��������

    }

}
