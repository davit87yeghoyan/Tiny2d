using System;
using System.Collections.Generic;
using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Animation;
using Unity.Tiny.Text;
using Unity.Transforms;
using Unity.U2D.Entities.Physics;

namespace Runtime.Systems
{
    

    public class DrawBordersSystem : SystemBase
    {

        public List<ProcessBorder> ProcessBorders = new List<ProcessBorder>();

        private ProcessBorder _processBorder;
        private int _entityDirections;
        private Entity _entityLineTemp,_entityLineTempOld;
        private bool _down;
        private float2 _oldPos;
        


        public class ProcessBorder
        {
            public Entity Start = Entity.Null;
            public Entity End = Entity.Null;
            public Entity StartBorder = Entity.Null;
            public Entity EndBorder = Entity.Null;
            public Entity LineBorder = Entity.Null;
        }


        private GamePrefabsComponent _gamePrefabsComponent;

        private InputToActionSystem _inputToActionSystem;


      
        protected override void OnCreate()
        {
            base.OnCreate();
            _inputToActionSystem = World.GetExistingSystem<InputToActionSystem>();
            _inputToActionSystem.StartProses += ()=>OnProcess(_inputToActionSystem.PositionDown,true);
            _inputToActionSystem.EndProses += ()=>OnProcess(_inputToActionSystem.PositionUp,false);
            RestAll();
        }
        
        protected override void OnStartRunning()
        {
            _gamePrefabsComponent = GetSingleton<GamePrefabsComponent>();
        }

        
        private void OnProcess(float2 worldPos, bool down)
        {
            _down = down;
            var physicsWorld = World.GetExistingSystem<PhysicsWorldSystem>().PhysicsWorld;
            Entity entity = GetInputEntity(physicsWorld,worldPos);
            if (entity == Entity.Null) return;
            DrawBorder(entity,down);
        }


        private void DrawBorder(Entity entity, bool down)
        {

            if(entity == Entity.Null) return;
            var pos = EntityManager.GetComponentData<Translation>(entity).Value;
            var index = EntityManager.GetComponentData<LevelBufferIndex>(entity).Index;
            if(_entityDirections == index) return;
            
            if (this._processBorder.Start != Entity.Null && this._processBorder.End == Entity.Null && down)
            {
                return;
            }
            
            // if click  created line item,  remove item
            var processBorder = GetProcessBorder(entity);
            if (processBorder !=null)
            {
                RemoveCurrentProcessBorder(processBorder);
            }
            
            
            
            
            if (index % 2 == _entityDirections%2)
            {
                RemoveCurrentProcessBorder(this._processBorder);
                this._processBorder = new ProcessBorder();
            }
           

            _entityDirections = index;
            
            var entityClip = EntityManager.GetComponentData<TinyAnimationPlayer>(entity).CurrentClip;
            EntityManager.SetComponentData(entityClip, new TinyAnimationTime() {Value = 0});
          
            
            var entityB = EntityManager.Instantiate(_gamePrefabsComponent.Border);
            EntityManager.SetComponentData(entityB, new Translation { Value = pos });
            
            
            if (this._processBorder.Start == Entity.Null)
            {
                this._processBorder.StartBorder = entityB;
                this._processBorder.Start = entity;
            }else if (this._processBorder.End == Entity.Null)
            {
                this._processBorder.EndBorder = entityB;
                this._processBorder.End = entity;
            }
         
            
            if(this._processBorder.Start != Entity.Null && this._processBorder.End != Entity.Null && this._processBorder.LineBorder == Entity.Null)
            {
                this._processBorder.LineBorder = DrawLine();
                ProcessBorders.Add(this._processBorder);
                this._processBorder = new ProcessBorder();
                _entityDirections = -1;
                Entity mainEntity = GetSingletonEntity<GamePrefabsComponent>();
                EntityManager.AddComponent(mainEntity,typeof(CheckResultComponent));
            }

        }


