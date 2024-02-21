using UnityEngine;

namespace LOL
{
    // TODO AttributeSet
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Character : MonoBehaviour, ICharacterSocket
    {
        /* 설정 */
        [SerializeField] Transform m_FrontSocket;

        /* 컴포넌트 */
        Rigidbody m_RigidBody;
        Collider m_Collider;
        Animator m_Animator;
        [SerializeField] AbilityManager m_AbilityManager = new AbilityManager();

        /* 읽기 전용 컴포넌트 */
        public Rigidbody GetRigidBody() => m_RigidBody;
        public Collider GetCollider() => m_Collider;
        protected Animator GetAnimator() => m_Animator;
        public AbilityManager GetAbilityManager() => m_AbilityManager;

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
            m_Animator = GetComponentInChildren<Animator>();

            // AbilityManager 초기화
            m_AbilityManager.Init(gameObject, m_Animator);
        }

        public Transform Front => m_FrontSocket;
    }
}
