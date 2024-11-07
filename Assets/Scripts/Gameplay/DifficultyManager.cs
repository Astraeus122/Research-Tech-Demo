using System;
using Unity.BossRoom.Gameplay.GameplayObjects;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.AI
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        // Base multipliers controlled by Rule-Based AI
        public float BaseEnemyDamageMultiplier { get; private set; } = 1f;
        public float BaseEnemySpawnRateMultiplier { get; private set; } = 1f;
        public float BaseHealingAssistanceMultiplier { get; private set; } = 1f;

        // RL multipliers controlled by RL Adaptive AI
        public float RLEnemyDamageMultiplier { get; private set; } = 1f;
        public float RLEnemySpawnRateMultiplier { get; private set; } = 1f;
        public float RLHealingAssistanceMultiplier { get; private set; } = 1f;

        // Final multipliers applied to the game
        public float FinalEnemyDamageMultiplier => BaseEnemyDamageMultiplier * RLEnemyDamageMultiplier;
        public float FinalEnemySpawnRateMultiplier => BaseEnemySpawnRateMultiplier * RLEnemySpawnRateMultiplier;
        public float FinalHealingAssistanceMultiplier => BaseHealingAssistanceMultiplier * RLHealingAssistanceMultiplier;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Methods to adjust base multipliers
        public void AdjustBaseEnemyDamage(float adjustment)
        {
            BaseEnemyDamageMultiplier += adjustment;
            BaseEnemyDamageMultiplier = Mathf.Clamp(BaseEnemyDamageMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Enemy Damage Multiplier adjusted to: {BaseEnemyDamageMultiplier}");
            ApplyFinalMultipliers();
        }

        public void AdjustBaseEnemySpawnRate(float adjustment)
        {
            BaseEnemySpawnRateMultiplier += adjustment;
            BaseEnemySpawnRateMultiplier = Mathf.Clamp(BaseEnemySpawnRateMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Enemy Spawn Rate Multiplier adjusted to: {BaseEnemySpawnRateMultiplier}");
            ApplyFinalMultipliers();
        }

        public void AdjustBaseHealingAssistance(float adjustment)
        {
            BaseHealingAssistanceMultiplier += adjustment;
            BaseHealingAssistanceMultiplier = Mathf.Clamp(BaseHealingAssistanceMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Healing Assistance Multiplier adjusted to: {BaseHealingAssistanceMultiplier}");
            ApplyFinalMultipliers();
        }

        // Methods to adjust RL multipliers
        public void AdjustRLEnemyDamage(float adjustment)
        {
            RLEnemyDamageMultiplier += adjustment;
            RLEnemyDamageMultiplier = Mathf.Clamp(RLEnemyDamageMultiplier, 0.5f, 1.5f);
            Debug.Log($"RL Enemy Damage Multiplier adjusted to: {RLEnemyDamageMultiplier}");
            ApplyFinalMultipliers();
        }

        public void AdjustRLEnemySpawnRate(float adjustment)
        {
            RLEnemySpawnRateMultiplier += adjustment;
            RLEnemySpawnRateMultiplier = Mathf.Clamp(RLEnemySpawnRateMultiplier, 0.5f, 1.5f);
            Debug.Log($"RL Enemy Spawn Rate Multiplier adjusted to: {RLEnemySpawnRateMultiplier}");
            ApplyFinalMultipliers();
        }

        public void AdjustRLHealingAssistance(float adjustment)
        {
            RLHealingAssistanceMultiplier += adjustment;
            RLHealingAssistanceMultiplier = Mathf.Clamp(RLHealingAssistanceMultiplier, 0.5f, 1.5f);
            Debug.Log($"RL Healing Assistance Multiplier adjusted to: {RLHealingAssistanceMultiplier}");
            ApplyFinalMultipliers();
        }

        // Apply the final multipliers to relevant game objects
        private void ApplyFinalMultipliers()
        {
            // Apply FinalEnemyDamageMultiplier to all DamageReceivers that are monsters
            foreach (var damageReceiver in FindObjectsOfType<DamageReceiver>())
            {
                if (damageReceiver.IsMonster())
                {
                    damageReceiver.SetDamageMultiplier(FinalEnemyDamageMultiplier);
                }
            }

            // Apply FinalEnemySpawnRateMultiplier to all enemy spawners
            foreach (var spawner in FindObjectsOfType<ServerWaveSpawner>())
            {
                spawner.SetSpawnRateMultiplier(FinalEnemySpawnRateMultiplier);
            }

            Debug.Log($"Final Multipliers Applied - Enemy Damage: {FinalEnemyDamageMultiplier}, Enemy Spawn Rate: {FinalEnemySpawnRateMultiplier}, Healing Assistance: {FinalHealingAssistanceMultiplier}");
        }
    }
}
