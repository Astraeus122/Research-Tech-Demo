using System.Collections;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
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

        [Header("Health and Damage Thresholds")]
        public float damageTakenPerMinuteThreshold = 20f; // Adjust as needed
        public float highHealthPercentageThreshold = 80f; // Adjust as needed
        public float lowHealthPercentageThreshold = 20f; // Adjust as needed

        // Current multipliers
        private float enemyDamageMultiplier = 1f;
        private float enemySpawnRateMultiplier = 1f;
        private float healingAssistanceMultiplier = 1f;
        private float enemyHealthMultiplier = 1f;
        private float playerDamageMultiplier = 1f;

        [Header("Kill Rate Thresholds")]
        public float highKillRateThreshold = 20f;
        public float lowKillRateThreshold = 5f;

        [Header("Adjustment Steps")]
        public float playerDamageAdjustmentStep = 0.05f;
        public float enemyDamageAdjustmentStep = 0.05f; // 5%
        public float enemySpawnRateAdjustmentStep = 0.05f; // 5%
        public float enemyHealthAdjustmentStep = 0.05f; // 5%
        public float healingAssistanceAdjustmentStep = 0.05f; // 5%

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
            // Evaluate rules periodically, e.g., every 15 seconds
            if (Time.time >= nextEvaluationTime)
            {
                EvaluateRules();
                nextEvaluationTime = Time.time + evaluationInterval;
            }
        }

        private bool isAdjusting = false;
        private IEnumerator AdjustmentCooldown(float cooldownTime)
        {
            isAdjusting = true;
            yield return new WaitForSeconds(cooldownTime);
            isAdjusting = false;
        }

        private float nextEvaluationTime = 0f;
        private float evaluationInterval = 15f; // Evaluate every 15 seconds

        private void EvaluateRules()
        {
            if (isAdjusting) return;

            ulong playerId = GetPlayerId();

            // Gather metrics
            int deathCount = metricsManager.GetDeathCount(playerId);
            float playTime = metricsManager.GetPlayTime(playerId);
            float deathsPerMinute = (playTime > 0) ? (deathCount / (playTime / 60f)) : 0f;
            int enemiesKilled = metricsManager.GetEnemiesKilled(playerId);
            float enemiesKilledPerMinute = metricsManager.GetEnemiesKilledPerMinute(playerId);
            int healingReceived = metricsManager.GetHealingReceived(playerId);
            float healingReceivedPerMinute = metricsManager.GetHealingReceivedPerMinute(playerId);
            float damageTakenPerMinute = metricsManager.GetDamageTakenPerMinute(playerId);
            float currentHealthPercentage = metricsManager.GetCurrentHealthPercentage(playerId);

            Debug.Log($"Player Metrics - Deaths/Min: {deathsPerMinute}, Kills/Min: {enemiesKilledPerMinute}, Healing/Min: {healingReceivedPerMinute}, DamageTaken/Min: {damageTakenPerMinute}, Health%: {currentHealthPercentage}");

            // Perform adjustments
            AdjustEnemyDamageBasedOnPlayerHealth(playerId);
            AdjustEnemySpawnRateBasedOnEnemiesKilled(playerId);
            AdjustEnemyHealthBasedOnEnemiesKilled(playerId);
            AdjustHealingAssistanceBasedOnHealingReceived(playerId);
            AdjustPlayerDamageBasedOnPerformance(playerId);

            // Start cooldown
            StartCoroutine(AdjustmentCooldown(30f)); // 30 seconds cooldown
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

        private void AdjustEnemyDamageBasedOnPlayerHealth(ulong playerId)
        {
            float damageTakenPerMinute = metricsManager.GetDamageTakenPerMinute(playerId);
            float currentHealthPercentage = metricsManager.GetCurrentHealthPercentage(playerId);

            if (damageTakenPerMinute < damageTakenPerMinuteThreshold && currentHealthPercentage > highHealthPercentageThreshold)
            {
                // Increase enemy damage since player is taking little damage and has high health
                AdjustEnemyDamage(enemyDamageAdjustmentStep, "Player taking little damage and high health");
            }
            else if (damageTakenPerMinute > damageTakenPerMinuteThreshold && currentHealthPercentage < lowHealthPercentageThreshold)
            {
                // Decrease enemy damage since player is taking too much damage and has low health
                AdjustEnemyDamage(-enemyDamageAdjustmentStep, "Player taking high damage and low health");
            }
            else
            {
                Debug.Log("No adjustment to enemy damage based on player health and damage taken.");
            }
        }

        private void AdjustEnemySpawnRateBasedOnEnemiesKilled(ulong playerId)
        {
            float enemiesKilledPerMinute = metricsManager.GetEnemiesKilledPerMinute(playerId);

            if (enemiesKilledPerMinute > highKillRateThreshold)
            {
                // Increase enemy spawn rate to maintain challenge
                AdjustEnemySpawnRate(enemySpawnRateAdjustmentStep, "Player killing enemies too quickly");
            }
            else if (enemiesKilledPerMinute < lowKillRateThreshold)
            {
                // Decrease enemy spawn rate to avoid overwhelming the player
                AdjustEnemySpawnRate(-enemySpawnRateAdjustmentStep, "Player struggling to kill enemies");
            }
            else
            {
                Debug.Log("No adjustment to enemy spawn rate based on enemies killed per minute.");
            }
        }

        private void AdjustEnemyHealthBasedOnEnemiesKilled(ulong playerId)
        {
            float enemiesKilledPerMinute = metricsManager.GetEnemiesKilledPerMinute(playerId);

            if (enemiesKilledPerMinute > highKillRateThreshold)
            {
                // Increase enemy health to make them harder to kill
                AdjustEnemyHealth(enemyHealthAdjustmentStep, "Player killing enemies too quickly");
            }
            else if (enemiesKilledPerMinute < lowKillRateThreshold)
            {
                // Decrease enemy health to make them easier to kill
                AdjustEnemyHealth(-enemyHealthAdjustmentStep, "Player struggling to kill enemies");
            }
            else
            {
                Debug.Log("No adjustment to enemy health based on enemies killed per minute.");
            }
        }

        private void AdjustHealingAssistanceBasedOnHealingReceived(ulong playerId)
        {
            float healingReceivedPerMinute = metricsManager.GetHealingReceivedPerMinute(playerId);
            float damageTakenPerMinute = metricsManager.GetDamageTakenPerMinute(playerId);

            if (healingReceivedPerMinute < healingReceivedThreshold && damageTakenPerMinute > damageTakenPerMinuteThreshold)
            {
                // Increase healing assistance if healing is too low and damage taken is high
                AdjustHealingAssistance(healingAssistanceAdjustmentStep, "Low healing received and high damage taken");
            }
            else if (healingReceivedPerMinute > healingReceivedThreshold && damageTakenPerMinute < damageTakenPerMinuteThreshold)
            {
                // Decrease healing assistance if healing is too high and damage taken is low
                AdjustHealingAssistance(-healingAssistanceAdjustmentStep, "High healing received and low damage taken");
            }
            else
            {
                Debug.Log("No adjustment to healing assistance based on healing received and damage taken.");
            }
        }

        private void AdjustPlayerDamageBasedOnPerformance(ulong playerId)
        {
            float enemiesKilledPerMinute = metricsManager.GetEnemiesKilledPerMinute(playerId);

            if (enemiesKilledPerMinute > highKillRateThreshold)
            {
                // Decrease player damage to balance
                AdjustPlayerDamage(-playerDamageAdjustmentStep, "Player killing enemies too quickly");
            }
            else if (enemiesKilledPerMinute < lowKillRateThreshold)
            {
                // Increase player damage to help balance
                AdjustPlayerDamage(playerDamageAdjustmentStep, "Player struggling to kill enemies");
            }
            else
            {
                Debug.Log("No adjustment to player damage based on performance.");
            }
        }

        public void AdjustEnemyDamage(float adjustment, string context)
        {
            float oldValue = enemyDamageMultiplier;
            enemyDamageMultiplier += adjustment;
            enemyDamageMultiplier = Mathf.Clamp(enemyDamageMultiplier, 0.5f, 1.5f); // Clamp between 50% and 150%

            foreach (var damageReceiver in FindObjectsOfType<DamageReceiver>())
            {
                if (damageReceiver.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    damageReceiver.SetDamageMultiplier(enemyDamageMultiplier);
                }
            }

            Debug.Log($"Adjusted Enemy Damage Multiplier to: {enemyDamageMultiplier}. Context: {context}");

            // Log the change
            AIChangeLogger.LogChange(
                "EnemyDamageMultiplier",
                oldValue,
                enemyDamageMultiplier,
                "Rule-Based Adjustment",
                context
            );
            AIChangeLogger.SaveLog();
        }

        public void AdjustEnemySpawnRate(float adjustment, string context)
        {
            float oldValue = enemySpawnRateMultiplier;
            enemySpawnRateMultiplier += adjustment;
            enemySpawnRateMultiplier = Mathf.Clamp(enemySpawnRateMultiplier, 0.5f, 1.5f); // Clamp between 50% and 150%

            foreach (var spawner in FindObjectsOfType<ServerWaveSpawner>())
            {
                spawner.SetSpawnRateMultiplier(enemySpawnRateMultiplier);
            }

            Debug.Log($"Adjusted Enemy Spawn Rate Multiplier to: {enemySpawnRateMultiplier}. Context: {context}");

            // Log the change
            AIChangeLogger.LogChange(
                "EnemySpawnRateMultiplier",
                oldValue,
                enemySpawnRateMultiplier,
                "Rule-Based Adjustment",
                context
            );
            AIChangeLogger.SaveLog();
        }

        public void AdjustEnemyHealth(float adjustment, string context)
        {
            float oldValue = enemyHealthMultiplier;
            enemyHealthMultiplier += adjustment;
            enemyHealthMultiplier = Mathf.Clamp(enemyHealthMultiplier, 0.5f, 1.5f);

            foreach (var enemy in FindObjectsOfType<ServerCharacter>())
            {
                if (enemy.IsNpc)
                {
                    var damageReceiver = enemy.GetComponent<DamageReceiver>();
                    if (damageReceiver != null)
                    {
                        damageReceiver.SetHealthMultiplier(enemyHealthMultiplier);
                    }
                }
            }

            Debug.Log($"Adjusted Enemy Health Multiplier to: {enemyHealthMultiplier}. Context: {context}");

            // Log the change
            AIChangeLogger.LogChange(
                "EnemyHealthMultiplier",
                oldValue,
                enemyHealthMultiplier,
                "Rule-Based Adjustment",
                context
            );
            AIChangeLogger.SaveLog();
        }

        public void AdjustHealingAssistance(float adjustment, string context)
        {
            float oldValue = healingAssistanceMultiplier;
            healingAssistanceMultiplier += adjustment;
            healingAssistanceMultiplier = Mathf.Clamp(healingAssistanceMultiplier, 0.5f, 1.5f); // Clamp between 50% and 150%

            // Apply the healing assistance multiplier to relevant game components
            // For example, adjust the healing amount of health pickups or abilities

            Debug.Log($"Adjusted Healing Assistance Multiplier to: {healingAssistanceMultiplier}. Context: {context}");

            // Log the change
            AIChangeLogger.LogChange(
                "HealingAssistanceMultiplier",
                oldValue,
                healingAssistanceMultiplier,
                "Rule-Based Adjustment",
                context
            );
            AIChangeLogger.SaveLog();
        }

        public void AdjustPlayerDamage(float adjustment, string context)
        {
            float oldValue = playerDamageMultiplier;
            playerDamageMultiplier += adjustment;
            playerDamageMultiplier = Mathf.Clamp(playerDamageMultiplier, 0.5f, 1.5f);

            foreach (var player in FindObjectsOfType<ServerCharacter>())
            {
                if (!player.IsNpc)
                {
                    var damageReceiver = player.GetComponent<DamageReceiver>();
                    if (damageReceiver != null)
                    {
                        damageReceiver.SetDamageMultiplier(playerDamageMultiplier);
                    }
                }
            }

            Debug.Log($"Adjusted Player Damage Multiplier to: {playerDamageMultiplier}. Context: {context}");

            // Log the change
            AIChangeLogger.LogChange(
                "PlayerDamageMultiplier",
                oldValue,
                playerDamageMultiplier,
                "Rule-Based Adjustment",
                context
            );
            AIChangeLogger.SaveLog();
        }
    }
}
