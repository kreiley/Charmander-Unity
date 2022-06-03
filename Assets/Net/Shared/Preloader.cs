using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject serverPrefab;
    public void OnClientClick()
    {
        DontDestroyOnLoad(Instantiate(clientPrefab).gameObject);
        SceneManager.LoadScene("Lobby");
    }
    public void OnServerClick()
    {
        DontDestroyOnLoad(Instantiate(serverPrefab).gameObject);
        SceneManager.LoadScene("Lobby");
    }
    public void OnClientServerClick()
    {
        OnClientClick();
        OnServerClick();
        SceneManager.LoadScene("Lobby");
    }
}
