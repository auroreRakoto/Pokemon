
using UnityEngine;

[System.Serializable]
public class PokemonNeeds
{
    [Range(0f, 100f)]
    [SerializeField] private float satiety = 100f;

    public float Satiety => satiety;

    public void Initialize(float initialHunger)
    {
        satiety = Mathf.Clamp(initialHunger, 0f, 100f);
    }

    public void UpdateSatiety(float deltaTime, float hungerDrain)
    {
        satiety = Mathf.Max(
            0f,
            satiety - hungerDrain * deltaTime
        );
    }

    public void RestoreSatiety(float foodAmount, float recoveryPerFood)
    {
        satiety = Mathf.Clamp(
            satiety + foodAmount * recoveryPerFood,
            0f,
            100f
        );
    }
}
