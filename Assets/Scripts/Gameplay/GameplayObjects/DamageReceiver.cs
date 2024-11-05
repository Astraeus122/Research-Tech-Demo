using System;
using Unity.BossRoom.Gameplay.AI;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.GameplayObjects
{
    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        public event Action<ServerCharacter, int> DamageReceived;
        public event Action<Collision> CollisionEntered;
        public static event Action<ServerCharacter, int, ulong> OnMetricHealingReceived;


        [SerializeField]
        NetworkLifeState m_NetworkLifeState;

        private float damageMultiplier = 1f;
        private CharacterTypeEnum characterType;

        public static event Action<ServerCharacter, int, ulong> OnMetricDamageReceived;

        public void Initialize(CharacterTypeEnum type)
        {
            characterType = type;
        }

        public void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            if (!IsDamageable()) return; // Early return if not damageable

            // Log the damage or healing event for metrics collection
            OnMetricDamageReceived?.Invoke(inflicter, HP, NetworkObjectId);

            // Adjust HP based on whether it’s damage or healing
            if (HP < 0) // Handling damage
            {
                if (IsMonster())
                {
                    HP = Mathf.RoundToInt(HP * damageMultiplier); // Apply damage multiplier only for monsters
                }
                if (RuleBasedAIManager.Instance != null)
                {
                    OnMetricDamageReceived?.Invoke(inflicter, HP, NetworkObjectId); // Log final adjusted damage
                }
                else
                {
                    Debug.LogWarning("RuleBasedAIManager.Instance is null. Damage not applied.");
                    return;
                }
            }
            else if (HP > 0) // Handling healing
            {
                if (RuleBasedAIManager.Instance != null)
                {
                    HP = Mathf.RoundToInt(HP * RuleBasedAIManager.Instance.healingMultiplier); // Adjust healing
                    OnMetricHealingReceived?.Invoke(inflicter, HP, NetworkObjectId); // Log final adjusted healing
                }
                else
                {
                    Debug.LogWarning("RuleBasedAIManager.Instance is null. Healing not applied.");
                    return;
                }
            }

            // Invoke the damage or healing event once
            DamageReceived?.Invoke(inflicter, HP);
        }

        public void SetDamageMultiplier(float multiplier)
        {
            damageMultiplier = multiplier;
        }

        public bool IsMonster()
        {
            return characterType == CharacterTypeEnum.Imp || characterType == CharacterTypeEnum.ImpBoss || characterType == CharacterTypeEnum.VandalImp;
        }

        public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
        {
            return IDamageable.SpecialDamageFlags.None;
        }

        public bool IsDamageable()
        {
            return m_NetworkLifeState.LifeState.Value == LifeState.Alive;
        }

        void OnCollisionEnter(Collision other)
        {
            CollisionEntered?.Invoke(other);
        }

        internal float GetDamageMultiplier()
        {
            return damageMultiplier;
        }
    }
}
