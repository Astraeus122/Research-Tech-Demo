using System;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using Unity.BossRoom.Gameplay.Actions;
using UnityEngine;
using System.IO;

namespace Unity.BossRoom.Gameplay.Metrics
{
    public class MetricsManager : MonoBehaviour
    {
        public static MetricsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Set the metrics folder path within the project
            metricsFolderPath = Path.Combine(Application.dataPath, "Scripts", "Metrics");

            // Ensure the directory exists
            if (!Directory.Exists(metricsFolderPath))
            {
                Directory.CreateDirectory(metricsFolderPath);
            }
        }


        // Metrics for damage taken by each character
        private Dictionary<ulong, int> damageTakenByCharacter = new Dictionary<ulong, int>();

        // Metrics for enemies killed by each character (placeholder for future use)
        private Dictionary<ulong, int> enemiesKilledByCharacter = new Dictionary<ulong, int>();

        // Metrics for portal break counts
        private Dictionary<string, int> portalBreakCounts = new Dictionary<string, int>();

        // Metrics for health state changes
        private Dictionary<ulong, int> healthDepletedCount = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> healthReplenishedCount = new Dictionary<ulong, int>();

        // Metrics for floor switch interactions
        private int floorSwitchActivationCount = 0;

        // Metrics for tossed items
        private int tossedItemDetonationCount = 0;
        private Dictionary<ulong, int> tossedItemDamageByCharacter = new Dictionary<ulong, int>();

        // Metrics for player spawns
        private int playerSpawnCount = 0;

        // Metrics for player life state changes (e.g., fainted, dead)
        private Dictionary<ulong, int> playerFaintedCount = new Dictionary<ulong, int>();

        // Metrics for actions performed by players
        private Dictionary<ulong, Dictionary<ActionLogic, int>> actionsPerformedByCharacter = new Dictionary<ulong, Dictionary<ActionLogic, int>>();

        // Metrics for charged actions
        private Dictionary<ulong, int> fullyChargedActionsByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> partiallyChargedActionsByCharacter = new Dictionary<ulong, int>();

        // Metrics for AoE hits
        private Dictionary<ulong, int> aoeHitsByCharacter = new Dictionary<ulong, int>();

        // Metrics for projectile accuracy
        private Dictionary<ulong, int> projectilesLaunchedByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> projectilesHitByCharacter = new Dictionary<ulong, int>();

        // Metrics for shield actions
        private Dictionary<ulong, int> shieldActionsUsedByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> fullyChargedShieldsByCharacter = new Dictionary<ulong, int>();

        // Metrics for dash attacks
        private Dictionary<ulong, int> dashAttacksPerformedByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> dashAttackEnemiesHitByCharacter = new Dictionary<ulong, int>();

        // Metrics for melee actions
        private Dictionary<ulong, int> meleeAttacksPerformedByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> meleeHitsByCharacter = new Dictionary<ulong, int>();

        // Metrics for raybeam attacks
        private Dictionary<ulong, int> raybeamActionsPerformedByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> raybeamHitsByCharacter = new Dictionary<ulong, int>();

        private Dictionary<ulong, int> targetAcquisitionCount = new Dictionary<ulong, int>();
        private Dictionary<ulong, float> targetEngagementDuration = new Dictionary<ulong, float>();
        private Dictionary<ulong, float> targetEngagementStartTime = new Dictionary<ulong, float>();

        // Metrics for Trample Actions
        private Dictionary<ulong, int> trampleEnemiesHitCount = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> trampleDamageDealt = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> trampleStunOccurrences = new Dictionary<ulong, int>();

        // Track metrics for different character classes
        private Dictionary<CharacterTypeEnum, int> damageTakenByCharacterClass = new Dictionary<CharacterTypeEnum, int>();
        private Dictionary<CharacterTypeEnum, int> damageDealtByCharacterClass = new Dictionary<CharacterTypeEnum, int>();
        private Dictionary<CharacterTypeEnum, int> abilityUsesByCharacterClass = new Dictionary<CharacterTypeEnum, int>();

