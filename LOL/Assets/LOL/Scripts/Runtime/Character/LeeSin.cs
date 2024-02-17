using LOL.AssetCollections;
using UnityEngine;

namespace LOL
{
    public class LeeSin : Character
    {
        /* 컴포넌트 */
        [SerializeField] Transform m_SonicWaveSpawnPosition;
        LeeSinAnimator m_CharacterAnimator;

        /* 필드 */
        [SerializeField] Projectile m_SonicWave;

        /* MonoBehaviour */
        protected override void Awake()
        {
            base.Awake();

            // LeeSinAnimator 초기화
            m_CharacterAnimator = GetComponentInChildren<LeeSinAnimator>();
            m_CharacterAnimator.OnSpawnEnergyBall += OnSpawnEnergyBall_Event;
        }

        /* 이벤트 함수 */
        void OnSpawnEnergyBall_Event()
        {
            // TODO 오브젝트 풀링
            Instantiate(m_SonicWave, m_SonicWaveSpawnPosition.position, Quaternion.LookRotation(GetTransform().forward));
        }
    }
}
