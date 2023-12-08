using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectSceneControll : MonoBehaviour
{
    public void SceneLoad(string name)
    {
        if(name != "SignUp")
        {
            SQL_Manager.instance.log = null;
        }
        SceneManager.LoadScene(name);
    }
}
