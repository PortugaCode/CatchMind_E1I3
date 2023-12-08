using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SignUpSystem : MonoBehaviour
{
    public InputField idInput;
    public InputField passInput;
    public InputField passConfirm;

    [SerializeField] private Text log;

    private void Start()
    {
        SQL_Manager.instance.log = log;
    }

    public void SignBotton()
    {
        if(idInput.text.Equals(string.Empty) || passInput.Equals(string.Empty) || passConfirm.Equals(string.Empty))
        {
            log.text = "��� ĭ�� �Է����ּ���.";
            return;
        }

        if (passInput.text != passConfirm.text)
        {
            //ȸ������ ����( ��й�ȣ ��ġx) 
            log.text = "��й�ȣ�� ��ġ���� �ʽ��ϴ�.";
            return;
        }
        else if (SQL_Manager.instance.signUp(idInput.text, passInput.text))
        {
            //ȸ������ ����

            SceneManager.LoadScene("Login");
        }
    }
}
