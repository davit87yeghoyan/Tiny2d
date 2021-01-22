using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class GameMain: MonoBehaviour,IConvertGameObjectToEntity,IDeclareReferencedPrefabs
    {

        
        public int currentLevel = 1;

        public GameObject border;
        public GameObject line;
        public GameObject icon;
        public GameObject okAll;
        public GameObject noAll;

        [SerializeField]
        public Levels[] levels;

     
        [Serializable]
        public class Levels
        {
            [SerializeField]
            public LevelInfo[] level;
        }
        
        [Serializable]
        public class LevelInfo
        {
            [SerializeField]
            public Sprite[] relation;
        }
        

      
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            DynamicBuffer<LevelsComponent> dynamicBuffer = dstManager.AddBuffer<LevelsComponent>(entity);
            
            for (var level = 0; level < levels.Length; level++)
            {
                for (var relation = 0; relation < levels[level].level.Length; relation++)
                {
                    for (var i1 = 0; i1 < levels[level].level[relation].relation.Length; i1++)
                    {
                        Entity primaryEntity = conversionSystem.GetPrimaryEntity(levels[level].level[relation].relation[i1]);
                       
                       dynamicBuffer.Add(new LevelsComponent()
                        {
                            Level = level+1,
                            Relation = relation,
                            Entity = primaryEntity
                        });
                    }
                }
            }
            
            dstManager.AddComponentData(entity, new CurrentLevelComponent {Level   = currentLevel});
            dstManager.AddComponentData(entity, new DrawLevelComponent {Level   = currentLevel});
            dstManager.AddComponentData(entity, new MaxLevelComponent {MaxLevel   = levels.Length});
            dstManager.AddComponentData(entity, new InputActivationComponent());
            dstManager.AddComponentData(entity, new GamePrefabsComponent()
            {
                Border   = conversionSystem.GetPrimaryEntity(border),
                Line   = conversionSystem.GetPrimaryEntity(line),
                Icon   = conversionSystem.GetPrimaryEntity(icon),
                OkAll   = conversionSystem.GetPrimaryEntity(okAll),
                NoAll   = conversionSystem.GetPrimaryEntity(noAll),
            });
        }
        
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(border);
            referencedPrefabs.Add(line);
            referencedPrefabs.Add(icon);
            referencedPrefabs.Add(okAll);
            referencedPrefabs.Add(noAll);
        }
      
    }
    
    
    /// <summary>
    /// for sprites registration
    /// </summary>
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))] 
    public class DeclareCellSpriteReference : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GameMain mgr) =>
            {
                foreach (Sprite sprite in from t in mgr.levels from t1 in t.level from t2 in t1.relation select t2) DeclareReferencedAsset(sprite);
            });
        }
    }
}