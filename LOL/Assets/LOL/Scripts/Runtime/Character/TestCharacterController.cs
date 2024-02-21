using UnityEngine;

namespace LOL
{
    public class TestCharacterController : MonoBehaviour
    {
        Character m_Character;

        bool m_Activated;

        void Awake()
        {
            m_Character = GetComponent<Character>();
        }

        void Start()
        {
            Activate();
            Invoke(nameof(Activate), 2f);
        }

        void Activate()
        {
            m_Character.GetAbilityManager().TryActivateAbility(0);
        }
    }
}
