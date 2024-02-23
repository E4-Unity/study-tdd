using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace LOL
{
    public interface ICharacter
    {
        public float WalkSpeed { get; }
        public void MoveTo(Vector3 destination);
    }

    /// <summary>
    /// 캐릭터 애니메이션 제어 전용 클래스
    /// </summary>
    [Serializable]
    public class CharacterAnimator
    {
        /* 캐시 */
        static readonly int s_HashWalkSpeed = Animator.StringToHash("WalkSpeed");

        /* 컴포넌트 */
        MonoBehaviour m_Owner;
        Animator m_Animator;

        public MonoBehaviour Owner => m_Owner;
        public Animator GetAnimator() => m_Animator;

        /* 필드 */
        ICharacter m_Character;
        bool m_Initialized;

        /* 의존성 주입 */
        public void Init(MonoBehaviour owner, Animator animator)
        {
            // 중복 호출 방지
            if (m_Initialized) return;
            m_Initialized = true;

            // 의존성 주입
            m_Owner = owner;
            m_Character = owner as ICharacter ?? throw new NullReferenceException(nameof(m_Character));
            m_Animator = animator;

            // 코루틴 시작
            owner.StartCoroutine(UpdateAnimatorParameters());
        }

        /* 메서드 */
        IEnumerator UpdateAnimatorParameters()
        {
            while (true)
            {
                yield return null;
                m_Animator.SetFloat(s_HashWalkSpeed, m_Character.WalkSpeed);
            }
        }
    }

    // TODO AttributeSet
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Character : MonoBehaviour, ICharacterSocket, ICharacter
    {
        /* 설정 */
        [SerializeField] Transform m_FrontSocket;

        /* 컴포넌트 */
        [SerializeField] CharacterAnimator m_CharacterAnimator = new CharacterAnimator();
        [SerializeField] AbilityManager m_AbilityManager = new AbilityManager();
        Rigidbody m_RigidBody;
        Collider m_Collider;
        NavMeshAgent m_NavMeshAgent;

        /* 읽기 전용 컴포넌트 */
        public AbilityManager GetAbilityManager() => m_AbilityManager;
        public Rigidbody GetRigidBody() => m_RigidBody;
        public Collider GetCollider() => m_Collider;
        public Animator GetAnimator() => m_CharacterAnimator.GetAnimator();
        public NavMeshAgent GetNavMeshAgent() => m_NavMeshAgent;

        /* API */
        public void SetPhysics(bool activate)
        {
            // TODO 공격은 받을 수 있는 상태여야 합니다.
            GetCollider().enabled = activate;
            GetRigidBody().useGravity = activate;
        }

        /* MonoBehaviour */
        protected virtual void Awake()
        {
            // 컴포넌트 할당
            m_RigidBody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            var animator = GetComponentInChildren<Animator>();
            m_NavMeshAgent = GetComponent<NavMeshAgent>();

            // CharacterAnimator 초기화
            m_CharacterAnimator.Init(this, animator);

            // AbilityManager 초기화
            m_AbilityManager.Init(gameObject, animator);
        }

        /* ICharacterSocket 인터페이스 */
        public Transform Front => m_FrontSocket;

        /* ICharacter 인터페이스 */
        public float WalkSpeed => m_NavMeshAgent.velocity.magnitude;
        public void MoveTo(Vector3 destination) => m_NavMeshAgent.SetDestination(destination);
    }
}
