using Godot;

namespace Pong
{
    public class Goal : Area2D
    {
        public override void _Ready()
        {
            this.Connect("area_entered", this, nameof(this.OnAreaEntered));
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