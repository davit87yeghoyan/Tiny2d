using Runtime.Components;
using Unity.Entities;
using Unity.Tiny;

namespace Runtime.Systems
{
    public class CheckResultSystem : SystemBase
    {
        private DrawBordersSystem _drawBordersSystem;
        private DrawLevelSystem _drawLevelSystem;

        public Entity WindowEntity = Entity.Null;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<CheckResultComponent>();
            _drawBordersSystem = World.GetExistingSystem<DrawBordersSystem>();
            _drawLevelSystem = World.GetExistingSystem<DrawLevelSystem>();
        }

        protected override void OnUpdate()
        {
            if(_drawLevelSystem.RelationCount > _drawBordersSystem.ProcessBorders.Count) return;
            Entity mainEntity = GetSingletonEntity<GamePrefabsComponent>();
            EntityManager.RemoveComponent<CheckResultComponent>(mainEntity);
            EntityManager.RemoveComponent<InputActivationComponent>(mainEntity);
            var prefabs = GetSingleton<GamePrefabsComponent>();
            var buffer = GetBuffer<LevelsComponent>(mainEntity);
            
            foreach (var t in _drawBordersSystem.ProcessBorders)
            {
                var start = EntityManager.GetComponentData<LevelBufferIndex>(t.Start);
                var end = EntityManager.GetComponentData<LevelBufferIndex>(t.End);
                if (buffer[start.Index].Relation == buffer[end.Index].Relation) continue;
                WindowEntity = EntityManager.Instantiate(prefabs.NoAll);
                return;
            }
            WindowEntity = EntityManager.Instantiate(prefabs.OkAll);
        }
    }
}