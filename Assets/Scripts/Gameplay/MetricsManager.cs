using System;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using Unity.BossRoom.Gameplay.Actions;
using UnityEngine;

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
        private Dictionary<ulong, float> playerSessionTime = new Dictionary<ulong, float>();
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


        void OnEnable()
        {
            // Subscribe to damage events from DamageReceiver
            DamageReceiver.OnMetricDamageReceived += TrackDamage;

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

        public void TrackDamageByCharacterClass(CharacterTypeEnum characterClass, int damage)
        {
            if (!damageTakenByCharacterClass.ContainsKey(characterClass))
            {
                damageTakenByCharacterClass[characterClass] = 0;
            }
            damageTakenByCharacterClass[characterClass] += damage;

            Debug.Log($"{characterClass} has taken {damageTakenByCharacterClass[characterClass]} total damage.");
        }

        public void TrackDamageDealtByCharacterClass(CharacterTypeEnum characterClass, int damage)
        {
            if (!damageDealtByCharacterClass.ContainsKey(characterClass))
            {
                damageDealtByCharacterClass[characterClass] = 0;
            }
            damageDealtByCharacterClass[characterClass] += damage;

            Debug.Log($"{characterClass} has dealt {damageDealtByCharacterClass[characterClass]} total damage.");
        }

        public void TrackAbilityUseByCharacterClass(CharacterTypeEnum characterClass)
        {
            if (!abilityUsesByCharacterClass.ContainsKey(characterClass))
            {
                abilityUsesByCharacterClass[characterClass] = 0;
            }
            abilityUsesByCharacterClass[characterClass]++;

            Debug.Log($"{characterClass} has used abilities {abilityUsesByCharacterClass[characterClass]} times.");
        }

        public void TrackActionUsage(ActionLogic actionLogic)
        {
            if (!actionCountByType.ContainsKey(actionLogic))
            {
                actionCountByType[actionLogic] = 0;
            }
            actionCountByType[actionLogic]++;

            Debug.Log($"Action {actionLogic} performed {actionCountByType[actionLogic]} times.");
        }

        public void TrackPlayerLogin(ulong playerId)
        {
            if (!playerLoginTimes.ContainsKey(playerId))
            {
                playerLoginTimes[playerId] = DateTime.Now;
                playerSessionTime[playerId] = 0f; // Initialize session time
            }

            Debug.Log($"Player {playerId} logged in at {playerLoginTimes[playerId]}.");
        }

        public void TrackPlayerLogout(ulong playerId)
        {
            if (playerLoginTimes.ContainsKey(playerId))
            {
                DateTime loginTime = playerLoginTimes[playerId];
                float sessionDuration = (float)(DateTime.Now - loginTime).TotalSeconds;
                playerSessionTime[playerId] += sessionDuration;
                playerLoginTimes.Remove(playerId);

                Debug.Log($"Player {playerId} logged out. Session duration: {sessionDuration} seconds. Total playtime: {playerSessionTime[playerId]} seconds.");
            }
        }


        void OnDisable()
        {
            // Unsubscribe from damage events from DamageReceiver
            DamageReceiver.OnMetricDamageReceived -= TrackDamage;

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
        private void TrackDamage(ServerCharacter inflicter, int HP, ulong receiverId)
        {
            if (HP < 0) // Damage is taken (negative value)
            {
                if (!damageTakenByCharacter.ContainsKey(receiverId))
                {
                    damageTakenByCharacter[receiverId] = 0;
                }
                damageTakenByCharacter[receiverId] += -HP; // Convert to positive damage value

                // Optional: Print damage metrics for debugging
                Debug.Log($"Character {receiverId} has taken {damageTakenByCharacter[receiverId]} total damage.");
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

        // Track charged actions
        public void TrackChargedAction(ulong characterId, bool isFullyCharged)
        {
            if (isFullyCharged)
            {
                if (!fullyChargedActionsByCharacter.ContainsKey(characterId))
                {
                    fullyChargedActionsByCharacter[characterId] = 0;
                }
                fullyChargedActionsByCharacter[characterId]++;
                Debug.Log($"Character {characterId} performed a fully charged action. Total count: {fullyChargedActionsByCharacter[characterId]}");
            }
            else
            {
                if (!partiallyChargedActionsByCharacter.ContainsKey(characterId))
                {
                    partiallyChargedActionsByCharacter[characterId] = 0;
                }
                partiallyChargedActionsByCharacter[characterId]++;
                Debug.Log($"Character {characterId} performed a partially charged action. Total count: {partiallyChargedActionsByCharacter[characterId]}");
            }
        }

        // Track shield actions
        public void TrackShieldAction(ulong characterId, bool isFullyCharged)
        {
            if (!shieldActionsUsedByCharacter.ContainsKey(characterId))
            {
                shieldActionsUsedByCharacter[characterId] = 0;
            }
            shieldActionsUsedByCharacter[characterId]++;

            if (isFullyCharged)
            {
                if (!fullyChargedShieldsByCharacter.ContainsKey(characterId))
                {
                    fullyChargedShieldsByCharacter[characterId] = 0;
                }
                fullyChargedShieldsByCharacter[characterId]++;
            }

            Debug.Log($"Character {characterId} used shield action. Total used: {shieldActionsUsedByCharacter[characterId]}, Fully Charged: {fullyChargedShieldsByCharacter.GetValueOrDefault(characterId)}");
        }

        // Track dash attack usage
        public void TrackDashAttack(ulong characterId, int enemiesHit)
        {
            if (!dashAttacksPerformedByCharacter.ContainsKey(characterId))
            {
                dashAttacksPerformedByCharacter[characterId] = 0;
            }
            dashAttacksPerformedByCharacter[characterId]++;

            if (!dashAttackEnemiesHitByCharacter.ContainsKey(characterId))
            {
                dashAttackEnemiesHitByCharacter[characterId] = 0;
            }
            dashAttackEnemiesHitByCharacter[characterId] += enemiesHit;

            Debug.Log($"Character {characterId} performed dash attack. Total performed: {dashAttacksPerformedByCharacter[characterId]}, Enemies Hit: {dashAttackEnemiesHitByCharacter[characterId]}");
        }

        // Track projectile launches and hits
        public void TrackProjectileLaunch(ulong characterId)
        {
            if (!projectilesLaunchedByCharacter.ContainsKey(characterId))
            {
                projectilesLaunchedByCharacter[characterId] = 0;
            }
            projectilesLaunchedByCharacter[characterId]++;
            Debug.Log($"Character {characterId} launched a projectile. Total launched: {projectilesLaunchedByCharacter[characterId]}");
        }

        public void TrackProjectileHit(ulong characterId)
        {
            if (!projectilesHitByCharacter.ContainsKey(characterId))
            {
                projectilesHitByCharacter[characterId] = 0;
            }
            projectilesHitByCharacter[characterId]++;
            Debug.Log($"Character {characterId} hit a target with a projectile. Total hits: {projectilesHitByCharacter[characterId]}");
        }

        // Track melee attacks
        public void TrackMeleeAttack(ulong characterId, bool wasHit)
        {
            if (!meleeAttacksPerformedByCharacter.ContainsKey(characterId))
            {
                meleeAttacksPerformedByCharacter[characterId] = 0;
            }
            meleeAttacksPerformedByCharacter[characterId]++;

            if (wasHit)
            {
                if (!meleeHitsByCharacter.ContainsKey(characterId))
                {
                    meleeHitsByCharacter[characterId] = 0;
                }
                meleeHitsByCharacter[characterId]++;
            }

            Debug.Log($"Character {characterId} performed melee attack. Total performed: {meleeAttacksPerformedByCharacter[characterId]}, Hits: {meleeHitsByCharacter.GetValueOrDefault(characterId)}");
        }

        // Track raybeam actions
        public void TrackRaybeamAction(ulong characterId, bool wasHit)
        {
            if (!raybeamActionsPerformedByCharacter.ContainsKey(characterId))
            {
                raybeamActionsPerformedByCharacter[characterId] = 0;
            }
            raybeamActionsPerformedByCharacter[characterId]++;

            if (wasHit)
            {
                if (!raybeamHitsByCharacter.ContainsKey(characterId))
                {
                    raybeamHitsByCharacter[characterId] = 0;
                }
                raybeamHitsByCharacter[characterId]++;
            }

            Debug.Log($"Character {characterId} performed raybeam attack. Total performed: {raybeamActionsPerformedByCharacter[characterId]}, Hits: {raybeamHitsByCharacter.GetValueOrDefault(characterId)}");
        }

        // Method to be called periodically or during specific events to gather data
        public void LogMetrics()
        {
            foreach (var entry in damageTakenByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Damage Taken: {entry.Value}");
            }

            foreach (var entry in portalBreakCounts)
            {
                Debug.Log($"{entry.Key}: Total Breakables Broken: {entry.Value}");
            }

            foreach (var entry in healthDepletedCount)
            {
                Debug.Log($"Character {entry.Key}: Total Times Health Depleted: {entry.Value}");
            }

            foreach (var entry in healthReplenishedCount)
            {
                Debug.Log($"Character {entry.Key}: Total Times Health Replenished: {entry.Value}");
            }

            Debug.Log($"Total Floor Switch Activations: {floorSwitchActivationCount}");
            Debug.Log($"Total Tossed Item Detonations: {tossedItemDetonationCount}");
            Debug.Log($"Total Player Spawns: {playerSpawnCount}");

            foreach (var entry in playerFaintedCount)
            {
                Debug.Log($"Player {entry.Key}: Total Times Fainted: {entry.Value}");
            }

            foreach (var characterEntry in actionsPerformedByCharacter)
            {
                foreach (var actionEntry in characterEntry.Value)
                {
                    Debug.Log($"Character {characterEntry.Key}: Action {actionEntry.Key} performed {actionEntry.Value} times");
                }
            }

            foreach (var entry in fullyChargedActionsByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Fully Charged Actions: {entry.Value}");
            }

            foreach (var entry in partiallyChargedActionsByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Partially Charged Actions: {entry.Value}");
            }

            foreach (var entry in aoeHitsByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total AoE Hits: {entry.Value}");
            }

            foreach (var entry in projectilesLaunchedByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Projectiles Launched: {entry.Value}");
            }

            foreach (var entry in projectilesHitByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Projectiles Hit: {entry.Value}");
            }

            foreach (var entry in shieldActionsUsedByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Shield Actions Used: {entry.Value}, Fully Charged: {fullyChargedShieldsByCharacter.GetValueOrDefault(entry.Key)}");
            }

            foreach (var entry in dashAttacksPerformedByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Dash Attacks Performed: {entry.Value}, Enemies Hit: {dashAttackEnemiesHitByCharacter.GetValueOrDefault(entry.Key)}");
            }

            foreach (var entry in meleeAttacksPerformedByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Melee Attacks Performed: {entry.Value}, Hits: {meleeHitsByCharacter.GetValueOrDefault(entry.Key)}");
            }

            foreach (var entry in raybeamActionsPerformedByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Total Raybeam Actions Performed: {entry.Value}, Hits: {raybeamHitsByCharacter.GetValueOrDefault(entry.Key)}");
            }

            // Log action cancellations
            foreach (var entry in actionCancellations)
            {
                Debug.Log($"Character {entry.Key}: Total Action Cancellations: {entry.Value}");
            }

            // Log buff usage counts
            foreach (var characterEntry in buffUsageByCharacter)
            {
                foreach (var buffEntry in characterEntry.Value)
                {
                    Debug.Log($"Character {characterEntry.Key}: Buff {buffEntry.Key} used {buffEntry.Value} times");
                }
            }

            // Log queue depth per character
            foreach (var entry in actionQueueDepthByCharacter)
            {
                Debug.Log($"Character {entry.Key}: Current Action Queue Depth: {entry.Value}");
            }
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

        // Track game state changes (e.g., win/loss)
        public void OnGameStateChanged(string gameState)
        {
            Debug.Log($"Game state changed to {gameState}.");
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

        public void TrackHealingReceived(ulong characterId, int healingAmount)
        {
            if (!healingReceivedByCharacter.ContainsKey(characterId))
            {
                healingReceivedByCharacter[characterId] = 0;
            }
            healingReceivedByCharacter[characterId] += healingAmount;

            Debug.Log($"Character {characterId} received {healingAmount} healing. Total healing received: {healingReceivedByCharacter[characterId]}");
        }

        public void TrackModifiedDamageReceived(ulong characterId, int damageAmount)
        {
            if (!modifiedDamageReceivedByCharacter.ContainsKey(characterId))
            {
                modifiedDamageReceivedByCharacter[characterId] = 0;
            }
            modifiedDamageReceivedByCharacter[characterId] += damageAmount;

            Debug.Log($"Character {characterId} received {damageAmount} modified damage. Total modified damage received: {modifiedDamageReceivedByCharacter[characterId]}");
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

        public void TrackActionStart(ulong characterId)
        {
            if (!actionStartsByCharacter.ContainsKey(characterId))
            {
                actionStartsByCharacter[characterId] = 0;
            }
            actionStartsByCharacter[characterId]++;

            Debug.Log($"Character {characterId} started an action. Total action starts: {actionStartsByCharacter[characterId]}");
        }

        public void TrackActionStop(ulong characterId)
        {
            if (!actionStopsByCharacter.ContainsKey(characterId))
            {
                actionStopsByCharacter[characterId] = 0;
            }
            actionStopsByCharacter[characterId]++;

            Debug.Log($"Character {characterId} stopped an action. Total action stops: {actionStopsByCharacter[characterId]}");
        }

        public void TrackActionInterrupt(ulong characterId)
        {
            if (!actionInterruptsByCharacter.ContainsKey(characterId))
            {
                actionInterruptsByCharacter[characterId] = 0;
            }
            actionInterruptsByCharacter[characterId]++;

            Debug.Log($"Character {characterId} interrupted an action. Total action interrupts: {actionInterruptsByCharacter[characterId]}");
        }

        public void TrackMovementStatusChange(ulong characterId)
        {
            if (!movementStatusChangesByCharacter.ContainsKey(characterId))
            {
                movementStatusChangesByCharacter[characterId] = 0;
            }
            movementStatusChangesByCharacter[characterId]++;

            Debug.Log($"Character {characterId} changed movement status. Total movement status changes: {movementStatusChangesByCharacter[characterId]}");
        }

        public void TrackAiDecision(ulong npcId)
        {
            if (!aiDecisionsByNpc.ContainsKey(npcId))
            {
                aiDecisionsByNpc[npcId] = 0;
            }
            aiDecisionsByNpc[npcId]++;

            Debug.Log($"NPC {npcId} made an AI decision. Total AI decisions: {aiDecisionsByNpc[npcId]}");
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

        public void TrackBuffUsage(ulong characterId, Unity.BossRoom.Gameplay.Actions.Action.BuffableValue buffType)
        {
            if (!buffUsageByCharacter.ContainsKey(characterId))
            {
                buffUsageByCharacter[characterId] = new Dictionary<Unity.BossRoom.Gameplay.Actions.Action.BuffableValue, int>();
            }

            if (!buffUsageByCharacter[characterId].ContainsKey(buffType))
            {
                buffUsageByCharacter[characterId][buffType] = 0;
            }

            buffUsageByCharacter[characterId][buffType]++;
            Debug.Log($"Character {characterId} used buff type {buffType}. Total usage: {buffUsageByCharacter[characterId][buffType]}");
        }
        public void TrackActionQueueDepth(ulong characterId, int queueDepth)
        {
            actionQueueDepthByCharacter[characterId] = queueDepth;
            Debug.Log($"Character {characterId} has an action queue depth of {queueDepth}");
        }
        // Get total cancellations by character for adaptive difficulty analysis
        public int GetTotalCancellations(ulong characterId)
        {
            return actionCancellations.TryGetValue(characterId, out int count) ? count : 0;
        }

        // Get the frequency of a specific buff type usage for a character
        public int GetBuffUsageCount(ulong characterId, Unity.BossRoom.Gameplay.Actions.Action.BuffableValue buffType)
        {
            if (buffUsageByCharacter.ContainsKey(characterId) && buffUsageByCharacter[characterId].ContainsKey(buffType))
            {
                return buffUsageByCharacter[characterId][buffType];
            }
            return 0;
        }

        // Get the current queue depth for a character's action queue
        public int GetCurrentQueueDepth(ulong characterId)
        {
            return actionQueueDepthByCharacter.TryGetValue(characterId, out int depth) ? depth : 0;
        }

    }
}