        // Track metrics for action types based on ActionLogic
        private Dictionary<ActionLogic, int> actionCountByType = new Dictionary<ActionLogic, int>();

        // Track player session data
        public Dictionary<ulong, float> playerSessionTime = new Dictionary<ulong, float>();
        private Dictionary<ulong, DateTime> playerLoginTimes = new Dictionary<ulong, DateTime>();

        // Health changes (healing and damage with modifiers)
        private Dictionary<ulong, int> healingReceivedByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> modifiedDamageReceivedByCharacter = new Dictionary<ulong, int>();

        // Buff modifications
        private Dictionary<ulong, Dictionary<Unity.BossRoom.Gameplay.Actions.Action.BuffableValue, float>> buffValuesByCharacter = new Dictionary<ulong, Dictionary<Unity.BossRoom.Gameplay.Actions.Action.BuffableValue, float>>();

        // Action-related tracking (starts, stops, interruptions)
        private Dictionary<ulong, int> actionStartsByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> actionInterruptsByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, int> actionStopsByCharacter = new Dictionary<ulong, int>();

        // Movement status changes
        private Dictionary<ulong, int> movementStatusChangesByCharacter = new Dictionary<ulong, int>();

        // NPC-specific AI decisions (for tracking responses to player actions)
        private Dictionary<ulong, int> aiDecisionsByNpc = new Dictionary<ulong, int>();

        // Track the number of times an action is canceled for each character
        private Dictionary<ulong, int> actionCancellations = new Dictionary<ulong, int>();

        // Track buff usage by each character, including both positive (buffs) and negative (debuffs)
        private Dictionary<ulong, Dictionary<Unity.BossRoom.Gameplay.Actions.Action.BuffableValue, int>> buffUsageByCharacter = new Dictionary<ulong, Dictionary<Unity.BossRoom.Gameplay.Actions.Action.BuffableValue, int>>();

        // Track queue depth per character for adaptive analysis
        private Dictionary<ulong, int> actionQueueDepthByCharacter = new Dictionary<ulong, int>();
        private Dictionary<ulong, List<float>> enemyAttackTimestamps = new Dictionary<ulong, List<float>>();


        // Session management variables
        private int currentSessionNumber = 1;
        private GameSessionMetrics currentSessionMetrics;
        private string metricsFolderPath;

        private int playerEnemiesKilled = 0;
        private int playerTotalDamageReceived = 0;
        private int playerTotalHealingReceived = 0;

        private int playerMaxHealthTracked = 0; // To track only once at the start of the game
        private int playerCurrentHealth = 0;    // To track current health
        private float playerHealthPercentage = 0f; // To store the percentage of health remaining
        private ulong playerNetworkObjectId;    // To store the player's NetworkObjectId

        // Helper method to get the file path for the current session
        private string GetSessionFilePath(int sessionNumber)
        {
            string fileName = $"GameSession_{sessionNumber}.json";
            return Path.Combine(metricsFolderPath, fileName);
        }

        private void Start()
        {
            StartNewSession();
        }

