using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindingLoader : MonoBehaviour
{
    public InputActionAsset inputActions;

    void Awake()
    {
        foreach (var action in inputActions)
        {
            if (PlayerPrefs.HasKey(action.name))
            {
                action.LoadBindingOverridesFromJson(
                    PlayerPrefs.GetString(action.name)
                );
            }
        }
    }
}
