using UnityEngine;

namespace Mahou.Input
{
    public struct PlayerInput
    {
        public Vector3 cameraForward;
        public Vector3 cameraRight;
        public Vector2 movement;
        public bool lockon;
        public bool jump;
        public bool light_atttack;
        public bool heavy_attack;
        public bool shoot;
        public bool dash;
        public bool parry;
        public bool abilityOne, abilityTwo, abilityThree, abilityFour;
    }
}