        private void StartNewSession()
        {
            currentSessionMetrics = new GameSessionMetrics
            {
                SessionNumber = currentSessionNumber,
                DamageTakenByCharacter = new Dictionary<ulong, int>(damageTakenByCharacter),
                EnemiesKilledByCharacter = new Dictionary<ulong, int>(enemiesKilledByCharacter),
                PortalBreakCounts = new Dictionary<string, int>(portalBreakCounts),
                HealthDepletedCount = new Dictionary<ulong, int>(healthDepletedCount),
                HealthReplenishedCount = new Dictionary<ulong, int>(healthReplenishedCount),
                FloorSwitchActivationCount = floorSwitchActivationCount,
                TossedItemDetonationCount = tossedItemDetonationCount,
                TossedItemDamageByCharacter = new Dictionary<ulong, int>(tossedItemDamageByCharacter),
                PlayerSpawnCount = playerSpawnCount,
                PlayerFaintedCount = new Dictionary<ulong, int>(playerFaintedCount),
                ActionsPerformedByCharacter = ConvertActionsPerformedByCharacter(),
                FullyChargedActionsByCharacter = new Dictionary<ulong, int>(fullyChargedActionsByCharacter),
                PartiallyChargedActionsByCharacter = new Dictionary<ulong, int>(partiallyChargedActionsByCharacter),
                AoeHitsByCharacter = new Dictionary<ulong, int>(aoeHitsByCharacter),
                ProjectilesLaunchedByCharacter = new Dictionary<ulong, int>(projectilesLaunchedByCharacter),
                ProjectilesHitByCharacter = new Dictionary<ulong, int>(projectilesHitByCharacter),
                ShieldActionsUsedByCharacter = new Dictionary<ulong, int>(shieldActionsUsedByCharacter),
                FullyChargedShieldsByCharacter = new Dictionary<ulong, int>(fullyChargedShieldsByCharacter),
                DashAttacksPerformedByCharacter = new Dictionary<ulong, int>(dashAttacksPerformedByCharacter),
                DashAttackEnemiesHitByCharacter = new Dictionary<ulong, int>(dashAttackEnemiesHitByCharacter),
                MeleeAttacksPerformedByCharacter = new Dictionary<ulong, int>(meleeAttacksPerformedByCharacter),
                MeleeHitsByCharacter = new Dictionary<ulong, int>(meleeHitsByCharacter),
                RaybeamActionsPerformedByCharacter = new Dictionary<ulong, int>(raybeamActionsPerformedByCharacter),
                RaybeamHitsByCharacter = new Dictionary<ulong, int>(raybeamHitsByCharacter),
                TargetAcquisitionCount = new Dictionary<ulong, int>(targetAcquisitionCount),
                TargetEngagementDuration = new Dictionary<ulong, float>(targetEngagementDuration),
                TrampleEnemiesHitCount = new Dictionary<ulong, int>(trampleEnemiesHitCount),
                TrampleDamageDealt = new Dictionary<ulong, int>(trampleDamageDealt),
                TrampleStunOccurrences = new Dictionary<ulong, int>(trampleStunOccurrences),
                DamageTakenByCharacterClass = ConvertCharacterTypeEnumDictionary(damageTakenByCharacterClass),
                DamageDealtByCharacterClass = ConvertCharacterTypeEnumDictionary(damageDealtByCharacterClass),
                AbilityUsesByCharacterClass = ConvertCharacterTypeEnumDictionary(abilityUsesByCharacterClass),
                ActionCountByType = ConvertActionLogicDictionary(actionCountByType),
                PlayerSessionTime = new Dictionary<ulong, float>(playerSessionTime),
                HealingReceivedByCharacter = new Dictionary<ulong, int>(healingReceivedByCharacter),
                ModifiedDamageReceivedByCharacter = new Dictionary<ulong, int>(modifiedDamageReceivedByCharacter),
                BuffValuesByCharacter = ConvertBuffValuesByCharacter(),
                ActionStartsByCharacter = new Dictionary<ulong, int>(actionStartsByCharacter),
                ActionInterruptsByCharacter = new Dictionary<ulong, int>(actionInterruptsByCharacter),
                ActionStopsByCharacter = new Dictionary<ulong, int>(actionStopsByCharacter),
                MovementStatusChangesByCharacter = new Dictionary<ulong, int>(movementStatusChangesByCharacter),
                AiDecisionsByNpc = new Dictionary<ulong, int>(aiDecisionsByNpc),
                ActionCancellations = new Dictionary<ulong, int>(actionCancellations),
                BuffUsageByCharacter = ConvertBuffUsageByCharacter(),
                ActionQueueDepthByCharacter = new Dictionary<ulong, int>(actionQueueDepthByCharacter)
            };

            // Start session logic
            Debug.Log($"Starting session {currentSessionNumber}");
        }