        private void RemoveCurrentProcessBorder(ProcessBorder processBorder)
        {
            EntityManager.DestroyEntity(processBorder.StartBorder);
            EntityManager.DestroyEntity(processBorder.EndBorder);
            EntityManager.DestroyEntity(processBorder.LineBorder);
            ProcessBorders.Remove(processBorder);
        }
        
        private Entity DrawLine()
        {
            float3 posS = EntityManager.GetComponentData<Translation>(_processBorder.Start).Value;
            float3 posE = EntityManager.GetComponentData<Translation>(_processBorder.End).Value;
            return DrawLine(posS,posE);
        }

        private Entity DrawLine(float3 posS, float3 posE)
        {
            float3 center = math.lerp(posS, posE, 0.5f);

            float deltaX = posS.x - posE.x;
            float deltaY = posS.y - posE.y;
            float thetaRadians = math.atan2(deltaY, deltaX);
            float line = math.distance(posS, posE)*3.4f;
            Entity entityPrefab = EntityManager.Instantiate(_gamePrefabsComponent.Line);
            EntityManager.SetComponentData(entityPrefab, new Translation { Value = center });
            EntityManager.SetComponentData(entityPrefab, new Rotation() { Value = quaternion.Euler(0,0,thetaRadians) });
            EntityManager.AddComponentData(entityPrefab, new NonUniformScale() {Value = new float3(line,1.2f,1.2f)});
            return entityPrefab;
        }
        

        
        
        
        private ProcessBorder GetProcessBorder(Entity entity)
        {
            foreach (var t in ProcessBorders)
            {
                if (t.Start == entity || t.End == entity)
                {
                    return t;
                }
            }
            return null;
        }


        protected override void OnUpdate()
        {
            if (!_down)
            {
                EntityManager.DestroyEntity(_entityLineTempOld);
                _entityLineTempOld = _entityLineTemp;     
            }

          
            if (_processBorder.Start != Entity.Null && _processBorder.End == Entity.Null && _down && _inputToActionSystem.Process)
            {
                if(_oldPos.Equals(_inputToActionSystem.PositionCurrent)) return;
                
                _oldPos = _inputToActionSystem.PositionCurrent;
                float3 posS = EntityManager.GetComponentData<Translation>(_processBorder.Start).Value;
                float3 float3 = new float3(_inputToActionSystem.PositionCurrent.x,_inputToActionSystem.PositionCurrent.y,0);
               
                
                EntityManager.DestroyEntity(_entityLineTempOld);
                _entityLineTempOld = _entityLineTemp;  
                _entityLineTemp = DrawLine(posS,float3);
            }
        }


        public static Entity GetInputEntity(PhysicsWorld physicsWorld, float2 worldPos)
        {
            var pointInput = new OverlapPointInput()
            {
                Position = worldPos,
                Filter = CollisionFilter.Default
            };
            
            if (physicsWorld.OverlapPoint(pointInput, out var overlapPointHit))
            {
                var body = physicsWorld.AllBodies[overlapPointHit.PhysicsBodyIndex];
                return body.Entity;
            }
            return Entity.Null;
        }

        public void RestAll()
        {
            foreach (var t in ProcessBorders)
            {
                EntityManager.DestroyEntity(t.End);
                EntityManager.DestroyEntity(t.Start);
                EntityManager.DestroyEntity(t.EndBorder);
                EntityManager.DestroyEntity(t.StartBorder);
                EntityManager.DestroyEntity(t.LineBorder);
            }
            EntityManager.DestroyEntity(_entityLineTemp);
            EntityManager.DestroyEntity(_entityLineTempOld);

            ProcessBorders = new List<ProcessBorder>();
            _entityDirections = -1;
            _processBorder = new ProcessBorder();
            _down = false;
            _oldPos = new float2();
            _entityLineTemp = Entity.Null;
            _entityLineTempOld = Entity.Null;
        }
        
    }
}
