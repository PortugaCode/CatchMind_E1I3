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
            log.text = "아이디 비밀번호를 입력하세요.";
            return;
        }
        if (SQL_Manager.instance.Login(idInput.text, passInput.text))
        {
            //로그인 성공

            /*userInfo info = SQL_Manager.instance.info;
            Debug.Log(info.userName + " | " + info.userPassword);*/ //Login Debug
            SceneManager.LoadScene("CatchMind_Main_Jinwon");
        }
        else
        {
            //로그인 실패
            log.text = "아이디 비밀번호를 확인해주세요..";
        }


    }

   

}
