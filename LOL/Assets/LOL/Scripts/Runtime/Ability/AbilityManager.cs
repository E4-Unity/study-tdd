using System;
using UnityEngine;

namespace LOL
{
    [Serializable]
    public class AbilityManager : AbilityManagerBase<AttributeType>
    {

        /* AbilityManagerBase */
        public override float AbilityHaste => 0f;
        public override float GetAttributeValue(AttributeType attributeType) => 0f;

        public override void SetAttributeValue(AttributeType attributeType, float value)
        {
            Debug.Log("AttributeType: " + attributeType + ", Value: " + value);
        }
    }
}
