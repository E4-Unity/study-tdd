using UnityEngine;

namespace LOL
{
    /// <summary>
    /// ProjectileBase 클래스와 달리 RigidBody 를 필수로 가지고 있으며 이동은 MovePosition 메서드를 사용합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidProjectileBase : ProjectileBase
    {
        /* 컴포넌트 */
        Rigidbody m_RigidBody;

        /* MonoBehaviour */
        protected override void Awake()
        {
            base.Awake();

            // 컴포넌트 할당
            m_RigidBody = GetComponent<Rigidbody>();
        }

        /* ProjectileBase */
        protected override void Move(Vector3 direction, float distance) => m_RigidBody.MovePosition(transform.position + direction * distance);
    }
}
