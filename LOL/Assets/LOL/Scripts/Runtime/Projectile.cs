using UnityEngine;

namespace LOL
{
    public class Projectile : ProjectileBase
    {
        /* ProjectileBase */
        // Character 만 피격 가능
        protected override bool CanHit(Collider other) => other.GetComponent<Character>();
        protected override void Deactivate() => gameObject.SetActive(false);
    }
}
