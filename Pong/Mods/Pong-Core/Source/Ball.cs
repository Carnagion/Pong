using Godot;

namespace Pong
{
    public class Ball : Area2D
    {
        private string texturePath;
        
        private Vector2 initialPosition;

        public int Speed
        {
            get;
            set;
        }
        
        public Vector2 Direction
        {
            get;
            set;
        }

        public override void _Ready()
        {
            this.GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>(this.texturePath);
        }

        public override void _Process(float delta)
        {
            this.Position += this.Speed * this.Direction * delta;
        }

        public void Reset()
        {
            this.Position = this.initialPosition;
        }
    }
}