using ME.ECS.Mathematics;
using UnityEngine;

namespace SG.Core
{
    public static class Constants
    {
        public const string MAINMENU_SCENE_NAME = "Main";
        public const string GAMEPLAY_SCENE_NAME = "Game Empty";
        public const string GAMEPLAY_MASTER_SCENE_NAME = "Game Master";
        public const string SHOOTER_SCENE_NAME = "Shooter";
        public const string CONSTRUCTOR_SCENE_NAME = "Competition Constructor";
        public const string INIT_STATES_FOLDER = "Assets/Resources/InitStates";
        public const string INIT_STATES_EXTERNAL_PATH = @"Saves/Zone.save";

#if UNITY_EDITOR && SAVE_STATE_FEATURE
        public const string INIT_STATES_RESOURCES_FOLDER = "InitStates";
#endif

        public static class Pathfinding
        {
            public const int MAX_BORDER_DISTANCE = 3;
        }

        public static class SpacePathfinding
        {
            public static readonly sfloat UNTOWING_RANGE_MULTIPLIER = 10f;
            public static readonly sfloat SHIP_COLLISION_RADIUS_DELTA = 10f;
            public static readonly sfloat BACKWARD_ENGINE_DISTANCE = 100f;
            public static readonly sfloat SHIP_COLLISION_MASS_DELTA_PERCENT = .33f;
            public static readonly sfloat DAMPING = .1f;
        }

        public static class Main
        {
            public const int MAX_GRAPHS_PER_SHIP = 256;

            public const float HALF_GRID_SIZE = 0.5f;
            public const int LEVEL_CREW_HEIGHT = 3;

            public const int SYNC_LOCK_MIN_TICKS = 3;
        }

        public static class Doors
        {
            public const float DISTANCE_TO_OPEN = 3.2f;
        }

        public static class RVO
        {
            public const float CHECK_DISTANCE = 20f;
            public const float CHECK_DISTANCE_REPATH = 5f;
            public const int FORWARD_NODE_COUNT = 1;
            public const uint CHARACTER_NODE_COST = 1_000u * (int) (RVO.CHECK_DISTANCE - 1f);
        }

        public static class Serialization
        {
            public const int STATE_SIZE = 1024 * 1024; // 1MB
            public const int EVENT_SIZE = 100; // 100 bytes
        }

        public static class ECSComponentsGroup
        {
            public const uint TRANSFORM = 1 << 0;
            public const uint PROPERTIES = 1 << 1;
            public const uint TASKS = 1 << 2;
            public const uint CHARACTER = 1 << 3;
        }

        public static class StaticSlotParams
        {
            public static readonly float3 FullHatOffset = new float3(0, 0, 0);
            public static readonly float3 FullHatRotation = new float3(0, 5, 0);
        }
    }
}
