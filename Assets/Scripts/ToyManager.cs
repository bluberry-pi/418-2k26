using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class ToyManager : MonoBehaviour
{
    public static ToyManager Instance;

    public Slider globalEnergySlider;
    public CinemachineCamera virtualCamera;

    private NormalToyMovement currentToy;
    private ToyEnergy currentEnergy;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (globalEnergySlider != null)
        {
            globalEnergySlider.value = 0f;
        }
    }

    public void SwitchToy(NormalToyMovement newToy, ToyEnergy newEnergy)
    {
        if (currentToy != null)
        {
            currentToy.SetControl(false);
        }

        currentToy = newToy;
        currentEnergy = newEnergy;
        
        if (currentToy != null)
        {
            currentToy.SetControl(true);
        }

        if (virtualCamera != null && currentToy != null)
        {
            virtualCamera.Follow = currentToy.transform;
        }

        if (globalEnergySlider != null && currentEnergy != null)
        {
            globalEnergySlider.maxValue = currentEnergy.maxEnergy;
            globalEnergySlider.value = currentEnergy.CurrentEnergy;
        }
    }

    private void Update()
    {
        if (currentToy != null && currentEnergy != null)
        {
            if (currentToy.IsControlled && currentEnergy.HasEnergy)
            {
                currentEnergy.Deplete(Time.deltaTime);
            }

            if (globalEnergySlider != null)
            {
                globalEnergySlider.value = currentEnergy.CurrentEnergy;
            }
        }
    }
}