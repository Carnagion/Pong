using Godot;

namespace Pong
{
    public class Wall : Area2D
    {
        public Vector2 BounceDirection
        {
            get;
            set;
        }

        public override void _Ready()
        {
            this.Connect("area_entered", this, nameof(this.OnAreaEntered));
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is Ball ball)
            {
                ball.Direction = (ball.Direction + this.BounceDirection).Normalized();
            }
        }
    }
}