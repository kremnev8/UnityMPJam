using Mirror;

namespace Gameplay.Controllers
{
    public struct  SceneChangeFinished : NetworkMessage
    {
        
    }

    public enum SceneChangeType : byte
    {
        START_GAME,
        RESTART_LEVEL,
        NEXT_LEVEL
    }
    
    public struct ClientChangeSceneRequest : NetworkMessage
    {
        public SceneChangeType type;

        public ClientChangeSceneRequest(SceneChangeType type)
        {
            this.type = type;
        }
    }

    public struct HideFade : NetworkMessage
    {
        
    }
}