using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject player;

    public void Main()
    {
        panel.SetActive(true);
    }

    public void SetClose()
    {
        cam.SetActive(false);
        player.SetActive(true);
        hud.SetActive(true);
        hud.SetActive(true);
    }
}
