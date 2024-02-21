using System;
using UnityEngine;

namespace LOL
{
    /// <summary>
    /// 발사체 움직임과 충돌 이벤트를 처리하는 클래스입니다.
    /// 충돌 이벤트는 Trigger 방식을 사용하며, 이동은 FixedUpdate 에서 이루어지며 Translate 메서드를 사용합니다.
    /// </summary>
    public class ProjectileBase : MonoBehaviour
    {
        /* 컴포넌트 */
        Collider m_Collider;

        /* 필드 */
        [SerializeField] float m_Speed = 15f; // 발사 속도
        [SerializeField] float m_MaxDistance = 8f; // 사정 거리
        [SerializeField] GameObject[] m_HitEffects;

        float m_Distance;
        bool m_Hit;

        /* 이벤트 */
        // this, other
        public event Action<ProjectileBase, Collider> OnHit;

        /* MonoBehaviour */
        protected virtual void Awake()
        {
            // 컴포넌트 할당
            m_Collider = GetComponent<Collider>();
            m_Collider.isTrigger = true;
        }

        protected virtual void OnEnable()
        {
            // 초기화
            m_Distance = 0f;
            m_Hit = false;
        }

        protected virtual void OnDisable()
        {
            // 충돌 실패 이벤트 호출
            if (!m_Hit) OnHit?.Invoke(this, null);

            // 모든 이벤트 언바인딩
            OnHit = null;
        }

        protected virtual void FixedUpdate()
        {
            // 이동
            float distance = m_Speed * Time.fixedDeltaTime;
            Move(transform.forward, distance);

            // 사정 거리를 벗어나면 비활성화
            m_Distance += distance;
            if (m_Distance > m_MaxDistance) Deactivate();
        }

        void OnTriggerEnter(Collider other)
        {
            // 피격 판정이 가능한 대상인지 검사
            if (!CanHit(other)) return;

            // 피격 효과
            foreach (var hitEffect in m_HitEffects)
            {
                // 피격 정보 분석
                Vector3 position = transform.position;
                Vector3 hitPosition = other.ClosestPoint(position);
                Vector3 hitDirection = position - hitPosition;

                // 피격 효과 생성 및 부착
                SpawnHitEffect(hitEffect, hitPosition, Quaternion.LookRotation(hitDirection));

                // 이펙트 제거 혹은 소멸은 이펙트 클래스에서 처리
            }

            // 기타 충돌 이벤트 처리
            OnHitEnter(other);

            // 이벤트 호출
            OnHit?.Invoke(this, other);

            // 비활성화
            Deactivate();
        }

        /* 메서드 */
        // 이동
        protected virtual void Move(Vector3 direction, float distance) => transform.Translate(direction * distance, Space.World);

        // 피격 판정이 가능한 대상인지 검사
        protected virtual bool CanHit(Collider other) => true;

        // 기타 충돌 이벤트 처리
        protected virtual void OnHitEnter(Collider other)
        {

        }

        // 자손 클래스에서 오브젝트 풀링 구현
        protected virtual void SpawnHitEffect(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject hitEffectInstance = Instantiate(prefab, position, rotation);
            if(parent is not null) hitEffectInstance.transform.parent = parent;
        }

        // 자손 클래스에서 오브젝트 풀링 구현
        protected virtual void Deactivate() => Destroy(gameObject);
    }
}
