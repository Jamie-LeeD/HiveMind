using System;
using UnityEngine;

/// <summary>
/// Lives lost when mana runs out during possession.
/// </summary>
public class WitchLives : MonoBehaviour
{
    [SerializeField] int maxLives = 3;
    [SerializeField] int currentLives = 3;

    public int Max => maxLives;
    public int Current => currentLives;
    public bool HasLives => currentLives > 0;

    public event Action<int, int> Changed;
    public event Action GameOver;

    void Awake()
    {
        maxLives = Mathf.Max(1, maxLives);
        currentLives = maxLives;
        Changed?.Invoke(currentLives, maxLives);
    }

    public void LoseLife()
    {
        if (currentLives <= 0)
            return;

        currentLives--;
        Changed?.Invoke(currentLives, maxLives);

        if (currentLives <= 0)
            GameOver?.Invoke();
    }

    void OnDestroy()
    {
        Changed = null;
        GameOver = null;
    }
}
