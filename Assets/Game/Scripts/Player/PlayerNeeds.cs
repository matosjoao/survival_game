using UnityEngine;

public class PlayerNeeds : MonoBehaviour, IDamagable
{
    [Header("Needs")]
    [SerializeField] private Need health;
    [SerializeField] private Need hunger;
    [SerializeField] private Need thirst;

    [Header("Decays")]
    [SerializeField] private float noHungerHealthDecay;
    [SerializeField] private float noThirstHealthDecay;

    private void Start() 
    {
        // Initialize needs values
        health.curValue = health.startValue;
        hunger.curValue = hunger.startValue;
        thirst.curValue = thirst.startValue;
    }

    private void Update() 
    {
        // Decay needs over time
        hunger.Subtract(hunger.decayRate * Time.deltaTime);
        thirst.Subtract(hunger.decayRate * Time.deltaTime);

        // Decay health over time if hunger
        if(hunger.curValue == 0.0f)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
        }

        // Decay health over time if thirst
        if(thirst.curValue == 0.0f)
        {
            health.Subtract(noThirstHealthDecay * Time.deltaTime);
        }

        // Check if player is dead
        // TODO:: Change health to another script
        if(health.curValue == 0.0f)
        {
            Die();
        }

        // Update UI bars
        UIManager.Instance.UpdateNeedsUI(health.GetPercentage(), hunger.GetPercentage(), thirst.GetPercentage());
    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Eat(float amount)
    {
        hunger.Add(amount);
    }

    public void Drink(float amount)
    {
        thirst.Add(amount);
    }

    public void TakePhisicalDamage(int amount)
    {
        // TODO:: Change health for another script
        health.Subtract(amount);
        UIManager.Instance.TakePhisicalDamage();
    }

    public void Die()
    {

    }

}

// TODO:: Change Need Class to another script
[System.Serializable]
public class Need
{
    [HideInInspector] public float curValue;
    public float maxValue;
    public float startValue;
    public float regenRate;
    public float decayRate;

    public void Add(float amount)
    {
        curValue = Mathf.Min(curValue + amount, maxValue);
    }

    public void Subtract(float amount)
    {
        curValue = Mathf.Max(curValue - amount, 0);
    }

    public float GetPercentage()
    {
        return curValue / maxValue;
    }
}
