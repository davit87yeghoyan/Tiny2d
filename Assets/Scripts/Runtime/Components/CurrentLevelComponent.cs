using Unity.Entities;

namespace Runtime.Components
{
    
    public struct CurrentLevelComponent:IComponentData
    {
        public int Level;
    }
}