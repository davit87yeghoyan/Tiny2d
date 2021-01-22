using Unity.Entities;

namespace Runtime.Components
{
    
    public struct GamePrefabsComponent:IComponentData
    {
        public Entity Border;
        public Entity Line;
        public Entity Icon;
        public Entity OkAll;
        public Entity NoAll;
    }
}