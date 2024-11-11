using System;
using System.Collections.Generic;

namespace Unity.BossRoom.Gameplay.Metrics
{
    [Serializable]
    public class GameSessionMetrics
    {
        public int SessionNumber;

        // Metrics for damage taken by each character
        public Dictionary<ulong, int> DamageTakenByCharacter;

        // Metrics for enemies killed by each character
        public Dictionary<ulong, int> EnemiesKilledByCharacter;

        // Metrics for portal break counts
        public Dictionary<string, int> PortalBreakCounts;

        // Metrics for health state changes
        public Dictionary<ulong, int> HealthDepletedCount;
        public Dictionary<ulong, int> HealthReplenishedCount;

        // Metrics for floor switch interactions
        public int FloorSwitchActivationCount;

        // Metrics for tossed items
        public int TossedItemDetonationCount;
        public Dictionary<ulong, int> TossedItemDamageByCharacter;

        // Metrics for player spawns
        public int PlayerSpawnCount;

        // Metrics for player life state changes (e.g., fainted, dead)
        public Dictionary<ulong, int> PlayerFaintedCount;

        // Metrics for actions performed by players
        public Dictionary<ulong, Dictionary<string, int>> ActionsPerformedByCharacter;

        // Metrics for charged actions
        public Dictionary<ulong, int> FullyChargedActionsByCharacter;
        public Dictionary<ulong, int> PartiallyChargedActionsByCharacter;

        // Metrics for AoE hits
        public Dictionary<ulong, int> AoeHitsByCharacter;

        // Metrics for projectile accuracy
        public Dictionary<ulong, int> ProjectilesLaunchedByCharacter;
        public Dictionary<ulong, int> ProjectilesHitByCharacter;

        // Metrics for shield actions
        public Dictionary<ulong, int> ShieldActionsUsedByCharacter;
        public Dictionary<ulong, int> FullyChargedShieldsByCharacter;

        // Metrics for dash attacks
        public Dictionary<ulong, int> DashAttacksPerformedByCharacter;
        public Dictionary<ulong, int> DashAttackEnemiesHitByCharacter;

        // Metrics for melee actions
        public Dictionary<ulong, int> MeleeAttacksPerformedByCharacter;
        public Dictionary<ulong, int> MeleeHitsByCharacter;

        // Metrics for raybeam attacks
        public Dictionary<ulong, int> RaybeamActionsPerformedByCharacter;
        public Dictionary<ulong, int> RaybeamHitsByCharacter;

        // Metrics for target acquisition and engagement
        public Dictionary<ulong, int> TargetAcquisitionCount;
        public Dictionary<ulong, float> TargetEngagementDuration;

        // Metrics for Trample Actions
        public Dictionary<ulong, int> TrampleEnemiesHitCount;
        public Dictionary<ulong, int> TrampleDamageDealt;
        public Dictionary<ulong, int> TrampleStunOccurrences;

        // Metrics for character classes
        public Dictionary<string, int> DamageTakenByCharacterClass;
        public Dictionary<string, int> DamageDealtByCharacterClass;
        public Dictionary<string, int> AbilityUsesByCharacterClass;

        // Metrics for action types
        public Dictionary<string, int> ActionCountByType;

        // Metrics for player session data
        public Dictionary<ulong, float> PlayerSessionTime;

        // Health changes
        public Dictionary<ulong, int> HealingReceivedByCharacter;
        public Dictionary<ulong, int> ModifiedDamageReceivedByCharacter;

        // Buff modifications
        public Dictionary<ulong, Dictionary<string, float>> BuffValuesByCharacter;

        // Action-related tracking
        public Dictionary<ulong, int> ActionStartsByCharacter;
        public Dictionary<ulong, int> ActionInterruptsByCharacter;
        public Dictionary<ulong, int> ActionStopsByCharacter;

        // Movement status changes
        public Dictionary<ulong, int> MovementStatusChangesByCharacter;

        // NPC-specific AI decisions
        public Dictionary<ulong, int> AiDecisionsByNpc;

        // Action cancellations
        public Dictionary<ulong, int> ActionCancellations;

        // Buff usage
        public Dictionary<ulong, Dictionary<string, int>> BuffUsageByCharacter;

        // Action queue depth
        public Dictionary<ulong, int> ActionQueueDepthByCharacter;
    }
}
