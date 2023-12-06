using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    private string apiUrl = "https://api.openai.com/v1/engines/davinci-codex/completions";
    private string apiKey = "temp";

    private void Start()
    {
        StartCoroutine(GetData_co("temp"));
    }

    private IEnumerator GetData_co(string userInput)
    {
        string requestBody = $"{{\"prompt\": \"{userInput}\", \"max_tokens\": 50}}";

        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, requestBody))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            // Error
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error : {request.error}");
            }
            else
            {
                Debug.Log($"Response : {request.downloadHandler.text}");
            }
        }
    }
}
