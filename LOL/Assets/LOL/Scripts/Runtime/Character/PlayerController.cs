using UnityEngine;

namespace LOL
{
    /// <summary>
    /// 플레이어 입력을 처리합니다.
    /// 플레이어 컨트롤러는 하나만 존재해야 합니다.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /* 컴포넌트 */
        Character m_Character;
        Camera m_MainCamera;

        /* 프로퍼티 */
        public Vector3 MousePosition
        {
            get
            {
                var ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

                return !Physics.Raycast(ray, out var hit)
                    ? transform.forward
                    : new Vector3(hit.point.x, transform.position.y, hit.point.z);
            }
        }

        /* MonoBehaviour 인터페이스 */
        void Awake()
        {
            // 컴포넌트 할당
            m_Character = GetComponent<Character>();
            m_MainCamera = Camera.main;
        }

        void Update()
        {
            // 좌 클릭 시 이동
            if (Input.GetMouseButtonDown(1))
            {
                m_Character.MoveTo(MousePosition);
            }
        }
    }
}
