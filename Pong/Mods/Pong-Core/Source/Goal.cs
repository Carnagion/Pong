using Godot;

namespace Pong
{
    public partial class Goal : Area2D
    {
        public override void _Ready()
        {
            AreaEntered += OnAreaEntered;
        }
        
        private void OnAreaEntered(Area2D area)
        {
            if (area is Ball ball)
            {
                ball.Reset();
            }
        }
    }
}