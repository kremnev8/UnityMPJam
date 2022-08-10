using Gameplay.Util;

namespace Gameplay.World
{
    public interface IMoveAble
    {
        bool isMagnetic { get; }
        void MoveClient(Direction direction);
        void MoveServer(Direction direction);
    }
}