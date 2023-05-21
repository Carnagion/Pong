using Godot;
using Godot.Modding;
using Godot.Utility.Extensions;
using System.Reflection;
using System;

namespace Pong
{
    internal sealed partial class Main : Node
    {
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            CallDeferred(nameof(LoadAllMods));

        }
        /** We need to load the assemblies from our domain into the reequest assemblies, fallback case if we dont have the assembly in our domain also */
        private Assembly? AssemblyResolve(object? sender, ResolveEventArgs args)
        {

            Assembly? ass = null;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            foreach (var domainAssembly in currentDomain.GetAssemblies())
            {
                if (domainAssembly.GetName().FullName == args.Name)
                {
                    ass = domainAssembly;
                    break;
                }
            }
            // fallback
            if (ass == null)
            {
                ass = Assembly.LoadFile(args.Name);
            }
            return ass;
        }

        private void LoadAllMods()
        {
            string modsPath = ProjectSettings.GlobalizePath("user://Mods");
            using DirAccess directory = DirAccess.Open("res://Mods");
            directory.CopyContents("res://Mods", modsPath, true);
            string[] modDirectoryPaths = System.IO.DirAccess.GetDirectories(modsPath);
            ModLoader.LoadMods(modDirectoryPaths);
        }
    }
}
