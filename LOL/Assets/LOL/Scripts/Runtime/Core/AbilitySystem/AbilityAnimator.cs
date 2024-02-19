using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace LOL
{
    /// <summary>
    /// Ability 에 설정된 애니메이션을 재생하기 위한 컴포넌트입니다.
    /// 애니메이션 이벤트를 사용하기 때문에 Animator 와 동일한 오브젝트에 부착되어야 합니다.
    /// </summary>
    public class AbilityAnimator : AbilityAnimatorBase
    {
        /* 이벤트 */
        /// <summary>
        /// 다른 오브젝트에서 애니메이션 이벤트를 감지하기 위한 이벤트입니다.
        /// </summary>
        public event Action<AbilityAnimator, string> OnAnimNotified;

        /* API */
        /// <summary>
        /// Ability 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="abilityAnimation">Ability 애니메이션</param>
        /// <param name="speed">Ability 애니메이션 속도</param>
        public void PlayAbilityAnimation(AnimationClip abilityAnimation, float speed = 1f)
        {
            /* Ability 애니메이션 설정 */
            // 오버라이드된 애니메이션 목록 불러오기
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            DummyAnimatorOverrideController.GetOverrides(overrides);

            // 중복 할당 방지
            if (overrides[AbilityAnimationIndex].Value != abilityAnimation)
            {
                overrides[AbilityAnimationIndex] =
                    new KeyValuePair<AnimationClip, AnimationClip>(overrides[AbilityAnimationIndex].Key, abilityAnimation);
                DummyAnimatorOverrideController.ApplyOverrides(overrides);
            }

            /* Ability 애니메이션 재생 */
            GetAnimator().SetFloat(HashAbilitySpeed, speed);
            GetAnimator().SetTrigger(HashAbility);
        }

        /* 메서드 */
        /// <summary>
        /// Ability 애니메이션 이벤트에서 OnAnimNotified 이벤트를 호출하기 위한 프록시 메서드입니다.
        /// Ability 애니메이션 이벤트 설정 시 Function 이름은 AnimNotify 를 사용하고, String 매개변수에
        /// 다른 오브젝트에 부착된 스크립트에서 호출하고 싶은 메서드 이름을 입력하면 됩니다.
        /// </summary>
        /// <param name="methodName"></param>
        void AnimNotify(string methodName) => OnAnimNotified?.Invoke(this, methodName);
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class AbilityAnimatorBase : MonoBehaviour
    {
        /* 정적 필드 */
        // 캐시
        protected static int HashAbility { get; } = Animator.StringToHash("Ability");
        protected static int HashAbilitySpeed { get; } = Animator.StringToHash("AbilitySpeed");

        /* 컴포넌트 */
        Animator m_Animator;
        AbilityManagerBase m_Owner;

        /* 설정 */
        [SerializeField] AnimatorOverrideController m_Dummy;
        AnimatorOverrideController m_DummyInstance;

        /* 필드 */
        bool m_Initialized;
        int m_AbilityAnimationIndex = -1;
        float m_AnimatorSpeed;
        bool m_Paused;

        /* 읽기 전용 프로퍼티 */
        public Animator GetAnimator() => m_Animator;
        public AbilityManagerBase Owner => m_Owner;
        protected AnimatorOverrideController DummyAnimatorOverrideController => m_DummyInstance;
        protected int AbilityAnimationIndex => m_AbilityAnimationIndex;

        /* API */
        public void Init(AbilityManagerBase owner, Animator animator)
        {
            // 중복 호출 방지
            if (m_Initialized) return;
            m_Initialized = true;

            // 컴포넌트 할당
            m_Owner = owner;
            m_Animator = animator;

            // 더미 애니메이터 오버라이드 컨트롤러 사본 생성
            if (!m_Dummy) throw new ArgumentNullException(nameof(m_Dummy));
            m_DummyInstance = new AnimatorOverrideController(m_Dummy);
            m_DummyInstance.name = m_Dummy.name + " (Instance)";

            // Animator 초기화
            m_Animator.runtimeAnimatorController = m_DummyInstance;

            // AbilityAnimation 인덱스 확인
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            m_Dummy.GetOverrides(overrides);

            m_AbilityAnimationIndex = -1;
            for (int i = 0; i < overrides.Count; i++)
            {
                if(overrides[i].Value is null) continue;

                m_AbilityAnimationIndex = i;
                break;
            }

            Assert.IsFalse(m_AbilityAnimationIndex < 0,
                "더미 애니메이터 오버라이드 컨트롤러의 Ability 애니메이션 슬롯에 더미 애니메이션 클립이 설정되어야 합니다.");
        }

        public void PauseAnimation()
        {
            // 중복 호출 방지
            if (m_Paused) return;
            m_Paused = true;

            // 기존 속도 저장 후 정지
            m_AnimatorSpeed = m_Animator.speed;
            m_Animator.speed = 0f;
        }

        public void ResumeAnimation()
        {
            // 정지된 상태에서만 호출 가능
            if (!m_Paused) return;
            m_Paused = false;

            // 기존 속도 복구
            m_Animator.speed = m_AnimatorSpeed;
        }
    }
}
