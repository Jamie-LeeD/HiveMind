using System;
using UnityEngine;

/// <summary>
/// Witch mana. Drains while a skeleton is possessed and regenerates otherwise.
/// </summary>
public class Mana : MonoBehaviour
{
    [SerializeField] float maxMana = 100f;
    [SerializeField] float currentMana = 100f;
    [SerializeField] float manaDrainRate = 8f;
    [SerializeField] float manaRegenerationRate = 15f;

    bool draining;
    bool depletionNotified;

    public float Max => maxMana;
    public float Current => currentMana;
    public bool HasMana => currentMana > 0.05f;
    public bool IsDraining => draining;

    public event Action<float, float> Changed;
    public event Action Depleted;

    void Awake()
    {
        maxMana = Mathf.Max(0.01f, maxMana);
        currentMana = maxMana;
        RaiseChanged();
    }

    public void SetDraining(bool value)
    {
        draining = value;
    }

    void Update()
    {
        float before = currentMana;

        if (draining)
        {
            currentMana -= manaDrainRate * Time.deltaTime;
            if (currentMana <= 0f)
            {
                currentMana = 0f;
                if (!depletionNotified)
                {
                    depletionNotified = true;
                    RaiseChanged();
                    Depleted?.Invoke();
                }
            }
        }
        else
        {
            currentMana += manaRegenerationRate * Time.deltaTime;
            if (currentMana > 0f)
                depletionNotified = false;
        }

        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        if (!Mathf.Approximately(before, currentMana))
            RaiseChanged();
    }

    void RaiseChanged()
    {
        Changed?.Invoke(currentMana, maxMana);
    }

    void OnDestroy()
    {
        Changed = null;
        Depleted = null;
    }
}
