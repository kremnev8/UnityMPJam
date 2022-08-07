using Gameplay.Util;

namespace Gameplay.World
{
    public interface IMoveAble
    {
        bool isMagnetic { get; }
        void Move(Direction direction);
    }
}