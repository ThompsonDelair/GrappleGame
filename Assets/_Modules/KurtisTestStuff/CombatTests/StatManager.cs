using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatManager : MonoBehaviour
{
    public enum StatType {
        Health,
        Damage,
        Speed,
        Defence
    }

    // Stat fields
    [SerializeField] private StatSheet statSheet;
    private Dictionary<StatType, float> baseStatValues;
    private Dictionary<StatType, List<float>> statModifiers;

    // Start is called before the first frame update
    void Awake() {
        // 1. Initialize both dictionaries
        InitalizeStats();
    }

    // Sets initial stat values
    private void InitalizeStats() {
        // Check for a statSheet
        if (statSheet == null) {
            Debug.LogWarning("No Stat Sheet provided to Actor " + this.name);
            return;
        }

        // Initialize all Stat Lookups
        baseStatValues = new Dictionary<StatType, float>();
        statModifiers = new Dictionary<StatType, List<float>>(); // We use a list over an array because values will be added and removed.

        // Set base values
        baseStatValues.Add(StatType.Health, statSheet.baseHealth);
        baseStatValues.Add(StatType.Damage, statSheet.baseDamage);
        baseStatValues.Add(StatType.Speed, statSheet.baseSpeed);
        baseStatValues.Add(StatType.Defence, statSheet.baseDefence);
        
        // Iterate through each stattype and init mod lists.
        foreach (StatType type in (StatType[])StatType.GetValues(typeof(StatType))) {
            statModifiers.Add(type, new List<float>());
        }

    }

    public void AddModifier(StatType statType, float modValue) {
        if (statModifiers.ContainsKey(statType)) {
            List<float> modifiers = statModifiers[statType];

            // Add the modifier to the list if it's not already in there.
            if (!modifiers.Contains(modValue)) {
                modifiers.Add(modValue);
            }

        }
    }

    public void RemoveModifier(StatType statType, float modValue) {
        if (statModifiers.ContainsKey(statType)) {
            
            List<float> modifiers = statModifiers[statType];

            // Add the modifier to the list if it's not already in there.
            if (modifiers.Contains(modValue)) {
                modifiers.Remove(modValue);
            }
            
        }
    }

    // Gets the base value * modifier
    public float GetStat(StatType statType) {
        if (baseStatValues.ContainsKey(statType)) {

            float modSum = GetStatModifier(statType);

            // Return base value * modifier
            return baseStatValues[statType] * modSum;

        } else {
            Debug.LogWarning("The Stat Type " + statType + " has not been properly initialized for actor " + this.name);
            return 0;
        }
    }

    // Returns the total modifier of given stat type.
    // Value is clamped to be >= 0
    public float GetStatModifier(StatType statType) {
        if (statModifiers.ContainsKey(statType)) {

            // Sum all modifiers. If no modifiers are in list, value will be 1;
            float modSum = 1f;
            foreach (float mod in statModifiers[statType]) {
                modSum += mod;
            }

            // Return modifier, clamped to be non-negative
            return Mathf.Clamp(modSum, 0, Mathf.Infinity);

        } else {
            Debug.LogWarning("The Stat Type " + statType + " has not been properly initialized for actor " + this.name);
            return 0;
        }
    }

    
}
