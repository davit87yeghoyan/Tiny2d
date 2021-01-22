using Unity.Entities;

namespace Runtime.Components
{

    public struct LevelsComponent: IBufferElementData
    {
        public int Level;
        public int Relation;
        public Entity Entity;
    }

    public struct LevelBufferIndex : IComponentData
    {
        public int Index;
    }
 
}