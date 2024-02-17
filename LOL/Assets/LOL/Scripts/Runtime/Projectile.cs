using System;
using UnityEngine;

namespace LOL
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        /* 컴포넌트 */
        Collider m_Collider;
        Rigidbody m_RigidBody;
        Transform m_Transform;

        /* 필드 */
        [SerializeField] float m_Speed = 10f; // 발사 속도
        [SerializeField] float m_MaxDistance = 8f; // 사정 거리
        [SerializeField] GameObject m_HitEffect;

        float m_Distance; // 이동한 거리
        bool m_Hit;

        /* 이벤트 */
        public event Action<Projectile, Collider> OnHit; // this, other

        /* MonoBehaviour */
        void Awake()
        {
            // 컴포넌트 할당
            m_RigidBody = GetComponent<Rigidbody>();
            m_Transform = GetComponent<Transform>();

            // RigidBody 초기화
            m_Collider = GetComponent<Collider>();
            m_Collider.isTrigger = true;
        }

        void OnEnable()
        {
            // 초기화
            m_Distance = 0f;
            m_Hit = false;
        }

        void OnDisable()
        {
            // 충돌 실패 이벤트 호출
            if (!m_Hit) OnHit?.Invoke(this, null);
        }

        void FixedUpdate()
        {
            // 이동
            float distance = m_Speed * Time.fixedDeltaTime;
            m_RigidBody.MovePosition(m_Transform.position + m_Transform.forward * distance);

            // 사정 거리를 벗어나면 비활성화
            m_Distance += distance;
            if (m_Distance > m_MaxDistance) Deactivate();
        }

        void OnTriggerEnter(Collider other)
        {
            // 캐릭터인 경우에만 충돌 이벤트 처리
            if (other.GetComponent<Character>() is null) return;

            // 충돌 이벤트 호출
            OnHit?.Invoke(this, other);

            if (m_HitEffect)
            {
                // 피격 효과 스폰
                Vector3 position = transform.position;
                Vector3 hitPosition = other.ClosestPoint(position);
                Vector3 hitDirection = position - hitPosition;
                var effect = Instantiate(m_HitEffect, hitPosition, Quaternion.LookRotation(hitDirection));

                // 피격체에 이펙트 부착
                effect.transform.parent = other.transform;
            }

            // 비활성화
            Deactivate();
        }

        /* 메서드 */
        // TODO 오브젝트 풀링
        protected virtual void Deactivate() => Destroy(gameObject);
    }
}
