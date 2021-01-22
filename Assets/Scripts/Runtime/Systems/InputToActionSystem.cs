using System;
using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Input;
using Unity.Tiny.Text;

namespace Runtime.Systems
{
    public  class InputToActionSystem : SystemBase
    {
        public bool Process;
        public float2 PositionDown;
        public float2 PositionUp;
        public float2 PositionCurrent;
        public event Action StartProses;
        public event Action EndProses;

        
        private InputSystem _input;
        
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<InputActivationComponent>();
        }
     

        protected override void OnUpdate()
        {
            _input = World.GetExistingSystem<InputSystem>();
            
            switch (Process)
            {
                case true:
                {
                    PositionCurrent = CameraUtil.ScreenPointToWorldPoint(World, InputUtil.GetInputPosition(_input));
                    if (InputUtil.GetInputUp(_input))
                    {
                        PositionUp = PositionCurrent;
                        Process = false;
                        OnEndProses();
                    }

                    break;
                }
                case false when InputUtil.GetInputDown(_input):
                    PositionDown = CameraUtil.ScreenPointToWorldPoint(World, InputUtil.GetInputPosition(_input));
                    Process = true;
                    OnStartProses();
                    break;
            }
        }


        private void OnStartProses()
        {
            StartProses?.Invoke();
        }


        private void OnEndProses()
        {
            EndProses?.Invoke();
        }
    }
}
