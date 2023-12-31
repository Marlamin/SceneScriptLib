using System.Numerics;

namespace SceneScriptLib
{
    public struct TimelineScene
    {
        public Dictionary<string, Actor> actors;
    }

    public struct Actor
    {
        public ActorProperties properties;
    }
    public struct ActorProperties
    {
        public AppearanceProperty? Appearance;
        public CustomScriptProperty? CustomScript;
        public EquipWeaponProperty? EquipWeapon;
        public FadeProperty? Fade;
        public FadeRegionProperty? FadeRegion;
        public GroundSnapProperty? GroundSnap;
        public MusicProperty? Music;
        public MoveSplineProperty? MoveSpline;
        public ScaleProperty? Scale;
        public SheatheProperty? Sheathe;
        public TransformProperty? Transform;
    }

    public struct GroundSnapProperty
    {
        public Dictionary<float, GroundSnapEvent> events;
    }
    public struct GroundSnapEvent
    {
        public bool snap;
    }

    public struct TransformProperty
    {
        public Dictionary<float, TransformEvent> events;
    }

    public struct TransformEvent
    {
        public Vector3 Position;
        public float Yaw;
        public float Pitch;
        public float Roll;
    }

    public struct MoveSplineProperty
    {
        public float overrideSpeed;
        public bool useModelRunSpeed;
        public bool useModelWalkSpeed;
        public bool yawUsesSplineTangent;
        public bool yawUsesNodeTransform;
        public bool yawBlendDisabled;
        public bool pitchUsesSplineTangent;
        public bool pitchUsesNodeTransform;
        public bool rollUsesNodeTransform;
        public Dictionary<float, TransformEvent> events;
    }

    public struct CustomScriptProperty
    {
        public Dictionary<float, CustomScriptEvent> events;
    }

    public struct CustomScriptEvent
    {
        public string script;
    }

    public struct FadeRegionProperty
    {
        public Dictionary<float, FadeRegionEvent> events;
    }

    public struct FadeRegionEvent
    {
        public bool enabled;
        public float radius;
        public bool includePlayer;
        public bool excludePlayers;
        public bool excludeNonPlayers;
        public bool includeSounds;
        public bool includeWMOs;
    }

    public struct MusicProperty
    {
        public Dictionary<float, MusicEvent> events;
    }

    public struct MusicEvent
    {
        public int soundKitID;
    }

    public struct AppearanceProperty
    {
        public Dictionary<float, AppearanceEvent> events;
    }

    public struct AppearanceEvent
    {
        public CreatureID creatureID;
        public int creatureDisplaySetIndex;
        public int creatureDisplayInfoID;
        public FileDataID fileDataID;
        public GameObjectDisplayInfoID wmoGameObjectDisplayID;
        public ItemID itemID;
        public bool isPlayerClone;
        public bool isPlayerCloneNative;
        public bool playerSummon;
        public int playerGroupIndex;
        public bool smoothPhase;
    }
    public struct ScaleProperty
    {
        public Dictionary<float, ScaleEvent> events;
    }
    public struct ScaleEvent
    {
        public float scale;
        public float duration;
    }
    public struct FadeProperty
    {
        public Dictionary<float, FadeEvent> events;
    }
    public struct FadeEvent
    {
        public float alpha;
        public float time;
    }

    public struct SheatheProperty
    {
        public Dictionary<float, SheatheEvent> events;
    }
    public struct SheatheEvent
    {
        public bool isSheathed;
        public bool isRanged;
        public bool animated;
    }

    public struct EquipWeaponProperty
    {
        public Dictionary<float, EquipWeaponEvent> events;
    }
    public struct EquipWeaponEvent
    {
        public int itemID;
        public bool MainHand;
        public bool OffHand;
        public bool Ranged;
    }

    public struct CreatureID
    {
        public int ID;
    }
    public struct FileDataID
    {
        public int ID;
    }
    public struct GameObjectDisplayInfoID
    {
        public int ID;
    }
    public struct ItemID
    {
        public int ID;
    }
}
