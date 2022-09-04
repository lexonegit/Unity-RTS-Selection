using System;
using UnityEngine;

namespace RTS.Core
{
    [Serializable]
    public class GameSettings : ISerializationCallbackReceiver
    {
        [field:SerializeField] public LayerMask UnitLayer { get; private set; } = default;
        [field:SerializeField] public LayerMask GroundLayer { get; private set; } = default;

        private static LayerMask _unitLayer = default;
        private static LayerMask _groundLayer = default;

        public void OnBeforeSerialize()
        {
            UnitLayer = _unitLayer;
            GroundLayer = _groundLayer;
        }

        public void OnAfterDeserialize()
        {
            _unitLayer = UnitLayer;
            _groundLayer = GroundLayer;
        }
    }
}