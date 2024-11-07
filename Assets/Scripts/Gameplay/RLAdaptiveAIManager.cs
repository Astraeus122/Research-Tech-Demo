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
            Action.MaintainHealingAssistance
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

            // Adjusted file path
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

        private void InitializeQTable()
        {
            // Initialize Q-Table with possible states and actions
            // For simplicity, discretize the state space

            foreach (var state in GeneratePossibleStates())
            {
                QTable[state] = new Dictionary<Action, float>();
                foreach (var action in actions)
                {
                    QTable[state][action] = 0f; // Initialize Q-values to 0
                }
            }
        }

        private List<State> GeneratePossibleStates()
        {
            // Discretize metrics into bins
            // Example: Death Rate (Low, Medium, High), etc.

            List<State> states = new List<State>();

            var deathRates = new float[] { 0f, 1f, 3f }; // per minute
            var enemiesKilledRates = new float[] { 0f, 5f, 15f };
            var healingReceivedRates = new float[] { 0f, 50f, 150f };

            foreach (var deathRate in deathRates)
            {
                foreach (var enemiesKilledRate in enemiesKilledRates)
                {
                    foreach (var healingReceivedRate in healingReceivedRates)
                    {
                        states.Add(new State
                        {
                            DeathRateCategory = Categorize(deathRate, new float[] { 1f, 3f }),
                            EnemiesKilledRateCategory = Categorize(enemiesKilledRate, new float[] { 5f, 15f }),
                            HealingReceivedRateCategory = Categorize(healingReceivedRate, new float[] { 50f, 150f })
                        });
                    }
                }
            }

            return states;
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

        private State GetCurrentState()
        {
            float deathRate = metricsManager.GetDeathRatePerMinute(playerId);
            float enemiesKilledRate = metricsManager.GetEnemiesKilledPerMinute(playerId);
            float healingReceivedRate = metricsManager.GetHealingReceivedPerMinute(playerId);

            return new State
            {
                DeathRateCategory = Categorize(deathRate, new float[] { 1f, 3f }),
                EnemiesKilledRateCategory = Categorize(enemiesKilledRate, new float[] { 5f, 15f }),
                HealingReceivedRateCategory = Categorize(healingReceivedRate, new float[] { 50f, 150f })
            };
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
            switch (action)
            {
                case Action.IncreaseEnemyDamage:
                    DifficultyManager.Instance.AdjustRLEnemyDamage(RuleBasedAIManager.Instance.enemyDamageAdjustmentStep);
                    break;
                case Action.DecreaseEnemyDamage:
                    DifficultyManager.Instance.AdjustRLEnemyDamage(-RuleBasedAIManager.Instance.enemyDamageAdjustmentStep);
                    break;
                case Action.MaintainEnemyDamage:
                    // Do nothing
                    break;
                case Action.IncreaseEnemySpawnRate:
                    DifficultyManager.Instance.AdjustRLEnemySpawnRate(RuleBasedAIManager.Instance.enemySpawnRateAdjustmentStep);
                    break;
                case Action.DecreaseEnemySpawnRate:
                    DifficultyManager.Instance.AdjustRLEnemySpawnRate(-RuleBasedAIManager.Instance.enemySpawnRateAdjustmentStep);
                    break;
                case Action.MaintainEnemySpawnRate:
                    // Do nothing
                    break;
                case Action.IncreaseHealingAssistance:
                    DifficultyManager.Instance.AdjustRLHealingAssistance(RuleBasedAIManager.Instance.healingAssistanceAdjustmentStep);
                    break;
                case Action.DecreaseHealingAssistance:
                    DifficultyManager.Instance.AdjustRLHealingAssistance(-RuleBasedAIManager.Instance.healingAssistanceAdjustmentStep);
                    break;
                case Action.MaintainHealingAssistance:
                    // Do nothing
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

            if (balancedDeathRate && balancedEnemiesKilled && balancedHealing)
            {
                return 1.0f; // Positive reward
            }
            else if (!balancedDeathRate || !balancedEnemiesKilled || !balancedHealing)
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

            public override bool Equals(object obj)
            {
                if (obj is State other)
                {
                    return DeathRateCategory == other.DeathRateCategory &&
                           EnemiesKilledRateCategory == other.EnemiesKilledRateCategory &&
                           HealingReceivedRateCategory == other.HealingReceivedRateCategory;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (DeathRateCategory + EnemiesKilledRateCategory + HealingReceivedRateCategory).GetHashCode();
            }

            // Convert state to a unique string identifier
            public string ToUniqueString()
            {
                return $"{DeathRateCategory}_{EnemiesKilledRateCategory}_{HealingReceivedRateCategory}";
            }

            // Create a state from a unique string identifier
            public static State FromUniqueString(string uniqueString)
            {
                var parts = uniqueString.Split('_');
                if (parts.Length != 3)
                    throw new Exception("Invalid state string format.");

                return new State
                {
                    DeathRateCategory = parts[0],
                    EnemiesKilledRateCategory = parts[1],
                    HealingReceivedRateCategory = parts[2]
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
            MaintainHealingAssistance
        }

        // <summary>
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
                InitializeQTable();
                Debug.Log("Q-Table initialized with default values.");
            }
        }
    }
}
