using System;
using System.Collections.Generic;
using UnityEngine;

namespace LOL
{
    [Serializable]
    public class AbilityCost<TEnum> where TEnum : Enum
    {
        /* 필드 */
        [SerializeField] TEnum m_AttributeType;
        [SerializeField] float m_Value;

        /* 읽기 전용 프로퍼티 */
        public TEnum AttributeType => m_AttributeType;
        public float Value => m_Value;
    }

    /// <summary>
    /// Ability Cost 관련 기능이 추가된 Ability 기본 제네릭 클래스
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    [Serializable]
    public abstract class AbilityBase<TEnum> : AbilityBase where TEnum : Enum
    {
        /* 직렬화 필드 */
        [SerializeField] AbilityCost<TEnum>[] m_Costs;

        /* 읽기 전용 프로퍼티 */
        public AbilityCost<TEnum>[] Costs => m_Costs;

        /* API */
        public void PayCost(AbilityManagerBase<TEnum> owner)
        {
            // Cost 지불
            foreach (var cost in m_Costs)
            {
                var attributeType = cost.AttributeType;
                var attributeValue = owner.GetAttributeValue(attributeType);
                owner.SetAttributeValue(attributeType, attributeValue - cost.Value);
            }
        }

        /* AbilityBase 인터페이스 */
        protected override void OnActivate(AbilityManagerBase owner) => PayCost(owner as AbilityManagerBase<TEnum>);

        public override bool CanActivate(AbilityManagerBase owner) => CanPayCost(owner as AbilityManagerBase<TEnum>) && base.CanActivate(owner);

        /* 메서드 */
        /// <returns>Ability Cost 지불이 가능한 상태인지 확인</returns>
        bool CanPayCost(AbilityManagerBase<TEnum> owner)
        {
            // Cost 지불 능력 확인
            foreach (var cost in m_Costs)
            {
                if (owner.GetAttributeValue(cost.AttributeType) < cost.Value) return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 쿨타임 기능이 구현된 기본 Ability 클래스
    /// </summary>
    [Serializable]
    public abstract class AbilityBase : ScriptableObject
    {
        /* 직렬화 필드 */
        [Min(0)] [SerializeField] int m_ID;
        [Min(0)] [SerializeField] float m_CoolTime;

        /* 필드 */
        // 쿨타임 계산을 위해 마지막으로 Ability 를 발동한 시간을 기록하는 곳
        readonly Dictionary<AbilityManagerBase, float> m_CoolTimeBlackBoard =
            new Dictionary<AbilityManagerBase, float>();

        /* 읽기 전용 프로퍼티 */
        public int ID => m_ID;
        public float CoolTime => m_CoolTime;

        /* API */
        /// <summary>
        /// Ability 실행
        /// </summary>
        /// <param name="owner"></param>
        public void Activate(AbilityManagerBase owner)
        {
            // AbilityManagerBase 최초 등록 시
            if (!m_CoolTimeBlackBoard.ContainsKey(owner))
            {
                // 애니메이션 노티파이 이벤트 바인딩
                owner.AbilityAnimator.OnAnimNotified += OnAnimNotified_Event;

                // AbilityManagerBase 등록
                m_CoolTimeBlackBoard.Add(owner, Time.time);
            }

            // 자손 클래스에서 정의된 Ability 동작 실행
            OnActivate(owner);
        }

        /// <summary>
        /// Ability 동작 정의
        /// </summary>
        /// <param name="owner"></param>
        protected abstract void OnActivate(AbilityManagerBase owner);

        /// <returns>Ability 발동 가능 여부</returns>
        public virtual bool CanActivate(AbilityManagerBase owner) => IsCooldownFinished(owner);

        /// <summary>
        /// 쿨타임을 초기화합니다. Owner 파괴 등의 이유로 캐시 정리를 위해 사용할 수도 있습니다.
        /// </summary>
        public void ResetCoolTime(AbilityManagerBase owner) => m_CoolTimeBlackBoard.Remove(owner);

        /// <summary>
        /// AbilityManagerBase 가 파괴될 때 호출되는 메서드로 관련된 모든 캐시를 정리합니다.
        /// </summary>
        /// <param name="owner"></param>
        public virtual void Flush(AbilityManagerBase owner) => ResetCoolTime(owner);

        /* 메서드 */
        /// <returns>쿨타임이 종료되었는지 여부</returns>
        bool IsCooldownFinished(AbilityManagerBase owner)
        {
            // 아직 한 번도 Ability 를 사용하지 않은 경우
            if (!m_CoolTimeBlackBoard.TryGetValue(owner, out var lastActivatedTime)) return true;

            // 경과 시간 > 쿨타임 (스킬 가속이 100 이면 쿨타임이 50 % 감소)
            return Time.time - lastActivatedTime > CoolTime * (1 + owner.AbilityHaste / 100f);
        }

        /// <summary>
        /// 애니메이션 이벤트 콜백 메서드
        /// </summary>
        /// <param name="abilityAnimator">이벤트 호출자</param>
        /// <param name="methodName">콜백 메서드 이름</param>
        protected virtual void OnAnimNotified_Event(AbilityAnimator abilityAnimator, string methodName){}
    }
}
