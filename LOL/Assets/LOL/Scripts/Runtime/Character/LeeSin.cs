using System.Collections;
using LOL.AssetCollections;
using UnityEngine;

namespace LOL
{
    // TODO 쿨타임이 지나면 공명의 일격 대상이 제거되야 합니다.
    // TODO 스킬을 스크립터블 오브젝트로 분리
    public class LeeSin : Character
    {
        /* 컴포넌트 */
        [SerializeField] Transform m_SonicWaveSpawnPosition;
        LeeSinAnimator m_CharacterAnimator;

        /* 필드 */
        [SerializeField] Projectile m_SonicWave;
        [SerializeField] float m_ResonatingStrikeSpeed = 10f;
        [SerializeField] float m_ResonatingStrikeDistance = 0.3f; // 피격 판정을 위한 거리
        Collider m_Target; // 음파 표식이 새겨진 대상

        // 캐시
        WaitForFixedUpdate m_WaitForFixedUpdate;

        /* 프로퍼티 */
        Collider Target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                if (value is null)
                {
                    // TODO 대상에게 새겨진 표식을 지웁니다.
                }
                else
                {
                    // TODO 대상에게 표식을 새깁니다.
                }
            }
        }

        /* API */
        /// <summary>
        /// 음파를 날립니다.
        /// 공명의 일격 대상이 존재하는 경우에는 음파 대신 공명의 일격을 실행합니다.
        /// </summary>
        [ContextMenu("SonicWave")]
        public void SonicWave()
        {
            if (Target is not null)
            {
                // 공명의 일격 애니메이션 재생
                // 애니메이션 이벤트를 통해 PauseAnimation 이 호출되기 때문에 완료 후 ResumeAnimation 을 따로 호출해주어야 합니다.
                m_CharacterAnimator.PlayAnimation(LeeSinAnimator.AnimationType.ResonatingStrike);

                // 공명의 일격
                ResonatingStrike();
            }
            else
            {
                // 음파 애니메이션 재생
                // 애니메이션 이벤트를 통해 음파 소환이 이루어집니다.
                m_CharacterAnimator.PlayAnimation(LeeSinAnimator.AnimationType.SonicWave);
            }
        }

        /* MonoBehaviour */
        protected override void Awake()
        {
            base.Awake();

            // LeeSinAnimator 초기화
            m_CharacterAnimator = GetComponentInChildren<LeeSinAnimator>();
            m_CharacterAnimator.OnSpawnEnergyBall += OnSpawnEnergyBall_Event;

            // 캐싱
            m_WaitForFixedUpdate = new WaitForFixedUpdate();
        }

        // TODO 테스트 코드 시작
        void Start()
        {
            SonicWave();
        }

        bool m_Wait;

        void Update()
        {
            if (!m_Wait && Target is not null)
            {
                m_Wait = true;
                SonicWave();
            }
        }

        // TODO 테스트 코드 끝

        /* 이벤트 함수 */
        // LeeSinAnimator
        void OnSpawnEnergyBall_Event() => SpawnSonicWave();

        // Projectile
        void OnHit_Event(Projectile projectile, Collider other)
        {
            // 층덜 이벤트 언바인딩
            projectile.OnHit -= OnHit_Event;

            // 충돌 성공 여부 검사
            if (other is null) return;

            // 공명의 일격 대상 기억
            Target = other;

            // TODO 대상에게 표식을 새깁니다. (이펙트, Setter 사용)
        }

        /* 메서드 */
        void SetPhysics(bool activate)
        {
            // TODO 공격은 받을 수 있는 상태여야 합니다.
            GetCollider().enabled = activate;
            GetRigidBody().useGravity = activate;
        }

        void SpawnSonicWave()
        {
            // TODO 오브젝트 풀링
            // SonicWave 스폰
            Projectile sonicWave = Instantiate(m_SonicWave, m_SonicWaveSpawnPosition.position, Quaternion.LookRotation(GetTransform().forward));

            // 충돌 이벤트 바인딩
            sonicWave.OnHit += OnHit_Event;
        }

        void ResonatingStrike()
        {
            // 장애물을 관통할 수 있습니다.
            SetPhysics(false);

            // 대상 추적
            StartCoroutine(TraceTarget());

            // TODO 피격 판정 거리에 도달하거나, 캔슬, 사망 시 공명의 일격 종료
        }

        // TODO 코루틴 대신 비동기로 대체해서 TraceTarget 과 피격 이벤트 분리
        /// <summary>
        /// ResonatingStrike 에서 호출되며 대상을 추적합니다.
        /// </summary>
        /// <returns></returns>
        IEnumerator TraceTarget()
        {
            while (Target is not null)
            {
                yield return m_WaitForFixedUpdate;

                /* 대상 추적 */
                // 계산
                Vector3 position = transform.position;
                Vector3 targetPosition = Target.transform.position;
                Vector3 offset = targetPosition - position;
                Vector3 direction = offset.normalized;
                float distance = offset.magnitude;
                float moveDistance = m_ResonatingStrikeSpeed * Time.fixedDeltaTime;

                // 이동
                GetRigidBody().MovePosition(position + direction * moveDistance);

                // 회전
                GetRigidBody().MoveRotation(Quaternion.LookRotation(direction));

                // 대상과의 거리가 일정 수준에 도달하면 피격 판정 처리
                if (distance < m_ResonatingStrikeDistance)
                {
                    Target = null;

                    // TODO 데미지 및 피격 효과
                    // 애니메이션 재개
                    m_CharacterAnimator.ResumeAnimation();

                    // 장애물을 관통할 수 없습니다.
                    SetPhysics(true);
                }
            }
        }
    }
}
