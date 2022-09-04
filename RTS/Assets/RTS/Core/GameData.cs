using UnityEngine;

namespace RTS.Core
{
    [CreateAssetMenu]
    public class GameData : ScriptableObject
    {
        public GameSettings Settings = default;
        public GameInput Input = default;

        private static GameData _instance = default;
        public static GameData Instance
        {
            get
            {
                if (_instance == null) 
                    _instance = Resources.Load<GameData>("GameData");
                return _instance;    
            }
        }
    }
}