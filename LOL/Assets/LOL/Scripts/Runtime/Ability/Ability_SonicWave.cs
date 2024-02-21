using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOL
{
    /// <summary>
    /// 음파와 공명의 일격이 구현된 Ability 입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Ability/SonicWave")]
    [Serializable]
    public class Ability_SonicWave : Ability
    {
        /// <summary>
        /// 음파 기능 구현
        /// </summary>
        [Serializable]
        class SonicWaveCore
        {
            /* 설정 */
            [SerializeField] ProjectileBase m_SonicWave;
            [SerializeField] AnimationClip m_AnimationMontage;
            [SerializeField] float m_AnimationSpeed = 1f;

            /* 필드 */
            readonly Dictionary<AbilityManagerBase, ProjectileBase> m_CachedMap =
                new Dictionary<AbilityManagerBase, ProjectileBase>();

            /* 이벤트 */
            public event Action<AbilityManagerBase, Transform> OnHit;

            /* API */
            public bool CanActivate(AbilityManagerBase owner)
            {
                // ICharacterSocket 인터페이스 확인
                var characterSocket = owner.gameObject.GetComponent<ICharacterSocket>();
                return characterSocket is not null;
            }

            public void Activate(AbilityManagerBase owner)
            {
                owner.AbilityAnimator.PlayAbilityAnimation(m_AnimationMontage, m_AnimationSpeed);
            }

            public ProjectileBase SpawnSonicWave(AbilityManagerBase owner)
            {
                // Spawn Transform 설정
                var characterSocket = owner.gameObject.GetComponent<ICharacterSocket>();
                Vector3 spawnPosition = characterSocket.Front.position;
                Quaternion spawnRotation = Quaternion.LookRotation(owner.transform.forward);

                // 배정된 음파가 없는 경우 신규 생성 및 배정
                var cachedSonicWave = GetOrCreateSonicWave(owner);
                Transform transform = cachedSonicWave.transform;
                transform.position = spawnPosition;
                transform.rotation = spawnRotation;

                // 피격 이벤트 바인딩
                cachedSonicWave.OnHit += (sonicWave, other) =>
                {
                    // 피격 여부 확인
                    if (other is null) return;

                    // 이벤트 호출
                    OnHit?.Invoke(owner, other.transform);

                    // 음파 비활성화
                    sonicWave.gameObject.SetActive(false);
                };

                return cachedSonicWave;
            }

            /* 메서드 */
            ProjectileBase GetOrCreateSonicWave(AbilityManagerBase owner)
            {
                // 캐시된 음파가 없으면 새로 생성
                if (!m_CachedMap.ContainsKey(owner)) m_CachedMap.Add(owner, Instantiate(m_SonicWave));

                // 음파 활성화
                var sonicWave = m_CachedMap[owner];
                sonicWave.gameObject.SetActive(true);

                return sonicWave;
            }
        }

        /// <summary>
        /// 공명의 일격 기능 구현
        /// </summary>
        [Serializable]
        class ResonatingStrikeCore
        {
            /* 설정 */
            [SerializeField] Transform m_SonicMark;
            [SerializeField] float m_ResonatingStrikeSpeed = 10f;
            [SerializeField] float m_ResonatingStrikeDistance = 1.5f; // 피격 판정을 위한 거리

            [SerializeField] AnimationClip m_AnimationMontage;
            [SerializeField] float m_AnimationSpeed = 1f;

            /* 필드 */
            readonly Dictionary<AbilityManagerBase, Transform> m_CachedMap =
                new Dictionary<AbilityManagerBase, Transform>();

            WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();

            /* API */
            public bool CanActivate(AbilityManagerBase owner)
            {
                // 표식 생성 여부 확인
                if (!m_CachedMap.TryGetValue(owner, out var sonicMark)) return false;

                // 표식 활성화 여부 확인
                return sonicMark.gameObject.activeSelf;
            }

            public void Activate(AbilityManagerBase owner)
            {
                owner.AbilityAnimator.PlayAbilityAnimation(m_AnimationMontage, m_AnimationSpeed);
                TraceSonicMark(owner);
            }

            public void SpawnSonicMark(AbilityManagerBase owner, Transform target)
            {
                // 대상에게 표식 새기기
                var sonicMark = GetOrCreateSonicMark(owner);
                sonicMark.transform.parent = target;
                sonicMark.localPosition = Vector3.zero;
                sonicMark.localRotation = Quaternion.identity;
            }

            /* 메서드 */
            void TraceSonicMark(AbilityManagerBase owner)
            {
                Character character = owner.gameObject.GetComponent<Character>();
                character.SetPhysics(false);
                character.StartCoroutine(TraceTarget(owner, character, GetOrCreateSonicMark(owner)));
            }

            // TODO 코루틴 대신 비동기로 대체해서 TraceTarget 과 피격 이벤트 분리
            /// <summary>
            /// ResonatingStrike 에서 호출되며 대상을 추적합니다.
            /// </summary>
            /// <returns></returns>
            IEnumerator TraceTarget(AbilityManagerBase owner, Character character, Transform target)
            {
                while (true)
                {
                    yield return m_WaitForFixedUpdate;

                    /* 대상 추적 */
                    // 계산
                    Vector3 position = character.transform.position;
                    Vector3 targetPosition = target.position;
                    Vector3 offset = targetPosition - position;
                    Vector3 direction = offset.normalized;
                    float distance = offset.magnitude;
                    float moveDistance = m_ResonatingStrikeSpeed * Time.fixedDeltaTime;

                    // 이동
                    character.GetRigidBody().MovePosition(position + direction * moveDistance);

                    // 회전
                    character.GetRigidBody().MoveRotation(Quaternion.LookRotation(direction));

                    // 대상과의 거리가 일정 수준에 도달하면 피격 판정 처리
                    if (distance < m_ResonatingStrikeDistance) break;
                }

                // 애니메이션 재생 재개
                owner.AbilityAnimator.ResumeAnimation();
            }

            Transform GetOrCreateSonicMark(AbilityManagerBase owner)
            {
                if (!m_CachedMap.ContainsKey(owner))
                {
                    var sonicMarkInstance = Instantiate(m_SonicMark);
                    m_CachedMap.Add(owner, sonicMarkInstance);
                }

                var sonicMark = m_CachedMap[owner];
                sonicMark.gameObject.SetActive(true);

                return m_CachedMap[owner];
            }
        }

        /* 컴포넌트 */
        [SerializeField] SonicWaveCore m_SonicWave = new SonicWaveCore();
        [SerializeField] ResonatingStrikeCore m_ResonatingStrike = new ResonatingStrikeCore();

        /* 생성자 */
        public Ability_SonicWave()
        {
            // SonicWaveCore 와 ResonatingStrikeCore 연동
            m_SonicWave.OnHit += m_ResonatingStrike.SpawnSonicMark;
        }

        /* AbilityBase 인터페이스 */
        public override bool CanActivate(AbilityManagerBase owner)
        {
            // 스킬 콤보의 역순으로 검사
            return (m_ResonatingStrike.CanActivate(owner) || m_SonicWave.CanActivate(owner)) && base.CanActivate(owner);
        }

        protected override void OnActivate(AbilityManagerBase owner)
        {
            if (m_ResonatingStrike.CanActivate(owner))
            {
                m_ResonatingStrike.Activate(owner);
            }
            else
            {
                m_SonicWave.Activate(owner);
            }
        }

        protected override void OnAnimNotified_Event(AbilityAnimator abilityAnimator, string methodName)
        {
            base.OnAnimNotified_Event(abilityAnimator, methodName);

            var owner = abilityAnimator.Owner;

            if (methodName == nameof(m_SonicWave.SpawnSonicWave)) m_SonicWave.SpawnSonicWave(owner);
            else if(methodName == nameof(abilityAnimator.PauseAnimation)) abilityAnimator.PauseAnimation();
        }
    }
}
