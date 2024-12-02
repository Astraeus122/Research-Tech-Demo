using System;
using System.IO;
using Unity.BossRoom.Gameplay.Metrics;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.BossRoom.Gameplay.AI
{
    public class RLAdaptiveAIManager : MonoBehaviour
    {
        public static RLAdaptiveAIManager Instance { get; private set; }

        private MetricsManager metricsManager;

        // Q-Learning parameters
        private Dictionary<State, Dictionary<Action, float>> QTable = new Dictionary<State, Dictionary<Action, float>>();
        private float learningRate = 0.1f;
        private float discountFactor = 0.9f;
        private float explorationRate = 1.0f;
        private float explorationDecay = 0.995f;
        private float minExplorationRate = 0.01f;

        // Define possible actions
        private Action[] actions = new Action[]
        {
            Action.IncreaseEnemyDamage,
            Action.DecreaseEnemyDamage,
            Action.MaintainEnemyDamage,
            Action.IncreaseEnemySpawnRate,
            Action.DecreaseEnemySpawnRate,
            Action.MaintainEnemySpawnRate,
            Action.IncreaseHealingAssistance,
            Action.DecreaseHealingAssistance,
            Action.MaintainHealingAssistance,
            Action.IncreaseEnemyHealth,
            Action.DecreaseEnemyHealth,
            Action.MaintainEnemyHealth,
            Action.IncreasePlayerDamage,
            Action.DecreasePlayerDamage,
            Action.MaintainPlayerDamage
        };

        // Current state
        private State currentState;

        private ulong playerId;

        // File path for Q-Table
        private string qTableFilePath;

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

            qTableFilePath = Application.dataPath + "/Scripts/RLAdaptiveAIManager/QTable.json";
        }

        private void Start()
        {
            metricsManager = MetricsManager.Instance;
            playerId = GetPlayerId();

            LoadQTable();
            currentState = GetCurrentState();
        }

        private void Update()
        {
            if (Time.time >= nextDecisionTime)
            {
                Step();
                nextDecisionTime = Time.time + decisionInterval;
            }
        }

        private float nextDecisionTime = 0f;
        private float decisionInterval = 10f; // Make a decision every 10 seconds

        private ulong GetPlayerId()
        {
            // Assuming single-player game for simplicity
            foreach (var key in metricsManager.playerSessionTime.Keys)
            {
                return key;
            }
            return 0;
        }

        private State GetCurrentState()
        {
            float deathRate = metricsManager.GetDeathRatePerMinute(playerId);
            float enemiesKilledRate = metricsManager.GetEnemiesKilledPerMinute(playerId);
            float healingReceivedRate = metricsManager.GetHealingReceivedPerMinute(playerId);
            float damageTakenPerMinute = metricsManager.GetDamageTakenPerMinute(playerId);
            float currentHealthPercentage = metricsManager.GetCurrentHealthPercentage(playerId);

            return new State
            {
                DeathRateCategory = Categorize(deathRate, new float[] { 1f, 3f }),
                EnemiesKilledRateCategory = Categorize(enemiesKilledRate, new float[] { 5f, 15f }),
                HealingReceivedRateCategory = Categorize(healingReceivedRate, new float[] { 50f, 150f }),
                DamageTakenRateCategory = Categorize(damageTakenPerMinute, new float[] { 10f, 30f }),
                HealthPercentageCategory = Categorize(currentHealthPercentage, new float[] { 40f, 70f })
            };
        }

        private string Categorize(float value, float[] thresholds)
        {
            if (value < thresholds[0])
                return "Low";
            else if (value < thresholds[1])
                return "Medium";
            else
                return "High";
        }

        private void Step()
        {
            State newState = GetCurrentState();
            Action chosenAction = ChooseAction(currentState);

            ApplyAction(chosenAction);

            float reward = CalculateReward(currentState, newState);
            UpdateQTable(currentState, chosenAction, reward, newState);

            currentState = newState;

            DecayExploration();

            SaveQTable(); // Save after each step
        }

        private Action ChooseAction(State state)
        {
            if (UnityEngine.Random.value < explorationRate)
            {
                // Explore: choose a random action
                int randomIndex = UnityEngine.Random.Range(0, actions.Length);
                return actions[randomIndex];
            }
            else
            {
                // Exploit: choose the best action based on Q-Table
                float maxQ = float.MinValue;
                Action bestAction = Action.MaintainEnemyDamage; // Default action

                if (!QTable.ContainsKey(state))
                {
                    // Initialize Q-values for unseen state
                    QTable[state] = new Dictionary<Action, float>();
                    foreach (var act in actions)
                    {
                        QTable[state][act] = 0f;
                    }
                }

                foreach (var action in actions)
                {
                    if (QTable[state].TryGetValue(action, out float qValue))
                    {
                        if (qValue > maxQ)
                        {
                            maxQ = qValue;
                            bestAction = action;
                        }
                    }
                }

                return bestAction;
            }
        }

        private void ApplyAction(Action action)
        {
            float oldValue, newValue;

            switch (action)
            {
                case Action.IncreaseEnemyDamage:
                    oldValue = DifficultyManager.Instance.RLEnemyDamageMultiplier;
                    DifficultyManager.Instance.AdjustRLEnemyDamage(RuleBasedAIManager.Instance.enemyDamageAdjustmentStep, "RL Adjustment: Increase enemy damage due to player performance");
                    newValue = DifficultyManager.Instance.RLEnemyDamageMultiplier;
                    AIChangeLogger.LogChange("RLEnemyDamageMultiplier", oldValue, newValue, "RL Adjustment", "Increase enemy damage");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.DecreaseEnemyDamage:
                    oldValue = DifficultyManager.Instance.RLEnemyDamageMultiplier;
                    DifficultyManager.Instance.AdjustRLEnemyDamage(-RuleBasedAIManager.Instance.enemyDamageAdjustmentStep, "RL Adjustment: Decrease enemy damage due to player struggles");
                    newValue = DifficultyManager.Instance.RLEnemyDamageMultiplier;
                    AIChangeLogger.LogChange("RLEnemyDamageMultiplier", oldValue, newValue, "RL Adjustment", "Decrease enemy damage");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.IncreaseEnemySpawnRate:
                    oldValue = DifficultyManager.Instance.RLEnemySpawnRateMultiplier;
                    DifficultyManager.Instance.AdjustRLEnemySpawnRate(RuleBasedAIManager.Instance.enemySpawnRateAdjustmentStep, "RL Adjustment: Increase enemy spawn rate for challenge");
                    newValue = DifficultyManager.Instance.RLEnemySpawnRateMultiplier;
                    AIChangeLogger.LogChange("RLEnemySpawnRateMultiplier", oldValue, newValue, "RL Adjustment", "Increase enemy spawn rate");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.DecreaseEnemySpawnRate:
                    oldValue = DifficultyManager.Instance.RLEnemySpawnRateMultiplier;
                    DifficultyManager.Instance.AdjustRLEnemySpawnRate(-RuleBasedAIManager.Instance.enemySpawnRateAdjustmentStep, "RL Adjustment: Decrease enemy spawn rate to ease difficulty");
                    newValue = DifficultyManager.Instance.RLEnemySpawnRateMultiplier;
                    AIChangeLogger.LogChange("RLEnemySpawnRateMultiplier", oldValue, newValue, "RL Adjustment", "Decrease enemy spawn rate");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.IncreaseHealingAssistance:
                    oldValue = DifficultyManager.Instance.RLHealingAssistanceMultiplier;
                    DifficultyManager.Instance.AdjustRLHealingAssistance(RuleBasedAIManager.Instance.healingAssistanceAdjustmentStep, "RL Adjustment: Increase healing assistance for player struggles");
                    newValue = DifficultyManager.Instance.RLHealingAssistanceMultiplier;
                    AIChangeLogger.LogChange("RLHealingAssistanceMultiplier", oldValue, newValue, "RL Adjustment", "Increase healing assistance");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.DecreaseHealingAssistance:
                    oldValue = DifficultyManager.Instance.RLHealingAssistanceMultiplier;
                    DifficultyManager.Instance.AdjustRLHealingAssistance(-RuleBasedAIManager.Instance.healingAssistanceAdjustmentStep, "RL Adjustment: Decrease healing assistance for balanced play");
                    newValue = DifficultyManager.Instance.RLHealingAssistanceMultiplier;
                    AIChangeLogger.LogChange("RLHealingAssistanceMultiplier", oldValue, newValue, "RL Adjustment", "Decrease healing assistance");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.IncreaseEnemyHealth:
                    oldValue = DifficultyManager.Instance.RLEnemyHealthMultiplier;
                    DifficultyManager.Instance.AdjustRLEnemyHealth(RuleBasedAIManager.Instance.enemyHealthAdjustmentStep, "RL Adjustment: Increase enemy health due to player performance");
                    newValue = DifficultyManager.Instance.RLEnemyHealthMultiplier;
                    AIChangeLogger.LogChange("RLEnemyHealthMultiplier", oldValue, newValue, "RL Adjustment", "Increase enemy health");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.DecreaseEnemyHealth:
                    oldValue = DifficultyManager.Instance.RLEnemyHealthMultiplier;
                    DifficultyManager.Instance.AdjustRLEnemyHealth(-RuleBasedAIManager.Instance.enemyHealthAdjustmentStep, "RL Adjustment: Decrease enemy health due to player struggles");
                    newValue = DifficultyManager.Instance.RLEnemyHealthMultiplier;
                    AIChangeLogger.LogChange("RLEnemyHealthMultiplier", oldValue, newValue, "RL Adjustment", "Decrease enemy health");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.IncreasePlayerDamage:
                    oldValue = DifficultyManager.Instance.RLPlayerDamageMultiplier;
                    DifficultyManager.Instance.AdjustRLPlayerDamage(RuleBasedAIManager.Instance.playerDamageAdjustmentStep, "RL Adjustment: Increase player damage due to player struggles");
                    newValue = DifficultyManager.Instance.RLPlayerDamageMultiplier;
                    AIChangeLogger.LogChange("RLPlayerDamageMultiplier", oldValue, newValue, "RL Adjustment", "Increase player damage");
                    AIChangeLogger.SaveLog();
                    break;

                case Action.DecreasePlayerDamage:
                    oldValue = DifficultyManager.Instance.RLPlayerDamageMultiplier;
                    DifficultyManager.Instance.AdjustRLPlayerDamage(-RuleBasedAIManager.Instance.playerDamageAdjustmentStep, "RL Adjustment: Decrease player damage due to player overperformance");
                    newValue = DifficultyManager.Instance.RLPlayerDamageMultiplier;
                    AIChangeLogger.LogChange("RLPlayerDamageMultiplier", oldValue, newValue, "RL Adjustment", "Decrease player damage");
                    AIChangeLogger.SaveLog();
                    break;

                default:
                    Debug.Log($"Action {action} does not modify state.");
                    break;
            }

            Debug.Log($"Applied Action: {action}");
        }

        private float CalculateReward(State previousState, State currentState)
        {
            // Define reward based on transition
            // Positive reward for balanced difficulty, negative otherwise

            // Example criteria:
            bool balancedDeathRate = currentState.DeathRateCategory == "Medium";
            bool balancedEnemiesKilled = currentState.EnemiesKilledRateCategory == "Medium";
            bool balancedHealing = currentState.HealingReceivedRateCategory == "Medium";
            bool balancedDamageTaken = currentState.DamageTakenRateCategory == "Medium";
            bool balancedHealthPercentage = currentState.HealthPercentageCategory == "Medium";

            int balancedMetrics = 0;

            if (balancedDeathRate) balancedMetrics++;
            if (balancedEnemiesKilled) balancedMetrics++;
            if (balancedHealing) balancedMetrics++;
            if (balancedDamageTaken) balancedMetrics++;
            if (balancedHealthPercentage) balancedMetrics++;

            if (balancedMetrics >= 4)
            {
                return 1.0f; // Positive reward
            }
            else if (balancedMetrics <= 2)
            {
                return -1.0f; // Negative reward
            }

            return 0f; // Neutral
        }

        private void UpdateQTable(State state, Action action, float reward, State nextState)
        {
            if (!QTable.ContainsKey(state))
            {
                QTable[state] = new Dictionary<Action, float>();
                foreach (var act in actions)
                {
                    QTable[state][act] = 0f;
                }
            }

            if (!QTable.ContainsKey(nextState))
            {
                QTable[nextState] = new Dictionary<Action, float>();
                foreach (var act in actions)
                {
                    QTable[nextState][act] = 0f;
                }
            }

            float currentQ = QTable[state][action];
            float maxNextQ = float.MinValue;

            foreach (var act in actions)
            {
                if (QTable[nextState][act] > maxNextQ)
                {
                    maxNextQ = QTable[nextState][act];
                }
            }

            if (maxNextQ == float.MinValue)
                maxNextQ = 0f;

            // Q-Learning update rule
            QTable[state][action] = currentQ + learningRate * (reward + discountFactor * maxNextQ - currentQ);
        }

        private void DecayExploration()
        {
            if (explorationRate > minExplorationRate)
            {
                explorationRate *= explorationDecay;
            }
            else
            {
                explorationRate = minExplorationRate;
            }
        }

        // State representation
        private class State
        {
            public string DeathRateCategory { get; set; }
            public string EnemiesKilledRateCategory { get; set; }
            public string HealingReceivedRateCategory { get; set; }
            public string DamageTakenRateCategory { get; set; }
            public string HealthPercentageCategory { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is State other)
                {
                    return DeathRateCategory == other.DeathRateCategory &&
                           EnemiesKilledRateCategory == other.EnemiesKilledRateCategory &&
                           HealingReceivedRateCategory == other.HealingReceivedRateCategory &&
                           DamageTakenRateCategory == other.DamageTakenRateCategory &&
                           HealthPercentageCategory == other.HealthPercentageCategory;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (DeathRateCategory + EnemiesKilledRateCategory + HealingReceivedRateCategory +
                        DamageTakenRateCategory + HealthPercentageCategory).GetHashCode();
            }

            // Convert state to a unique string identifier
            public string ToUniqueString()
            {
                return $"{DeathRateCategory}_{EnemiesKilledRateCategory}_{HealingReceivedRateCategory}_{DamageTakenRateCategory}_{HealthPercentageCategory}";
            }

            // Create a state from a unique string identifier
            public static State FromUniqueString(string uniqueString)
            {
                var parts = uniqueString.Split('_');
                if (parts.Length != 5)
                    throw new Exception("Invalid state string format.");

                return new State
                {
                    DeathRateCategory = parts[0],
                    EnemiesKilledRateCategory = parts[1],
                    HealingReceivedRateCategory = parts[2],
                    DamageTakenRateCategory = parts[3],
                    HealthPercentageCategory = parts[4]
                };
            }
        }

        // Define possible actions
        private enum Action
        {
            IncreaseEnemyDamage,
            DecreaseEnemyDamage,
            MaintainEnemyDamage,
            IncreaseEnemySpawnRate,
            DecreaseEnemySpawnRate,
            MaintainEnemySpawnRate,
            IncreaseHealingAssistance,
            DecreaseHealingAssistance,
            MaintainHealingAssistance,
            IncreaseEnemyHealth,
            DecreaseEnemyHealth,
            MaintainEnemyHealth,
            IncreasePlayerDamage,
            DecreasePlayerDamage,
            MaintainPlayerDamage
        }

        /// <summary>
        /// Saves the Q-Table to a JSON file.
        /// </summary>
        private void SaveQTable()
        {
            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(qTableFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            SerializableQTable serializableQTable = new SerializableQTable();

            foreach (var state in QTable.Keys)
            {
                string stateId = state.ToUniqueString();
                serializableQTable.QTableData[stateId] = new Dictionary<string, float>();

                foreach (var action in QTable[state].Keys)
                {
                    serializableQTable.QTableData[stateId][action.ToString()] = QTable[state][action];
                }
            }

            string json = JsonUtility.ToJson(serializableQTable, prettyPrint: true);
            File.WriteAllText(qTableFilePath, json);
            Debug.Log($"Q-Table saved to {qTableFilePath}");
        }

        /// <summary>
        /// Loads the Q-Table from a JSON file. If the file doesn't exist, initializes a new Q-Table.
        /// </summary>
        private void LoadQTable()
        {
            if (File.Exists(qTableFilePath))
            {
                string json = File.ReadAllText(qTableFilePath);
                SerializableQTable serializableQTable = JsonUtility.FromJson<SerializableQTable>(json);

                foreach (var stateEntry in serializableQTable.QTableData)
                {
                    State state = State.FromUniqueString(stateEntry.Key);
                    QTable[state] = new Dictionary<Action, float>();

                    foreach (var actionEntry in stateEntry.Value)
                    {
                        if (Enum.TryParse<Action>(actionEntry.Key, out Action action))
                        {
                            QTable[state][action] = actionEntry.Value;
                        }
                    }
                }

                Debug.Log($"Q-Table loaded from {qTableFilePath}");
            }
            else
            {
                // No need to initialize here; Q-values will be initialized when states are encountered
                Debug.Log("Q-Table file not found. A new Q-Table will be created during runtime.");
            }
        }

        // Serializable Q-Table class for JSON serialization
        [Serializable]
        private class SerializableQTable
        {
            public Dictionary<string, Dictionary<string, float>> QTableData = new Dictionary<string, Dictionary<string, float>>();
        }
    }
}
