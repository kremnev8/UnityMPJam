using Gameplay.Util;

namespace Gameplay.World
{
    public interface IMoveAble
    {
        void Move(Direction direction, bool isDashing);
    }
}