using System.Xml;

using Godot;
using Godot.Modding;
using Godot.Serialization;

namespace Pong
{
    public static class Game
    {
        [ModStartup]
        public static void OnModStartup()
        {
            Mod pongCore = ModLoader.LoadedMods["Pong-Core"];
            XmlDocument data = pongCore.Data!;
            
            XmlNode ballXml = data.SelectSingleNode("//*[@Id=\"Ball\"]")!;
            XmlNode paddleLeftXml = data.SelectSingleNode("//*[@Id=\"PaddleLeft\"]")!;
            XmlNode paddleRightXml = data.SelectSingleNode("//*[@Id=\"PaddleRight\"]")!;
            XmlNode wallTopXml = data.SelectSingleNode("//*[@Id=\"WallTop\"]")!;
            XmlNode wallBottomXml = data.SelectSingleNode("//*[@Id=\"WallBottom\"]")!;
            XmlNode goalLeftXml = data.SelectSingleNode("//*[@Id=\"GoalLeft\"]")!;
            XmlNode goalRightXml = data.SelectSingleNode("//*[@Id=\"GoalRight\"]")!;
            
            Serializer serializer = new();

            Ball ball = serializer.Deserialize<Ball>(ballXml)!;
            Paddle paddleLeft = serializer.Deserialize<Paddle>(paddleLeftXml)!;
            Paddle paddleRight = serializer.Deserialize<Paddle>(paddleRightXml)!;
            Wall wallTop = serializer.Deserialize<Wall>(wallTopXml)!;
            Wall wallBottom = serializer.Deserialize<Wall>(wallBottomXml)!;
            Goal goalLeft = serializer.Deserialize<Goal>(goalLeftXml)!;
            Goal goalRight = serializer.Deserialize<Goal>(goalRightXml)!;
            
            SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
            
            sceneTree.Root.AddChild(ball);
            sceneTree.Root.AddChild(paddleLeft);
            sceneTree.Root.AddChild(paddleRight);
            sceneTree.Root.AddChild(wallTop);
            sceneTree.Root.AddChild(wallBottom);
            sceneTree.Root.AddChild(goalLeft);
            sceneTree.Root.AddChild(goalRight);
        }
    }
}