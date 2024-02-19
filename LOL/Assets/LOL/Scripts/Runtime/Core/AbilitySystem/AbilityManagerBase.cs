using System;
using System.Collections.Generic;
using UnityEngine;

namespace LOL
{
    [Serializable]
    public abstract class AbilityManagerBase<TEnum> : AbilityManagerBase where TEnum : Enum
    {
        /* API */
        public abstract float GetAttributeValue(TEnum attributeType);
        public abstract void SetAttributeValue(TEnum attributeType, float value);
    }

    [Serializable]
    public abstract class AbilityManagerBase
    {
        /* 컴포넌트 */
        GameObject m_Owner;
        Transform m_Transform;
        AbilityAnimator m_AbilityAnimator;

        /* 필드 */
        // 기본 Ability 목록
        [SerializeField] AbilityBase[] m_DefaultAbilities;

        // 사용 가능한 Ability 목록
        Dictionary<int, AbilityBase> m_GrantedAbilities;

        bool m_Initialized;

        /* 읽기 전용 프로퍼티 */
        // 스킬 가속이 100 이면 쿨타임이 50 % 감소
        public abstract float AbilityHaste { get; }
        public GameObject gameObject => m_Owner;
        public Transform transform => m_Transform;
        public AbilityAnimator AbilityAnimator => m_AbilityAnimator;

        /* 파괴자 */
        ~AbilityManagerBase()
        {
            // 유효성 검사
            if (m_GrantedAbilities is null) return;

            // 캐시 정리
            foreach (var ability in m_GrantedAbilities.Values)
            {
                ability.Flush(this);
            }
        }

        /* API */
        /// <summary>
        /// 초기화
        /// </summary>
        public virtual void Init(GameObject owner, Animator animator)
        {
            // 중복 호출 방지
            if (m_Initialized) return;
            m_Initialized = true;

            // 의존성 주입
            m_Owner = owner;
            m_Transform = owner.transform;
            m_AbilityAnimator = animator.GetComponent<AbilityAnimator>() ?? animator.gameObject.AddComponent<AbilityAnimator>();
            m_AbilityAnimator.Init(this, animator);

            // 기본 Ability 등록
            m_GrantedAbilities = new Dictionary<int, AbilityBase>(m_DefaultAbilities.Length);
            foreach (var ability in m_DefaultAbilities)
            {
                GrantAbility(ability);
            }
        }

        /// <summary>
        /// 사용 가능한 Ability 목록에 새로운 Ability 추가
        /// </summary>
        public void GrantAbility(AbilityBase ability) => m_GrantedAbilities.TryAdd(ability.ID, ability);

        /// <summary>
        /// 사용 가능한 Ability 목록에서 Ability 제거
        /// </summary>
        /// <param name="ability"></param>
        public void RemoveAbility(AbilityBase ability)
        {
            // 등록되지 않은 Ability 는 제거할 필요가 없습니다.
            if (!m_GrantedAbilities.TryGetValue(ability.ID, out var grantedAbility)) return;

            // Ability 캐시 정리 및 제거
            grantedAbility.Flush(this);
            m_GrantedAbilities.Remove(ability.ID);
        }

        /// <returns>Ability 발동 가능 여부</returns>
        public bool CanActivateAbility(int abilityID)
        {
            // Granted Abilities 목록에 존재하는지 확인
            if (!m_GrantedAbilities.ContainsKey(abilityID)) return false;

            // Ability 발동 가능 여부 확인
            return m_GrantedAbilities[abilityID].CanActivate(this);
        }

        /// <returns>Ability 발동 성공 여부</returns>
        public bool TryActivateAbility(int abilityID)
        {
            if (!CanActivateAbility(abilityID)) return false;

            // Ability 발동
            var ability = m_GrantedAbilities[abilityID];
            ability.Activate(this);

            return true;
        }
    }
}
