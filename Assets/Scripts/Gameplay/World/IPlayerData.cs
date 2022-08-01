using Gameplay.Conrollers;

namespace Gameplay.World
{
    public interface IPlayerData
    {
        string PlayerName { get; set; }
        Timeline role { get; set; }

        void StartMap();

    }
}