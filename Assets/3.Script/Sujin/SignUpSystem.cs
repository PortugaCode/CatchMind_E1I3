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

    public void SignBotton()
    {
        if(idInput.text.Equals(string.Empty) || passInput.Equals(string.Empty) || passConfirm.Equals(string.Empty))
        {
            log.text = "모든 칸을 입력해주세요.";
            return;
        }
      

        if (passInput.text != passConfirm.text)
        {
            //회원가입 실패( 비밀번호 일치x) 
            log.text = "비밀번호가 일치 하지 않습니다.다시 확인";
            return;
        }
        else if (SQL_Manager.instance.signUp(idInput.text, passInput.text))
        {
            //회원가입 성공

            SceneManager.LoadScene("Login");
            
        }


    }
}
