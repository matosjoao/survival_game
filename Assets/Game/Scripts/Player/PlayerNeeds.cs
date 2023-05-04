using System;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(PlayerController))]
public class PlayerNeeds : MonoBehaviour, IDamagable
{
    [Header("Needs")]
    private Inventory playerInventory;
    private InputReader inputReader;
    private PlayerController playerController;

    [Header("Needs")]
    [SerializeField] private Need health;
    [SerializeField] private Need hunger;
    [SerializeField] private Need thirst;

    [Header("Decays")]
    [SerializeField] private float noHungerHealthDecay;
    [SerializeField] private float noThirstHealthDecay;

    [Header("Slider")]
    private float sliderProgress;
    private bool sliderActive;
    private float sliderActionTime;

    private ItemSlot curSelectedItemSlot;

    private void Awake() 
    {
        playerInventory = GetComponent<Inventory>();
        inputReader = GetComponent<InputReader>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.MouseLeftEvent += OnConsume;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.MouseLeftEvent -= OnConsume;
    }

    private void Start() 
    {
        // Initialize needs values
        health.curValue = health.startValue;
        hunger.curValue = hunger.startValue;
        thirst.curValue = thirst.startValue;

        sliderProgress = 0.0f;
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

        // If slider action active
        if(!sliderActive)
            return;

        if(inputReader.IsPressingLeftMouse)
        {
            OnSliderProgress(Time.deltaTime);
        }
        else
        {
            ResetSlider();
        }
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

    private void OnConsume()
    {   
        // Is interacting ? 
        if(playerController.IsInteracting)
            return;

        // Get current slot
        curSelectedItemSlot = playerInventory.GetSelectedItemSlot();
        if(curSelectedItemSlot == null)
            return;

        // Is consumable
        if(curSelectedItemSlot.Item.type != ItemType.Consumable)
            return;

        // Execute Animation
        // TODO:: BUILD THE ANIMATION

        // Show action slider
        sliderActive = true;
        sliderActionTime = 2.0f;
        UIManager.Instance.ShowSlider();
    }

    private void OnSliderProgress(float deltaTime)
    {   
        // Update progress
        sliderProgress += deltaTime / sliderActionTime;

        if(sliderProgress >= 1.0f)
        {
            // Reset slider
            ResetSlider();

            //Invoke event
            OnSliderFinish();
        }
        
        UIManager.Instance.UpdateActionSlider(sliderProgress);
    }

    private void ResetSlider()
    {
        // Reset variables
        sliderActive = false;
        sliderActionTime = 0.0f;
        sliderProgress = 0.0f;

        // Hide Slider
        UIManager.Instance.HideSlider();
    }

    private void OnSliderFinish()
    {
        if(curSelectedItemSlot == null)
            return;

        // Done with the progress bar
        for (int i = 0; i < curSelectedItemSlot.Item.consumables.Length; i++)
        {
            switch (curSelectedItemSlot.Item.consumables[i].type)
            {
                case ConsumableType.Health: 
                    Heal(curSelectedItemSlot.Item.consumables[i].value);
                    break;
                
                case ConsumableType.Hunger: 
                    Eat(curSelectedItemSlot.Item.consumables[i].value);
                    break;
                
                case ConsumableType.Thirst: 
                    Drink(curSelectedItemSlot.Item.consumables[i].value);
                    break;

                default:
                    return;
            }
        }

        playerInventory.OnActionReduceSelectedItemQuantity();
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
