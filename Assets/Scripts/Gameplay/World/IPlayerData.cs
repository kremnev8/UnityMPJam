using Gameplay.Conrollers;

namespace Gameplay.World
{
    public interface IPlayerData
    {
        string PlayerName { get; set; }
        PlayerRole role { get; set; }

        void StartMap();

    }
}