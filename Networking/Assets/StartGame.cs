using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
