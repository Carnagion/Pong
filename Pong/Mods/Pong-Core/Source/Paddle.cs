using System;

using Godot;

namespace Pong
{
    public partial class Paddle : Area2D
    {
        public int Speed
        {
            get;
            set;
        }

        public Key UpAction
        {
            get;
            set;
        }

        public Key DownAction
        {
            get;
            set;
        }

        private string texturePath = "";

        public override void _Ready()
        {
            this.GetNode<Sprite2D>("Sprite2D").Texture2D = GD.Load<Texture2D>(this.texturePath);
            AreaEntered += OnAreaEntered;
        }

        public override void _Process(double delta)
        {
            int up = Input.IsKeyPressed(this.UpAction) ? -1 : 0;
            int down = Input.IsKeyPressed(this.DownAction) ? 1 : 0;
            this.Position += new Vector2(0, this.Speed * (float)delta * (up + down));
            this.Position = new(this.Position.X, Math.Clamp(this.Position.Y, 16, this.GetViewportRect().Size.Y - 16));
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is Ball ball)
            {
                ball.Direction = new Vector2(-Math.Sign(ball.Direction.X), ((float)new Random().NextDouble() * 2) - 1).Normalized();
            }
        }
    }
}