using UnityEngine;

namespace LOL
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Character : MonoBehaviour
    {
        /* 컴포넌트 */
        Rigidbody m_RigidBody;
        protected Rigidbody GetRigidBody() => m_RigidBody;

        Collider m_Collider;
        protected Collider GetCollider() => m_Collider;

        Transform m_Transform;
        protected Transform GetTransform() => m_Transform;

        /* MonoBehaviour */
        protected virtual void Awake()
        {
            // 컴포넌트 할당
            m_RigidBody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            m_Transform = GetComponent<Transform>();
        }
    }
}