        private void EndCurrentSession()
        {
            SaveMetricsToFile();
            currentSessionNumber++;
            ResetMetrics();
            StartNewSession();
        }

        private void SaveMetricsToFile()
        {
            string filePath = GetSessionFilePath(currentSessionMetrics.SessionNumber);

            try
            {
                // Optional: Ensure the directory exists (already handled in Awake)
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string json = JsonUtility.ToJson(currentSessionMetrics, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"Metrics for Session {currentSessionMetrics.SessionNumber} saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save metrics to file: {ex.Message}");
            }
        }

        private void ResetMetrics()
        {
            // Reset all metrics dictionaries and counters
            damageTakenByCharacter.Clear();
            enemiesKilledByCharacter.Clear();
            portalBreakCounts.Clear();
            healthDepletedCount.Clear();
            healthReplenishedCount.Clear();
            floorSwitchActivationCount = 0;
            tossedItemDetonationCount = 0;
            tossedItemDamageByCharacter.Clear();
            playerSpawnCount = 0;
            playerFaintedCount.Clear();
            actionsPerformedByCharacter.Clear();
            fullyChargedActionsByCharacter.Clear();
            partiallyChargedActionsByCharacter.Clear();
            aoeHitsByCharacter.Clear();
            projectilesLaunchedByCharacter.Clear();
            projectilesHitByCharacter.Clear();
            shieldActionsUsedByCharacter.Clear();
            fullyChargedShieldsByCharacter.Clear();
            dashAttacksPerformedByCharacter.Clear();
            dashAttackEnemiesHitByCharacter.Clear();
            meleeAttacksPerformedByCharacter.Clear();
            meleeHitsByCharacter.Clear();
            raybeamActionsPerformedByCharacter.Clear();
            raybeamHitsByCharacter.Clear();
            targetAcquisitionCount.Clear();
            targetEngagementDuration.Clear();
            trampleEnemiesHitCount.Clear();
            trampleDamageDealt.Clear();
            trampleStunOccurrences.Clear();
            damageTakenByCharacterClass.Clear();
            damageDealtByCharacterClass.Clear();
            abilityUsesByCharacterClass.Clear();
            actionCountByType.Clear();
            playerSessionTime.Clear();
            healingReceivedByCharacter.Clear();
            modifiedDamageReceivedByCharacter.Clear();
            buffValuesByCharacter.Clear();
            actionStartsByCharacter.Clear();
            actionInterruptsByCharacter.Clear();
            actionStopsByCharacter.Clear();
            movementStatusChangesByCharacter.Clear();
            aiDecisionsByNpc.Clear();
            actionCancellations.Clear();
            buffUsageByCharacter.Clear();
            actionQueueDepthByCharacter.Clear();
        }

        // Modify OnGameStateChanged to handle session start and end
        public void OnGameStateChanged(string gameState)
        {
            Debug.Log($"Game state changed to {gameState}.");

            if (gameState.Equals("GameStart", StringComparison.OrdinalIgnoreCase))
            {
                StartNewSession();
            }
            else if (gameState.Equals("GameEnd", StringComparison.OrdinalIgnoreCase))
            {
                EndCurrentSession();
            }
        }

        // Conversion methods to handle serialization of complex types
        private Dictionary<ulong, Dictionary<string, int>> ConvertActionsPerformedByCharacter()
        {
            var newDict = new Dictionary<ulong, Dictionary<string, int>>();
            foreach (var kvp in actionsPerformedByCharacter)
            {
                newDict[kvp.Key] = new Dictionary<string, int>();
                foreach (var actionKvp in kvp.Value)
                {
                    newDict[kvp.Key].Add(actionKvp.Key.ToString(), actionKvp.Value);
                }
            }
            return newDict;
        }

