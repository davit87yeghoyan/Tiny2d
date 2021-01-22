using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Input;
using Unity.Tiny.Text;
using Unity.U2D.Entities.Physics;

namespace Runtime.Systems
{
    
    
    public  class ButtonsSystem : SystemBase
    {


        private CheckResultSystem _checkResultSystem;
        private DrawBordersSystem _drawBordersSystem;
        private DrawLevelSystem _drawLevelSystem;
        private InputSystem _input;
        
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ButtonsComponent>();
            _checkResultSystem = World.GetExistingSystem<CheckResultSystem>();
            _drawBordersSystem = World.GetExistingSystem<DrawBordersSystem>();
            _drawLevelSystem = World.GetExistingSystem<DrawLevelSystem>();
        }
        
       
        protected override void OnUpdate()
        {
            _input = World.GetExistingSystem<InputSystem>();
            var physicsWorld = World.GetExistingSystem<PhysicsWorldSystem>().PhysicsWorld;
            if (InputUtil.GetInputUp(_input))
            {
                float2 pos = CameraUtil.ScreenPointToWorldPoint(World, InputUtil.GetInputPosition(_input));
                Entity entity = DrawBordersSystem.GetInputEntity(physicsWorld,pos);
                if (entity == Entity.Null) return;
                ButtonsType buttonsType = EntityManager.GetComponentData<ButtonsComponent>(entity).ButtonsType;
                Button(buttonsType);
            }
        }


        private void Button(ButtonsType buttonsType)
        {
            Debug.Log("ButtonButtonButtonButtonButton");
            Entity mainEntity = GetSingletonEntity<GamePrefabsComponent>();
            EntityManager.AddComponent<InputActivationComponent>(mainEntity);
            EntityManager.DestroyEntity(_checkResultSystem.WindowEntity);
            int currentLevel = GetSingleton<CurrentLevelComponent>().Level;
            RemoveCurrentLevelEntity();
            switch (buttonsType)
            {
                case ButtonsType.Next:
                    ButtonNext(currentLevel,mainEntity);
                    break;
                case ButtonsType.Restart:
                    ButtonRestart(currentLevel,mainEntity);
                    break;
            }
        }

        private void ButtonNext(int currentLevel, Entity mainEntity)
        {
            int nextLevel = GetNextLevel(currentLevel);
            SetSingleton(new CurrentLevelComponent() {Level = nextLevel});
            EntityManager.AddComponentData(mainEntity, new DrawLevelComponent {Level = nextLevel});
        }
        
        private void ButtonRestart(int currentLevel,Entity mainEntity)
        {
            EntityManager.AddComponentData(mainEntity, new DrawLevelComponent {Level = currentLevel});
        }

        private int GetNextLevel(int currentLevel)
        {
            int maxLevel = GetSingleton<MaxLevelComponent>().MaxLevel;
            if (++currentLevel > maxLevel)
            {
                return 1;
            }
            return currentLevel;
        }


        private void RemoveCurrentLevelEntity()
        {
            _drawBordersSystem.RestAll();
            _drawLevelSystem.RemoveLevelEntityAll();
        }
        
    }
}
