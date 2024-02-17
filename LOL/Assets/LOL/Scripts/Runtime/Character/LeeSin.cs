using LOL.AssetCollections;

namespace LOL
{
    public class LeeSin : Character
    {
        /* 컴포넌트 */
        LeeSinAnimator m_CharacterAnimator;

        /* MonoBehaviour */
        void Awake()
        {
            // LeeSinAnimator 초기화
            m_CharacterAnimator = GetComponentInChildren<LeeSinAnimator>();
            m_CharacterAnimator.OnSpawnEnergyBall += OnSpawnEnergyBall_Event;
        }

        /* 이벤트 함수 */
        void OnSpawnEnergyBall_Event()
        {
            // TODO 음파 발사
        }
    }
}
