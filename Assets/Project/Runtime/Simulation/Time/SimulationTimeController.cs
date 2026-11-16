
using UnityEngine;
using UnityEngine.InputSystem;

public class SimulationTimeController : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float[] speedLevels =
    {
        0f, 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f
    };

    [SerializeField] private int defaultSpeedIndex = 2;

    private int currentSpeedIndex;

    public float CurrentSpeed => speedLevels[currentSpeedIndex];
    public bool IsPaused => CurrentSpeed == 0f;

    private void Start()
    {
        currentSpeedIndex = Mathf.Clamp(
            defaultSpeedIndex,
            0,
            speedLevels.Length - 1
        );

        ApplySpeed();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            TogglePause();

        if (Keyboard.current.equalsKey.wasPressedThisFrame ||
            Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            IncreaseSpeed();

        if (Keyboard.current.minusKey.wasPressedThisFrame ||
            Keyboard.current.numpadMinusKey.wasPressedThisFrame)
            DecreaseSpeed();

        if (Keyboard.current.digit0Key.wasPressedThisFrame ||
            Keyboard.current.numpad0Key.wasPressedThisFrame)
            SetNormalSpeed();
    }

    public void IncreaseSpeed()
    {
        currentSpeedIndex = Mathf.Min(
            currentSpeedIndex + 1,
            speedLevels.Length - 1
        );

        ApplySpeed();
    }

    public void DecreaseSpeed()
    {
        currentSpeedIndex = Mathf.Max(
            currentSpeedIndex - 1,
            0
        );

        ApplySpeed();
    }

    public void TogglePause()
    {
        if (IsPaused)
            SetNormalSpeed();
        else
        {
            currentSpeedIndex = 0;
            ApplySpeed();
        }
    }

    public void SetNormalSpeed()
    {
        currentSpeedIndex = Mathf.Clamp(
            defaultSpeedIndex,
            0,
            speedLevels.Length - 1
        );

        ApplySpeed();
    }

    public void SetSpeed(int index)
    {
        currentSpeedIndex = Mathf.Clamp(
            index,
            0,
            speedLevels.Length - 1
        );

        ApplySpeed();
    }

    private void ApplySpeed()
    {
        Time.timeScale = CurrentSpeed;

        Debug.Log($"Simulation speed: {CurrentSpeed}x");
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
