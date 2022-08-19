using System;

using Godot;

namespace Pong
{
    public class Paddle : Area2D
    {
        private string texturePath;
        
        public int Speed
        {
            get;
            set;
        }
        
        public KeyList UpAction
        {
            get;
            set;
        }
        
        public KeyList DownAction
        {
            get;
            set;
        }
        
        public override void _Ready()
        {
            this.GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>(this.texturePath);
            this.Connect("area_entered", this, nameof(this.OnAreaEntered));
        }
        
        public override void _Process(float delta)
        {
            int up = Input.IsKeyPressed((int)this.UpAction) ? -1 : 0;
            int down = Input.IsKeyPressed((int)this.DownAction) ? 1 : 0;
            this.Position += new Vector2(0, this.Speed * delta * (up + down));
            this.Position = new(this.Position.x, Math.Clamp(this.Position.y, 16, this.GetViewportRect().Size.y - 16));
        }
        
        private void OnAreaEntered(Area2D area)
        {
            if (area is Ball ball)
            {
                ball.Direction = new Vector2(-Math.Sign(ball.Direction.x), ((float)new Random().NextDouble() * 2) - 1).Normalized();
            }
        }
    }
}