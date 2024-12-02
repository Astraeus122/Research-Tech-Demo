using System;
using System.IO;
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
        public float BaseEnemyHealthMultiplier { get; private set; } = 1f; // Added

        // RL multipliers controlled by RL Adaptive AI
        public float RLEnemyDamageMultiplier { get; private set; } = 1f;
        public float RLEnemySpawnRateMultiplier { get; private set; } = 1f;
        public float RLHealingAssistanceMultiplier { get; private set; } = 1f;
        public float RLEnemyHealthMultiplier { get; private set; } = 1f; // Added

        public float BasePlayerDamageMultiplier { get; private set; } = 1f;
        public float RLPlayerDamageMultiplier { get; private set; } = 1f;
        public float FinalPlayerDamageMultiplier => BasePlayerDamageMultiplier * RLPlayerDamageMultiplier;

        // Final multipliers applied to the game
        public float FinalEnemyDamageMultiplier => BaseEnemyDamageMultiplier * RLEnemyDamageMultiplier;
        public float FinalEnemySpawnRateMultiplier => BaseEnemySpawnRateMultiplier * RLEnemySpawnRateMultiplier;
        public float FinalHealingAssistanceMultiplier => BaseHealingAssistanceMultiplier * RLHealingAssistanceMultiplier;
        public float FinalEnemyHealthMultiplier => BaseEnemyHealthMultiplier * RLEnemyHealthMultiplier; // Added

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
        public void AdjustBaseEnemyDamage(float adjustment, string context = null)
        {
            float oldValue = BaseEnemyDamageMultiplier;
            BaseEnemyDamageMultiplier += adjustment;
            BaseEnemyDamageMultiplier = Mathf.Clamp(BaseEnemyDamageMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Enemy Damage Multiplier adjusted to: {BaseEnemyDamageMultiplier}. Context: {context}");
            ApplyFinalMultipliers();

            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "BaseEnemyDamageMultiplier",
                    oldValue,
                    BaseEnemyDamageMultiplier,
                    "Rule-Based Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustBaseEnemySpawnRate(float adjustment, string context = null)
        {
            float oldValue = BaseEnemySpawnRateMultiplier;
            BaseEnemySpawnRateMultiplier += adjustment;
            BaseEnemySpawnRateMultiplier = Mathf.Clamp(BaseEnemySpawnRateMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Enemy Spawn Rate Multiplier adjusted to: {BaseEnemySpawnRateMultiplier}. Context: {context}");
            ApplyFinalMultipliers();

            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "BaseEnemySpawnRateMultiplier",
                    oldValue,
                    BaseEnemySpawnRateMultiplier,
                    "Rule-Based Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustBaseHealingAssistance(float adjustment, string context = null)
        {
            float oldValue = BaseHealingAssistanceMultiplier;
            BaseHealingAssistanceMultiplier += adjustment;
            BaseHealingAssistanceMultiplier = Mathf.Clamp(BaseHealingAssistanceMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Healing Assistance Multiplier adjusted to: {BaseHealingAssistanceMultiplier}. Context: {context}");
            ApplyFinalMultipliers();

            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "BaseHealingAssistanceMultiplier",
                    oldValue,
                    BaseHealingAssistanceMultiplier,
                    "Rule-Based Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustBaseEnemyHealth(float adjustment, string context = null) // Added
        {
            float oldValue = BaseEnemyHealthMultiplier;
            BaseEnemyHealthMultiplier += adjustment;
            BaseEnemyHealthMultiplier = Mathf.Clamp(BaseEnemyHealthMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Enemy Health Multiplier adjusted to: {BaseEnemyHealthMultiplier}. Context: {context}");
            ApplyFinalMultipliers();

            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "BaseEnemyHealthMultiplier",
                    oldValue,
                    BaseEnemyHealthMultiplier,
                    "Rule-Based Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        // Methods to adjust RL multipliers
        public void AdjustRLEnemyDamage(float adjustment, string context = null)
        {
            float oldValue = RLEnemyDamageMultiplier;
            RLEnemyDamageMultiplier += adjustment;
            RLEnemyDamageMultiplier = Mathf.Clamp(RLEnemyDamageMultiplier, 0.5f, 1.5f);

            Debug.Log($"RL Enemy Damage Multiplier adjusted to: {RLEnemyDamageMultiplier}. Context: {context}");

            ApplyFinalMultipliers();

            // Log the change if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "RLEnemyDamageMultiplier",
                    oldValue,
                    RLEnemyDamageMultiplier,
                    "RL Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustRLEnemySpawnRate(float adjustment, string context = null)
        {
            float oldValue = RLEnemySpawnRateMultiplier;
            RLEnemySpawnRateMultiplier += adjustment;
            RLEnemySpawnRateMultiplier = Mathf.Clamp(RLEnemySpawnRateMultiplier, 0.5f, 1.5f);

            Debug.Log($"RL Enemy Spawn Rate Multiplier adjusted to: {RLEnemySpawnRateMultiplier}. Context: {context}");

            ApplyFinalMultipliers();

            // Log the change if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "RLEnemySpawnRateMultiplier",
                    oldValue,
                    RLEnemySpawnRateMultiplier,
                    "RL Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustRLHealingAssistance(float adjustment, string context = null)
        {
            float oldValue = RLHealingAssistanceMultiplier;
            RLHealingAssistanceMultiplier += adjustment;
            RLHealingAssistanceMultiplier = Mathf.Clamp(RLHealingAssistanceMultiplier, 0.5f, 1.5f);

            Debug.Log($"RL Healing Assistance Multiplier adjusted to: {RLHealingAssistanceMultiplier}. Context: {context}");

            ApplyFinalMultipliers();

            // Log the change if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "RLHealingAssistanceMultiplier",
                    oldValue,
                    RLHealingAssistanceMultiplier,
                    "RL Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustRLEnemyHealth(float adjustment, string context = null) // Added
        {
            float oldValue = RLEnemyHealthMultiplier;
            RLEnemyHealthMultiplier += adjustment;
            RLEnemyHealthMultiplier = Mathf.Clamp(RLEnemyHealthMultiplier, 0.5f, 1.5f);

            Debug.Log($"RL Enemy Health Multiplier adjusted to: {RLEnemyHealthMultiplier}. Context: {context}");

            ApplyFinalMultipliers();

            // Log the change if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "RLEnemyHealthMultiplier",
                    oldValue,
                    RLEnemyHealthMultiplier,
                    "RL Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        // Apply the final multipliers to relevant game objects
        private void ApplyFinalMultipliers()
        {
            // Apply FinalEnemyDamageMultiplier and FinalEnemyHealthMultiplier to all DamageReceivers on the "NPCs" layer
            foreach (var damageReceiver in FindObjectsOfType<DamageReceiver>())
            {
                if (damageReceiver.gameObject.layer == LayerMask.NameToLayer("NPCs"))
                {
                    damageReceiver.SetDamageMultiplier(FinalEnemyDamageMultiplier);
                    damageReceiver.SetHealthMultiplier(FinalEnemyHealthMultiplier); // Added
                }
            }

            // Apply FinalPlayerDamageMultiplier to all DamageReceivers on the "Player" layer
            foreach (var damageReceiver in FindObjectsOfType<DamageReceiver>())
            {
                if (damageReceiver.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    damageReceiver.SetDamageMultiplier(FinalPlayerDamageMultiplier);
                }
            }

            // Apply FinalHealingAssistanceMultiplier indirectly for PCs by ensuring it's used in ReceiveHP
            Debug.Log($"Final Multipliers Applied - Enemy Damage: {FinalEnemyDamageMultiplier}, Enemy Health: {FinalEnemyHealthMultiplier}, Enemy Spawn Rate: {FinalEnemySpawnRateMultiplier}, Healing Assistance: {FinalHealingAssistanceMultiplier}, Player Damage: {FinalPlayerDamageMultiplier}");
        }

        public void AdjustBasePlayerDamage(float adjustment, string context = null)
        {
            float oldValue = BasePlayerDamageMultiplier;
            BasePlayerDamageMultiplier += adjustment;
            BasePlayerDamageMultiplier = Mathf.Clamp(BasePlayerDamageMultiplier, 0.5f, 1.5f);
            Debug.Log($"Base Player Damage Multiplier adjusted to: {BasePlayerDamageMultiplier}. Context: {context}");
            ApplyFinalMultipliers();

            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "BasePlayerDamageMultiplier",
                    oldValue,
                    BasePlayerDamageMultiplier,
                    "Rule-Based Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }

        public void AdjustRLPlayerDamage(float adjustment, string context = null) // Added
        {
            float oldValue = RLPlayerDamageMultiplier;
            RLPlayerDamageMultiplier += adjustment;
            RLPlayerDamageMultiplier = Mathf.Clamp(RLPlayerDamageMultiplier, 0.5f, 1.5f);

            Debug.Log($"RL Player Damage Multiplier adjusted to: {RLPlayerDamageMultiplier}. Context: {context}");

            ApplyFinalMultipliers();

            if (!string.IsNullOrEmpty(context))
            {
                AIChangeLogger.LogChange(
                    "RLPlayerDamageMultiplier",
                    oldValue,
                    RLPlayerDamageMultiplier,
                    "RL Adjustment",
                    context
                );
                AIChangeLogger.SaveLog();
            }
        }
    }
}
