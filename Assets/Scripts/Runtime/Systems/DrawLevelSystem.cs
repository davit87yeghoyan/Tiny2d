using System;
using System.Collections.Generic;
using Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Text;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Runtime.Systems
{
    public class DrawLevelSystem:SystemBase
    {

        public int RelationCount;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<DrawLevelComponent>();
        }

        protected override void OnUpdate()
        {
            var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
            Entities
                .WithAll<DrawLevelComponent>()
                .WithoutBurst()
                .ForEach((Entity entity,in DrawLevelComponent drawLevelComponent, in GamePrefabsComponent gamePrefabsComponent) =>{
                    Draw(entity,cmdBuffer,drawLevelComponent,gamePrefabsComponent);
                    cmdBuffer.RemoveComponent<DrawLevelComponent>(entity);
                }).Run();
            
            cmdBuffer.Playback(EntityManager);
            cmdBuffer.Dispose();
        }



        private void Draw(Entity entity, EntityCommandBuffer cmdBuffer, DrawLevelComponent drawLevelComponent, GamePrefabsComponent gamePrefabsComponent)
        {
          
            var levelsComponents = GetBuffer<LevelsComponent>(entity);
            
            List<int> indexs = new List<int>();

            List<int> relations = new List<int>();
            
            for (int i = 0; i < levelsComponents.Length; i++)
            {
                if (levelsComponents[i].Level != drawLevelComponent.Level)  continue;
                indexs.Add(i);
                if (!relations.Contains(levelsComponents[i].Relation))
                {
                    relations.Add(levelsComponents[i].Relation);
                }
                
            }

            uint time = (uint)DateTime.Now.Millisecond;
            var rand = new Random(time);
            var count = indexs.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = rand.NextInt(i, count);
                var tmp = indexs[i];
                indexs[i] = indexs[r];
                indexs[r] = tmp;
            }
            
            

            //inside system
            ComponentDataFromEntity<SpriteRenderer> sDataFromEntity = GetComponentDataFromEntity<SpriteRenderer>(true);


            int leftIndex = 0, rightIndex = 0;
            
            foreach (var index in indexs)
            {
                float3 pos = GetPositionByIndex(index,ref leftIndex, ref rightIndex);
                Entity entityPrefab = cmdBuffer.Instantiate(gamePrefabsComponent.Icon);

                var spriteRenderer = sDataFromEntity[gamePrefabsComponent.Icon];
                spriteRenderer.Sprite = levelsComponents[index].Entity;
                
                cmdBuffer.SetComponent(entityPrefab, spriteRenderer);
                cmdBuffer.SetComponent(entityPrefab, new Translation { Value = pos });
                cmdBuffer.AddComponent(entityPrefab, new LevelBufferIndex { Index = index });
            }
            
            
            RelationCount = relations.Count;
            
            Entity entityLevel = GetSingletonEntity<TextLevelComponent>();
            TextLayout.SetEntityTextRendererString(EntityManager, entityLevel,"Level: "+drawLevelComponent.Level);
        }

        
        private float3 GetPositionByIndex(int indexGlobal,  ref int leftIndex, ref int rightIndex)
        {
            int i =0;
            float3 pos = new float3();
            if(indexGlobal%2==0)
            {
                i= ++leftIndex;
                pos.x = -4;
            }
            else
            {
                i= ++rightIndex;
                pos.x = 4;
                
            }
            
            pos.y -= i*4 + i*2  - 16;

            return pos;
        }

        public void RemoveLevelEntityAll()
        {

            var query = GetEntityQuery(typeof(LevelBufferIndex));
            NativeArray<Entity> entityAll = query.ToEntityArray(Allocator.Temp);
            foreach (var t in entityAll)
            {
                EntityManager.DestroyEntity(t);
            }
        }
        
    }
    
}