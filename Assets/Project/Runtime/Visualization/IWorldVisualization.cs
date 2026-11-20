
using Pokemon.World;
using Pokemon.CharacterSystem;

namespace Pokemon.Visualization
{
    public interface IWorldVisualization
    {
        void Render(
            IMapData map,
            Character character,
            string message
        );
    }
}
