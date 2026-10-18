using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads the project's Player action map. Possession uses the Possess action (Q).
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameplayInput : MonoBehaviour
{
    [SerializeField] InputActionAsset actions;

    public InputAction Move { get; private set; }
    public InputAction Look { get; private set; }
    public InputAction Possess { get; private set; }
    public InputAction Carry { get; private set; }

    InputActionMap playerMap;

    void Awake()
    {
        if (actions == null)
        {
            Debug.LogError("GameplayInput is missing the InputSystem_Actions asset.", this);
            return;
        }

        playerMap = actions.FindActionMap("Player", false);
        if (playerMap == null)
        {
            Debug.LogError("Player action map was not found on the input asset.", this);
            return;
        }

        Move = playerMap.FindAction("Move", false);
        Look = playerMap.FindAction("Look", false);
        Possess = playerMap.FindAction("Possess", false);
        Carry = playerMap.FindAction("Carry", false);

        if (Move == null || Look == null || Possess == null || Carry == null)
        {
            Debug.LogError("Move, Look, Possess, or Carry is missing from the Player action map.", this);
            return;
        }

        playerMap.Enable();
    }

    public void SetGameplayEnabled(bool enabled)
    {
        if (playerMap == null)
            return;

        if (enabled)
            playerMap.Enable();
        else
            playerMap.Disable();
    }
}
