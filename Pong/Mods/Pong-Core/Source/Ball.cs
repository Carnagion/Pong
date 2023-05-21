using Godot;

namespace Pong
{
    public partial class Ball : Area2D
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
            this.GetNode<Sprite2D>("Sprite2D").Texture2D = GD.Load<Texture2D>(this.texturePath);
        }

        public override void _Process(double delta)
        {
            this.Position += this.Speed * this.Direction * (float)delta;
        }

        public void Reset()
        {
            this.Position = this.initialPosition;
        }
    }
}