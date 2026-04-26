using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class ToyManager : MonoBehaviour
{
    public static ToyManager Instance;

    public Slider globalEnergySlider;
    public CinemachineCamera virtualCamera;

    private NormalToyMovement currentToy;
    private AeroplaneMovement currentAeroplane;
    private ToyEnergy currentEnergy;
    private KeyStart  currentKeyStart;   // tracks whose music is active

    public NormalToyMovement CurrentToy => currentToy;

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

    public void SwitchToy(NormalToyMovement newToy, ToyEnergy newEnergy, KeyStart keyStart = null)
    {
        // Stop the previous toy's music immediately
        if (currentKeyStart != null && currentKeyStart != keyStart)
            currentKeyStart.StopMusicImmediate();
        currentKeyStart = keyStart;

        // Revoke any current control
        if (currentToy != null)       currentToy.SetControl(false);
        if (currentAeroplane != null) currentAeroplane.SetControl(false);
        currentAeroplane = null;

        currentToy    = newToy;
        currentEnergy = newEnergy;

        if (currentToy != null) currentToy.SetControl(true);

        if (virtualCamera != null && currentToy != null)
            virtualCamera.Follow = currentToy.transform;

        if (globalEnergySlider != null && currentEnergy != null)
        {
            globalEnergySlider.maxValue = currentEnergy.maxEnergy;
            globalEnergySlider.value   = currentEnergy.CurrentEnergy;
        }
    }

    public void SwitchAeroplane(AeroplaneMovement newPlane, ToyEnergy newEnergy, KeyStart keyStart = null)
    {
        // Stop the previous toy's music immediately
        if (currentKeyStart != null && currentKeyStart != keyStart)
            currentKeyStart.StopMusicImmediate();
        currentKeyStart = keyStart;

        // Revoke any current control
        if (currentToy != null)       currentToy.SetControl(false);
        if (currentAeroplane != null) currentAeroplane.SetControl(false);
        currentToy = null;

        currentAeroplane = newPlane;
        currentEnergy    = newEnergy;

        if (currentAeroplane != null) currentAeroplane.SetControl(true);

        if (virtualCamera != null && currentAeroplane != null)
            virtualCamera.Follow = currentAeroplane.transform;

        if (globalEnergySlider != null && currentEnergy != null)
        {
            globalEnergySlider.maxValue = currentEnergy.maxEnergy;
            globalEnergySlider.value   = currentEnergy.CurrentEnergy;
        }
    }

    private void Update()
    {
        // Normal toy energy drain
        if (currentToy != null && currentEnergy != null)
        {
            if (currentToy.IsControlled && currentEnergy.HasEnergy && currentToy.IsMoving)
                currentEnergy.Deplete(Time.deltaTime);

            if (globalEnergySlider != null)
                globalEnergySlider.value = currentEnergy.CurrentEnergy;
        }

        // Aeroplane energy drain
        if (currentAeroplane != null && currentEnergy != null)
        {
            if (currentAeroplane.IsControlled && currentEnergy.HasEnergy && currentAeroplane.IsMoving)
                currentEnergy.Deplete(Time.deltaTime);

            if (globalEnergySlider != null)
                globalEnergySlider.value = currentEnergy.CurrentEnergy;
        }
    }
}