        private Dictionary<string, int> ConvertCharacterTypeEnumDictionary(Dictionary<CharacterTypeEnum, int> originalDict)
        {
            var newDict = new Dictionary<string, int>();
            foreach (var kvp in originalDict)
            {
                newDict[kvp.Key.ToString()] = kvp.Value;
            }
            return newDict;
        }

        private Dictionary<string, int> ConvertActionLogicDictionary(Dictionary<ActionLogic, int> originalDict)
        {
            var newDict = new Dictionary<string, int>();
            foreach (var kvp in originalDict)
            {
                newDict[kvp.Key.ToString()] = kvp.Value;
            }
            return newDict;
        }

        private Dictionary<ulong, Dictionary<string, float>> ConvertBuffValuesByCharacter()
        {
            var newDict = new Dictionary<ulong, Dictionary<string, float>>();
            foreach (var kvp in buffValuesByCharacter)
            {
                newDict[kvp.Key] = new Dictionary<string, float>();
                foreach (var buffKvp in kvp.Value)
                {
                    newDict[kvp.Key].Add(buffKvp.Key.ToString(), buffKvp.Value);
                }
            }
            return newDict;
        }

        private Dictionary<ulong, Dictionary<string, int>> ConvertBuffUsageByCharacter()
        {
            var newDict = new Dictionary<ulong, Dictionary<string, int>>();
            foreach (var kvp in buffUsageByCharacter)
            {
                newDict[kvp.Key] = new Dictionary<string, int>();
                foreach (var buffKvp in kvp.Value)
                {
                    newDict[kvp.Key].Add(buffKvp.Key.ToString(), buffKvp.Value);
                }
            }
            return newDict;
        }

        private void OnApplicationQuit()
        {
            if (currentSessionMetrics != null)
            {
                SaveMetricsToFile();
            }
        }

        void OnEnable()
        {
            // Subscribe to damage events from DamageReceiver
            DamageReceiver.OnMetricDamageReceived += TrackDamage;
            DamageReceiver.OnMetricHealingReceived += TrackHealing; // Subscribe to healing

            // Subscribe to breakable events from EnemyPortal objects
            foreach (var portal in FindObjectsOfType<EnemyPortal>())
            {
                foreach (var breakable in portal.m_BreakableElements)
                {
                    breakable.IsBroken.OnValueChanged += OnBreakableStateChanged;
                }
            }

            // Subscribe to health state changes
            foreach (var healthState in FindObjectsOfType<NetworkHealthState>())
            {
                healthState.HitPointsDepleted += OnHitPointsDepleted;
                healthState.HitPointsReplenished += OnHitPointsReplenished;
            }

            // Subscribe to floor switch interactions
            foreach (var floorSwitch in FindObjectsOfType<FloorSwitch>())
            {
                floorSwitch.IsSwitchedOn.OnValueChanged += OnFloorSwitchStateChanged;
            }

            // Subscribe to tossed item detonations
            foreach (var tossedItem in FindObjectsOfType<TossedItem>())
            {
                tossedItem.detonatedCallback.AddListener(OnTossedItemDetonated);
            }
        }

        void OnDisable()
        {
            // Unsubscribe from damage events from DamageReceiver
            DamageReceiver.OnMetricDamageReceived -= TrackDamage;
            DamageReceiver.OnMetricHealingReceived -= TrackHealing; // Unsubscribe from healing

            // Unsubscribe from breakable events from EnemyPortal objects
            foreach (var portal in FindObjectsOfType<EnemyPortal>())
            {
                foreach (var breakable in portal.m_BreakableElements)
                {
                    breakable.IsBroken.OnValueChanged -= OnBreakableStateChanged;
                }
            }

            // Unsubscribe from health state changes
            foreach (var healthState in FindObjectsOfType<NetworkHealthState>())
            {
                healthState.HitPointsDepleted -= OnHitPointsDepleted;
                healthState.HitPointsReplenished -= OnHitPointsReplenished;
            }

            // Unsubscribe from floor switch interactions
            foreach (var floorSwitch in FindObjectsOfType<FloorSwitch>())
            {
                floorSwitch.IsSwitchedOn.OnValueChanged -= OnFloorSwitchStateChanged;
            }

            // Unsubscribe from tossed item detonations
            foreach (var tossedItem in FindObjectsOfType<TossedItem>())
            {
                tossedItem.detonatedCallback.RemoveListener(OnTossedItemDetonated);
            }
        }

