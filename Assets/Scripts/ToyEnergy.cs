using UnityEngine;

public class ToyEnergy : MonoBehaviour
{
    public float maxEnergy = 100f;
    public float energyDepleteRate = 10f;
    
    public float CurrentEnergy { get; private set; }
    public bool HasEnergy => CurrentEnergy > 0f;

    private void Start()
    {
        CurrentEnergy = 0f;
    }

    public void FillEnergy()
    {
        CurrentEnergy = maxEnergy;
    }

    public void Deplete(float amount)
    {
        CurrentEnergy -= energyDepleteRate * amount;
        if (CurrentEnergy < 0f) CurrentEnergy = 0f;
    }
}