using Godot;
using Godot.Modding;
using Godot.Utility.Extensions;

namespace Pong
{
    internal sealed class Main : Node
    {
        public override void _Ready()
        {
            this.CallDeferred(nameof(this.LoadAllMods));
        }
        
        private void LoadAllMods()
        {
            using Directory directory = new();
            directory.CopyContents("res://Mods", "user://Mods", true);
            
            string modsPath = ProjectSettings.GlobalizePath("user://Mods");
            string[] modDirectoryPaths = System.IO.Directory.GetDirectories(modsPath);
            
            ModLoader.LoadMods(modDirectoryPaths);
        }
    }
}
