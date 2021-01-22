using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct ButtonsComponent:IComponentData
    {
        public ButtonsType ButtonsType;
    }
    
   

    public enum ButtonsType
    {
        Next,
        Restart
    }
}