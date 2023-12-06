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
            log.text = "아이디 비밀번호를 입력하세요.";
            return;
        }
        //SQL_Manager 가져오기

    }

}
