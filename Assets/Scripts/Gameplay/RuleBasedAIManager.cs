using System;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.Metrics;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.AI
{
    public class RuleBasedAIManager : MonoBehaviour
    {
        public static RuleBasedAIManager Instance { get; private set; }

        // Thresholds
        [Header("Thresholds")]
        public float deathRateThreshold = 0.5f; // deaths per minute
        public float enemiesKilledThreshold = 10f; // enemies killed per minute
        public float healingReceivedThreshold = 100f; // healing received per minute

        // Adjustment steps
        [Header("Adjustment Steps")]
        public float enemyDamageAdjustmentStep = 0.1f; // 10%
        public float enemySpawnRateAdjustmentStep = 0.1f; // 10%
        public float healingAssistanceAdjustmentStep = 0.1f; // 10%

        // Current multipliers
        private float enemyDamageMultiplier = 1f;
        private float enemySpawnRateMultiplier = 1f;
        private float healingAssistanceMultiplier = 1f;
        public float healingMultiplier = 1f; // Default to 1 (no adjustment)

        private MetricsManager metricsManager;

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

        private void Start()
        {
            metricsManager = MetricsManager.Instance;
        }

        private void Update()
        {
            // Evaluate rules periodically, e.g., every 5 seconds
            if (Time.time >= nextEvaluationTime)
            {
                EvaluateRules();
                nextEvaluationTime = Time.time + evaluationInterval;
            }
        }

        private float nextEvaluationTime = 0f;
        private float evaluationInterval = 5f; // Evaluate every 5 seconds

        private void EvaluateRules()
        {
            ulong playerId = GetPlayerId();

            // Rule 1: Adjust enemy damage based on player's death rate
            AdjustEnemyDamageBasedOnDeathRate(playerId);

            // Rule 2: Adjust enemy spawn rate based on enemies killed
            AdjustEnemySpawnRateBasedOnEnemiesKilled(playerId);

            // Rule 3: Adjust healing assistance based on healing received
            AdjustHealingAssistanceBasedOnHealingReceived(playerId);
        }

        private ulong GetPlayerId()
        {
            // Assuming single-player game for simplicity
            foreach (var key in metricsManager.playerSessionTime.Keys)
            {
                return key;
            }
            return 0;
        }

        private void AdjustEnemyDamageBasedOnDeathRate(ulong playerId)
        {
            int deathCount = metricsManager.GetDeathCount(playerId);
            float playTime = metricsManager.GetPlayTime(playerId);

            float deathsPerMinute = (playTime > 0) ? (deathCount / (playTime / 60f)) : 0f;

            if (deathsPerMinute > deathRateThreshold)
            {
                // Reduce enemy damage
                AdjustEnemyDamage(-enemyDamageAdjustmentStep);
            }
            else
            {
                // Increase enemy damage back towards normal
                AdjustEnemyDamage(enemyDamageAdjustmentStep);
            }
        }

        private void AdjustEnemySpawnRateBasedOnEnemiesKilled(ulong playerId)
        {
            int enemiesKilled = metricsManager.GetEnemiesKilled(playerId);
            float playTime = metricsManager.GetPlayTime(playerId);

            float enemiesKilledPerMinute = (playTime > 0) ? (enemiesKilled / (playTime / 60f)) : 0f;

            if (enemiesKilledPerMinute > enemiesKilledThreshold)
            {
                // Increase enemy spawn rate to maintain challenge
                AdjustEnemySpawnRate(enemySpawnRateAdjustmentStep);
            }
            else
            {
                // Decrease enemy spawn rate to prevent boredom
                AdjustEnemySpawnRate(-enemySpawnRateAdjustmentStep);
            }
        }

        public void AdjustEnemyDamage(float adjustment)
        {
            enemyDamageMultiplier += adjustment;
            enemyDamageMultiplier = Mathf.Clamp(enemyDamageMultiplier, 0.5f, 1.5f); // Clamp between 50% and 150%

            // Apply to all enemies
            foreach (var damageReceiver in FindObjectsOfType<DamageReceiver>())
            {
                // Check if the DamageReceiver is initialized as a monster
                if (damageReceiver.IsMonster())
                {
                    damageReceiver.SetDamageMultiplier(enemyDamageMultiplier);
                }
            }

            Debug.Log($"Adjusted Enemy Damage Multiplier to: {enemyDamageMultiplier}");
        }

        public void AdjustEnemySpawnRate(float adjustment)
        {
            enemySpawnRateMultiplier += adjustment;
            enemySpawnRateMultiplier = Mathf.Clamp(enemySpawnRateMultiplier, 0.5f, 1.5f); // Clamp between 50% and 150%

            // Apply to enemy spawner(s )
            foreach (var spawner in FindObjectsOfType<ServerWaveSpawner>())
            {
                spawner.SetSpawnRateMultiplier(enemySpawnRateMultiplier);
            }

            Debug.Log($"Adjusted Enemy Spawn Rate Multiplier to: {enemySpawnRateMultiplier}");
        }

        private void AdjustHealingAssistanceBasedOnHealingReceived(ulong playerId)
        {
            int healingReceived = metricsManager.GetHealingReceived(playerId);
            float playTime = metricsManager.GetPlayTime(playerId);

            float healingReceivedPerMinute = (playTime > 0) ? (healingReceived / (playTime / 60f)) : 0f;

            if (healingReceivedPerMinute < healingReceivedThreshold)
            {
                // Increase healing assistance if healing is too low
                AdjustHealingAssistance(healingAssistanceAdjustmentStep);
            }
            else
            {
                // Decrease healing assistance if healing is too high
                AdjustHealingAssistance(-healingAssistanceAdjustmentStep);
            }
        }
        public void AdjustHealingAssistance(float adjustment)
        {
            healingMultiplier += adjustment;
            healingMultiplier = Mathf.Clamp(healingMultiplier, 0.5f, 1.5f); // Clamp between 50% and 150%

            Debug.Log($"Adjusted Global Healing Multiplier to: {healingMultiplier}");
        }
    }
}