        // Track damage metrics from damage received events
        public void TrackDamage(ServerCharacter inflicter, int HP, ulong receiverId)
        {
            if (HP < 0) // Damage is taken (negative value)
            {
                if (receiverId == 0) // If the receiver is the player
                {
                    playerTotalDamageReceived += -HP; // Convert to positive damage value
                    Debug.Log($"Player has taken {playerTotalDamageReceived} total damage.");
                    UpdatePlayerHealth(playerTotalDamageReceived);
                }

                if (!damageTakenByCharacter.ContainsKey(receiverId))
                {
                    damageTakenByCharacter[receiverId] = 0;
                }
                damageTakenByCharacter[receiverId] += -HP;
            }
        }

        // Track breakable state changes for portals
        private void OnBreakableStateChanged(bool wasBroken, bool isBroken)
        {
            if (!wasBroken && isBroken)
            {
                string portalName = "Portal_" + gameObject.GetInstanceID().ToString();

                if (!portalBreakCounts.ContainsKey(portalName))
                {
                    portalBreakCounts[portalName] = 0;
                }
                portalBreakCounts[portalName]++;

                Debug.Log($"Portal {portalName} had a breakable broken! Total broken: {portalBreakCounts[portalName]}");
            }
        }

        // Track health state changes when hit points are depleted
        private void OnHitPointsDepleted()
        {
            ulong characterId = 0; // Placeholder: Update to use actual character ID if available
            if (!healthDepletedCount.ContainsKey(characterId))
            {
                healthDepletedCount[characterId] = 0;
            }
            healthDepletedCount[characterId]++;

            Debug.Log($"Character {characterId} has depleted their health {healthDepletedCount[characterId]} times.");
        }

        // Track health state changes when hit points are replenished
        private void OnHitPointsReplenished()
        {
            ulong characterId = 0; // Placeholder: Update to use actual character ID if available
            if (!healthReplenishedCount.ContainsKey(characterId))
            {
                healthReplenishedCount[characterId] = 0;
            }
            healthReplenishedCount[characterId]++;

            Debug.Log($"Character {characterId} has replenished their health {healthReplenishedCount[characterId]} times.");
        }

