using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainScene : MonoBehaviour
{
    public void LoadLevelOne() 
    {
        SceneManager.LoadScene("Main");
    }
}