        // Track floor switch interactions
        private void OnFloorSwitchStateChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                floorSwitchActivationCount++;
                Debug.Log($"Floor switch activated {floorSwitchActivationCount} times.");
            }
        }

        // Track tossed item detonations
        private void OnTossedItemDetonated()
        {
            tossedItemDetonationCount++;
            Debug.Log($"Tossed item detonated {tossedItemDetonationCount} times.");
        }

        // Track actions performed by players
        public void TrackActionPerformed(ulong characterId, ActionLogic actionLogic)
        {
            if (!actionsPerformedByCharacter.ContainsKey(characterId))
            {
                actionsPerformedByCharacter[characterId] = new Dictionary<ActionLogic, int>();
            }

            if (!actionsPerformedByCharacter[characterId].ContainsKey(actionLogic))
            {
                actionsPerformedByCharacter[characterId][actionLogic] = 0;
            }

            actionsPerformedByCharacter[characterId][actionLogic]++;

            Debug.Log($"Character {characterId} performed action {actionLogic}. Total count: {actionsPerformedByCharacter[characterId][actionLogic]}");
        }

        // Track player spawns
        public void OnPlayerSpawned(ulong playerId)
        {
            playerSpawnCount++;
            Debug.Log($"Player {playerId} spawned. Total spawns: {playerSpawnCount}");
        }

        // Track player life state changes (e.g., fainted, dead)
        public void OnLifeStateChanged(ulong playerId, LifeState newState)
        {
            if (newState == LifeState.Fainted)
            {
                if (!playerFaintedCount.ContainsKey(playerId))
                {
                    playerFaintedCount[playerId] = 0;
                }
                playerFaintedCount[playerId]++;
                Debug.Log($"Player {playerId} fainted {playerFaintedCount[playerId]} times.");
            }
        }

        // Track target acquisition
        public void TrackTargetAcquisition(ulong characterId, ulong targetId)
        {
            if (!targetAcquisitionCount.ContainsKey(characterId))
            {
                targetAcquisitionCount[characterId] = 0;
            }
            targetAcquisitionCount[characterId]++;

            // Start engagement timer
            targetEngagementStartTime[targetId] = Time.time;

            Debug.Log($"Character {characterId} acquired a new target: {targetId}. Total acquisitions: {targetAcquisitionCount[characterId]}");
        }

        // Track target release or death
        public void TrackTargetEngagementEnd(ulong targetId)
        {
            if (targetEngagementStartTime.ContainsKey(targetId))
            {
                float duration = Time.time - targetEngagementStartTime[targetId];
                targetEngagementDuration[targetId] = duration;

                Debug.Log($"Target {targetId} was engaged for {duration} seconds.");

                // Remove tracking data
                targetEngagementStartTime.Remove(targetId);
            }
        }

        public void TrackTrampleHit(ulong characterId, int enemiesHit)
        {
            if (!trampleEnemiesHitCount.ContainsKey(characterId))
            {
                trampleEnemiesHitCount[characterId] = 0;
            }
            trampleEnemiesHitCount[characterId] += enemiesHit;

            Debug.Log($"Character {characterId} hit {enemiesHit} enemies during a trample. Total hits: {trampleEnemiesHitCount[characterId]}");
        }

        // Track damage dealt during a trample action
        public void TrackTrampleDamage(ulong characterId, int damageDealt)
        {
            if (!trampleDamageDealt.ContainsKey(characterId))
            {
                trampleDamageDealt[characterId] = 0;
            }
            trampleDamageDealt[characterId] += damageDealt;

            Debug.Log($"Character {characterId} dealt {damageDealt} damage during a trample. Total damage: {trampleDamageDealt[characterId]}");
        }

        // Track stun occurrences due to trample
        public void TrackTrampleStun(ulong characterId)
        {
            if (!trampleStunOccurrences.ContainsKey(characterId))
            {
                trampleStunOccurrences[characterId] = 0;
            }
            trampleStunOccurrences[characterId]++;

            Debug.Log($"Character {characterId} was stunned during a trample. Total stuns: {trampleStunOccurrences[characterId]}");
        }

        public void TrackBuffModification(ulong characterId, Unity.BossRoom.Gameplay.Actions.Action.BuffableValue buffType, float value)
        {
            if (!buffValuesByCharacter.ContainsKey(characterId))
            {
                buffValuesByCharacter[characterId] = new Dictionary<Unity.BossRoom.Gameplay.Actions.Action.BuffableValue, float>();
            }
            buffValuesByCharacter[characterId][buffType] = value;

            Debug.Log($"Character {characterId} has a buff modification for {buffType} with value {value}");
        }

        public void TrackActionCanceled(ulong characterId, ActionLogic actionLogic)
        {
            if (!actionCancellations.ContainsKey(characterId))
            {
                actionCancellations[characterId] = 0;
            }
            actionCancellations[characterId]++;
            Debug.Log($"Character {characterId} canceled action {actionLogic}. Total cancellations: {actionCancellations[characterId]}");
        }
        public int GetDeathCount(ulong playerId)
        {
            return healthDepletedCount.TryGetValue(playerId, out int count) ? count : 0;
        }

        public float GetPlayTime(ulong playerId)
        {
            return playerSessionTime.TryGetValue(playerId, out float time) ? time : 0f;
        }

        public int GetEnemiesKilled(ulong playerId)
        {
            return enemiesKilledByCharacter.TryGetValue(playerId, out int kills) ? kills : 0;
        }
        public int GetHealingReceived(ulong characterId)
        {
            return healingReceivedByCharacter.TryGetValue(characterId, out int healingAmount) ? healingAmount : 0;
        }
        public void TrackHealing(ServerCharacter healer, int healingAmount, ulong receiverId)
        {
            if (receiverId == 0) // If the receiver is the player
            {
                playerTotalHealingReceived += healingAmount;
                Debug.Log($"Player has received {playerTotalHealingReceived} total healing.");
                UpdatePlayerHealth(playerTotalDamageReceived);
            }

            if (!healingReceivedByCharacter.ContainsKey(receiverId))
            {
                healingReceivedByCharacter[receiverId] = 0;
            }
            healingReceivedByCharacter[receiverId] += healingAmount;
        }

        public float GetDeathRatePerMinute(ulong playerId)
        {
            float playTime = GetPlayTime(playerId);
            return (playTime > 0) ? (GetDeathCount(playerId) / (playTime / 60f)) : 0f;
        }

        public float GetEnemiesKilledPerMinute(ulong playerId)
        {
            float playTime = GetPlayTime(playerId);
            return (playTime > 0) ? (GetEnemiesKilled(playerId) / (playTime / 60f)) : 0f;
        }

        public float GetHealingReceivedPerMinute(ulong playerId)
        {
            float playTime = GetPlayTime(playerId);
            return (playTime > 0) ? (GetHealingReceived(playerId) / (playTime / 60f)) : 0f;
        }

        public void TrackEnemyKilled(ulong killerId, GameObject killedEntity)
        {
            if (killedEntity.layer == LayerMask.NameToLayer("NPCs"))
            {
                if (killerId == 0) // Check if it's the player
                {
                    playerEnemiesKilled++;
                    Debug.Log($"Player killed an enemy. Total kills: {playerEnemiesKilled}");
                }

                if (!enemiesKilledByCharacter.ContainsKey(killerId))
                {
                    enemiesKilledByCharacter[killerId] = 0;
                }
                enemiesKilledByCharacter[killerId]++;
            }
            else
            {
                Debug.Log($"Character {killerId} killed a non-enemy entity. No metrics recorded.");
            }
        }

        // New getters
        public int GetPlayerEnemiesKilled()
        {
            return playerEnemiesKilled;
        }

        public int GetPlayerTotalDamageReceived()
        {
            return playerTotalDamageReceived;
        }

        public int GetPlayerTotalHealingReceived()
        {
            return playerTotalHealingReceived;
        }

        public void SetPlayerMaxHealth(int maxHealth)
        {
            if (playerMaxHealthTracked == 0)
            {
                playerMaxHealthTracked = maxHealth;
                Debug.Log($"Player max health set to {playerMaxHealthTracked}");
            }
        }

        public void UpdatePlayerHealth(int currentHealth)
        {
            playerCurrentHealth = currentHealth;
            CalculatePlayerHealthPercentage();
        }

        private void CalculatePlayerHealthPercentage()
        {
            if (playerMaxHealthTracked > 0)
            {
                playerHealthPercentage = (float)playerCurrentHealth / playerMaxHealthTracked * 100f;
                Debug.Log($"Player current health: {playerCurrentHealth}/{playerMaxHealthTracked} ({playerHealthPercentage}%)");
            }
        }

        public float GetDamageTakenPerMinute(ulong playerId)
        {
            float playTime = GetPlayTime(playerId);
            int damageTaken = GetPlayerTotalDamageReceived();

            return (playTime > 0) ? (damageTaken / (playTime / 60f)) : 0f;
        }

        public float GetCurrentHealthPercentage(ulong playerId)
        {
            if (playerMaxHealthTracked > 0)
            {
                return (float)playerCurrentHealth / playerMaxHealthTracked * 100f;
            }
            return 0f;
        }

    }
}
