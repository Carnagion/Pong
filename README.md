# Pong

Welcome to the **Pong** walkthrough!

The aim of this demo is to provide a step-by-step tutorial on creating a simple, moddable game using [Modot](https://github.com/Carnagion/Modot).

At the end of the walkthrough, you will have a fully playable game of Pong, as well as a mod that changes some parts of the game.

During the tutorial, you will learn:
- Structuring moddable projects
- Creating and loading mods
- Deserializing nodes and other objects from XML data
- Changing XML data using mod patches

and more.

# Prerequisites

This walkthrough assumes that you have:
- A basic understanding of C#
- A basic understanding of Godot
- A basic understanding of XML or other data serialization formats (such as JSON) and how they are commonly to save or load data

> **Note:** If you are new to Godot, C#, or programming in general, consider completing Godot's official [Getting Started](https://docs.godotengine.org/en/3.5/getting_started/introduction/index.html#) section first.

You will also need to download/install the following, if you have not already done so:
- [Godot Mono](https://godotengine.org/download) v3.4 (along with its dependencies such as the .NET SDK)
- [Export templates](https://godotengine.org/download) for Godot Mono v3.4
- [Assets](https://github.com/Carnagion/Pong/Assets) for the game
- A good C# editor or IDE, such as [Rider](https://www.jetbrains.com/rider/), [Visual Studio](https://visualstudio.microsoft.com/), or [Visual Studio Code](https://code.visualstudio.com/)

> **Note:** Downloading an external editor is optional, but *highly* recommended. Most editors and IDEs provide advanced features - such as autocomplete, improved intellisense, quick fixes, and so on - which are not available in Godot's in-built script editor.

# Project structure

A normal Godot application would usually consist of a single Godot project, with all source code and assets contained in a single directory (`res://`).

With **Modot**, however, you have the option to use a more modularised approach by splitting up your application into several mods - each having its own Godot project. These mods can be bundled into mod directories and can then be loaded at runtime by a main project.

Such a modular project structure has many advantages over a more traditional one:
- You can treat the application itself as a mod, which allows other mod authors to easily patch its XML data, or load other mods before it
- There is a smaller chance of causing a bug in one mod's source code by editing another mod
- It becomes easier to add (or remove) features from the application

Therefore, for this demo, a modular structure will be used - it will consist of a single mod project containing all the source code and assets, which will then be loaded by a main project. These two projects - the main and the mod - will be called "Pong" and "Pong-Core" respectively.

> **Note:** Structuring a Godot application as multiple mods is optional, but recommended. **Modot** was designed with such usage in mind, and can be used to its full extent with a modular project structure.

In this demo, bundled mods - the mods that are part of the application itself - will be stored in the `res://Mods` directory, while user-created mods - mods that are optional and (usually) created by users - will be loaded from the `user://Mods` directory.

# Setting up the main project

The main project - named "Pong" - will be the entry point of the application, and will be responsible for loading all mods - both bundled mods (from `res://Mods`) and user-created mods (from `user://Mods`).

First, launch Godot and create a new project named "Pong".

> **Note:** You may pick whichever renderer version you want. This walkthrough uses `OpenGL ES 3.0`, though if you want to run the game on a browser or on older hardware, you should choose `OpenGL ES 2.0`.

As this project needs to load mods, it will require **Modot** as a dependency. However, to do that, a few changes must be made to the C# project file as Godot's default C# project settings are not compatible with **Modot**.

You may notice that the C# project file is not created yet - this is because Godot only generates a C# project and solution file when a C# file is created. To fix this, create a new C# script (name it whatever you want) from within the editor, then delete it immediately.

> **Note:** If a C# solution and project file have been generated, the "Build" button will be visible in the top-right corner of the Godot editor.

Now open the C# project file (`Pong.csproj`) - you may need an external editor to do this.

The first (and most important) change is to set the target framework to .NET Standard 2.1. **Modot** only supports .NET Standard 2.1 and will not work with .NET Framework 4.7.2 or any other target framework. You can change the target framework like so (the value will be `net472` by default):
```xml
<TargetFramework>netstandard2.1</TargetFramework>
```

Next, add **Modot** as a dependency. It is available as a [NuGet package](https://nuget.org/packages/Modot), so it can easily be installed by including the following lines:
```xml
<ItemGroup>
    <PackageReference Include="Modot" Version="2.0.2"/>
</ItemGroup>
```

> **Note:** If you have an IDE such as Rider or Visual Studio, you can use their in-built GUIs for NuGet to help you add dependencies without manually editing the `.csproj` file.

**Modot** is also dependent on certain libraries, which are available as NuGet packages as well:
- [GDSerializer](https://github.com/Carnagion/GDSerializer)
- [GDLogger](https://github.com/Carnagion/GDLogger)
- [Carnagion.MoreLinq](https://github.com/Carnagion/MoreLinq)
- [System.CodeDom](https://www.nuget.org/packages/System.CodeDom/7.0.0-preview.7.22375.6)
- [JetBrains.Annotations](https://github.com/JetBrains/ExternalAnnotations)

These are automatically (*implicitly*) installed in your project when you add **Modot**, so there is no need to manually add them to the C# project file.

> **Note:** You can still explicitly install these dependencies if you want to in certain cases, such as when you want a different version than what is implcitly installed.

Then, add the following line under the `<PropertyGroup>` node:
```xml
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```

This ensures that the assemblies of all dependencies are copied over to your local device when building.

> **Bug:** Doing this is necessary due to a [bug](https://github.com/godotengine/godot/issues/42271) in Godot related to NuGet packages in .NET Standard 2.1. Without copying the assemblies, Godot cannot properly export projects that use NuGet packages.

Finally, set the language version to `default`, and enable nullability analysis by adding the following tags under the `<PropertyGroup>` node:
```xml
<LangVersion>default</LangVersion>
<Nullable>enable</Nullable>
```

Doing this provides a better development experience as **Modot**'s and **GDSerializer**'s APIs were created with these settings in mind.

> **Note:** Enabling nullability analysis and changing the language version is optional, but *highly* recommended. They provide newer C# syntax features and better intellisense support from external editors.

Now, navigate to `Project` -> `Project Settings...` -> `Display` -> `Window` and set the width and height to 640 and 480 respectively.

# Loading mods

The vast majority of games load mods as soon as they startup, though there are a few that load mods at other times, such as when actually loading a save file or starting a new save. **Modot** is highly flexible, and the code for loading mods remains the same regardless of when you choose to load them.

In this demo, mods will be loaded as soon as the game starts up - i.e. as soon as Godot calls the `_Ready()` method on the first (main) scene's root node.

> **Note:** Loading mods may be an intensive process depending on the number of mods being loaded, the size of their XML data, and whether they execute any patches or code upon loading. If you expect mod loading to take a significant amount of time, it is best do this process in a separate thread to prevent blocking the main thread - though this will not be covered here.

First, create a new scene - a simple `Node` will suffice as the root node, since it does not need to be displayed or have any other special functionality. Save this scene as `Main.tscn`.

Now, create a C# file to attach to this scene - save it as `Main.cs`. Attach it to the root node of `Main.tscn` and then open the file.

Remove any unncessary comments and the `using System;` directive - nothing from the `System` namespace will be used here. Go ahead and add these `using` directives instead:
```cs
using Godot;
using Godot.Modding;
using Godot.Utility.Extensions;
```

These include all of the classes necessary for loading mods, as well as some helpful extension methods.

You can also put the this class inside a namespace, and change its modifiers like so:
```cs
namespace Pong
{
    internal sealed class Main : Node
    {
    }
}
```

> **Note:** Adding a namespace and editing modifiers is optional, though recommended. Having a namespace that matches the project name and/or directory structure is a part of C# conventions - this project is called "Pong", hence the `Pong` namespace. Adding the `internal` and `sealed` modifiers helps the C# compiler perform some optimisations, and also prevents external code (such as from a mod) from accidentally instantiating `Main` using `new Main()`.

Now create a method called `LoadAllMods()`, which (as you might have guessed) will be in charge of loading mods:
```cs
private void LoadAllMods()
{
}
```

**Modot** requires the full, native OS paths of all mod directories in order to load mods - this is because the C# assembly loading and XML document API do not recognise Godot paths (i.e. starting with `res://` and `user://`).

> **Wiki:** Read [Defining mods](https://github.com/Carnagion/Modot/wiki/Defining-mods) in the **Modot** wiki to understand more about what constitutes a mod directory.

However, `res://` paths may not have a native OS equivalent due to the way Godot exports its projects - meaning that mods from `res://Mods` will first have to be copied over to `user://Mods` before loading them. This can be done like so:
```cs
using Directory directory = new();
directory.CopyContents("res://Mods", "user://Mods", true);
```

> **Wiki:** Read [Loading mods](https://github.com/Carnagion/Modot/wiki/Loading-mods) in the **Modot** wiki to understand more about how and why mods from `res://` must be copied over to a corresponding `user://` directory.

This way, all mods - both bundled and user-created - will be located under `user://Mods`. The `user://Mods` directory will be automatically created if it doesn't exist.

> **Note:** Sub-directories and files from the source directory (`res://Mods`) will overwrite those in the destination directory (`user://Mods`) that have the same relative path, so be careful when naming mod directories.

You may notice that the `res://Mods` directory doesn't actually exist yet, so go ahead and create it - though it will be empty for now.

The full paths of all mod directories can then be obtained as follows:
```cs
string modsPath = ProjectSettings.GlobalizePath("user://Mods");
string[] modDirectoryPaths = System.IO.Directory.GetDirectories(modsPath);
```

Then, the `ModLoader` class can be used to actually load the mods from the given mod directory paths:
```cs
ModLoader.LoadMods(modDirectoryPaths);
```

> **Wiki:** Read [Loading mods](https://github.com/Carnagion/Modot/wiki/Loading-mods) in the **Modot** wiki to find out more about the `ModLoader` class and different ways of loading mods.

And that should be all for the `LoadAllMods()` method. This method should be called in `_Ready()` using `CallDeferred()` like so:
```cs
public override void _Ready()
{
    this.CallDeferred(nameof(this.LoadAllMods));
}
```

> **Note:** `CallDeferred()` is used instead of simply loading all mods directly in `_Ready()` because if a mod tries to add a scene or node to the tree using `AddChild()` during loading, then Godot will throw an error as the main scene's `_Ready()` function won't have finished.

# Setting up the core game

Now comes the actual game content, which will be in a new Godot project - named "Pong-Core". This project will be exported as a mod, which will then be loaded by the "Pong" project.

Close the "Pong" project and create a new project named "Pong-Core", following the earlier instructions for installing dependencies.

> **Note:** While this project does not need to load any mods using `ModLoader`, it does need to execute code on startup, which is a feature provided by **Modot** - so it will still need to include it as a dependency.

If done right, you should have a project called "Pong-Core" with its C# project file's (`Pong-Core.csproj`) contents looking exactly like the "Pong" project's (`Pong.csproj`).

If you made a `Main.cs` file for this project, delete it - there is no need to have a main scene in this project since it will be loaded as a mod.

# Downloading assets

If you have not done so already, download the game's assets - the link is available in the "Prerequisites" section of this tutorial. Extract all of its contents into the "Pong-Core" project - you should have `Ball.png`, `PaddleRight.png`, and `PaddleLeft.png`, all under `res://`.

# Defining mod metadata

The next step is to define a mod metadata file for "Pong-Core". A mod metadata file is an XML file containing important information about a mod, such as its unique ID, name, author, dependencies, load order, etc. **Modot** uses these to sort mods properly while loading, which ensures that mods are loaded in the most compatible way possible.

> **Wiki:** Read [Defining mods](https://github.com/Carnagion/Modot/wiki/Defining-mods) in the **Modot** wiki to understand more about what kinds of data a mod metadata file can provide, and how it is used.

Since "Pong-Core" does not depend on any other mods, does not have any known incompatibilities, and does not have any specific load order requirements, a simple mod metadata file will suffice.

Create a file named `Mod.xml` with the following data:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Mod>
    <Id>Pong-Core</Id>
    <Name>Pong-Core</Name>
    <Author>YOUR NAME</Author>
</Mod>
```

> **Note:** While the mod ID and name have the same value above, they do not necessarily have to be the same. The ID is a unique string used to identify a mod, whereas the name is more for display purposes and need not be unique.

This will be moved to a different location later, but can stay as it is for now.

# Game content

The classic **Pong** game contains two paddles - controlled by a player each - and a ball, which can collide with the paddles and bounce back. A player earns a point when their opponent is unable to hit the ball back to them, and the game ends when one of the players achieves 11 points.

From this description, it is clear that the following classes are needed:
- A `Ball` class, which will control the movement of the ball
- A `Paddle` class, which will control the movements of each paddle according to player input, and will bounce the ball when it hits
- A `Wall` class (upper and lower walls), which will bounce the ball back when it hits
- A `Goal` class (side walls), which will reset the ball's position when it passes through

For the sake of simplicity, this demo will not implement score-keeping or a "game over" - the game will be endless.

## Ball

To begin with, create a C# file named `Ball.cs` - this will contain the code for the ball.

The ball needs to be able to collide with the paddles, walls, and goals, so it should inherit `Area2D`, and it should also have a child `CollisionShape2D` node with the appropriate shape - a `CircleShape2D` will suffice. It needs to be displayed, so a child `Sprite` will be needed as well. 

In addition, it will need fields and properties for speed, direction, and initial position, as well as a method to reset itself:
```cs
using Godot;

namespace Pong
{
    public class Ball : Area2D
    {
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
```

## Paddle

Next, create a C# file named `Paddle.cs`, which will contain code for the paddles.

The paddles will need to collide with the ball, so they too should inherit `Area2D` and have a child `CollisionShape2D` node. For the collision shape, a `RectangleShape2D` will suffice. Like the ball, they will need a child `Sprite` node to display themselves.

Each paddle will also need a property for speed, and will also know which keys to use for moving up or down. They should bounce the ball when it hits them, preferably in a slightly random direction:
```cs
using System;

using Godot;

namespace Pong
{
    public class Paddle : Area2D
    {   
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
```

## Wall

Now create a C# file named `Wall.cs` for the wall code.

The walls need to collide with the ball, so they too will have to inherit from `Area2D` and have a child `CollisionShape2D` with a `RectangleShape2D` collision shape. Unlike the ball and paddles though, the walls don't need to be seen.

They will, however, need a property to let them know which direction to bounce the ball in, and they should also bounce the ball when it hits them:
```cs
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
```

## Goal

And finally, create a file named `Goal.cs`, which will contain the code for the goals.

Just like the walls, the goals will need to inherit from `Area2D`, have a child `CollisionShape2D` with a `RectangleShape2D` shape, and will not be visible.

They will also need to detect and reset the ball when it enters them:
```cs
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
```

# Storing data as XML

Now that the code for the game's main classes is done, the usual Godot way would be to create scenes for each one - `Ball.tscn`, `Paddle.tscn`, `Wall.tscn`, `Goal.tscn`, and so on.

However, **Modot** allows for a different option - storing instance data in XML files, loading them as part of mod XML data, and instancing (*deserializing*) the XML data into actual objects. This is done with the help of **GDSerializer** - an XML serialization framework specifically for Godot.

This demo will not use scenes for the ball, paddles, walls, and goals, but will instead use XML serialization and deserialization - so if you have created any scenes in "Pong-Core", delete them.

> **Bug:** Using XML deserialization instead of scenes is necessary due to a [bug](https://github.com/godotengine/godot/issues/36828) in Godot related to `.pck` files and scenes with custom C# scripts. You can still create and use scenes if they do not use any nodes with custom C# scripts.

## Ball

First, the data for the ball. Create a file named `Ball.xml` and enter the following data into it:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Ball" Id="Ball">
    <Position>(320, 240)</Position>
    <initialPosition>(320, 240)</initialPosition>
    <Direction>(1, 0)</Direction>
    <Speed>200</Speed>
    <Children>
        <item Type="Godot.Sprite">
            <Name>Sprite</Name>
        </item>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.CircleShape2D">
                <Radius>15</Radius>
            </Shape>
        </item>
    </Children>
</Object>
```

## Paddles

Next comes the data for the paddles. Since there are two different paddles that each require different data (mainly up/down keys), two XML files will be needed.

For the right-side paddle, create a file named `PaddleRight.xml` and enter the following data:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Paddle" Id="PaddleRight">
    <Position>(600, 240)</Position>
    <Speed>100</Speed>
    <UpAction>Up</UpAction>
    <DownAction>Down</DownAction>
    <Children>
        <item Type="Godot.Sprite">
            <Name>Sprite</Name>
        </item>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.RectangleShape2D">
                <Extents>(8.5, 64)</Extents>
            </Shape>
        </item>
    </Children>
</Object>
```

Then for the left-side paddle, create an XML file named `PaddleLeft.xml` and enter the following data:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Paddle" Id="PaddleLeft">
    <Position>(40, 240)</Position>
    <Speed>100</Speed>
    <UpAction>W</UpAction>
    <DownAction>S</DownAction>
    <Children>
        <item Type="Godot.Sprite">
            <Name>Sprite</Name>
        </item>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.RectangleShape2D">
                <Extents>(8.5, 64)</Extents>
            </Shape>
        </item>
    </Children>
</Object>
```

## Walls

Now the walls. Like the paddles, they too will need two separate XML files.

One for the top, named `WallTop.xml`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Wall" Id="WallTop">
    <Position>(320, -15)</Position>
    <BounceDirection>(0, 1)</BounceDirection>
    <Children>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.RectangleShape2D">
                <Extents>(320, 15)</Extents>
            </Shape>
        </item>
    </Children>
</Object>
```

And one for the bottom, named `WallBottom.xml`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Wall" Id="WallBottom">
    <Position>(320, 495)</Position>
    <BounceDirection>(0, -1)</BounceDirection>
    <Children>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.RectangleShape2D">
                <Extents>(320, 15)</Extents>
            </Shape>
        </item>
    </Children>
</Object>
```

## Goals

And finally, the XML data for the goals.

Create a file named `GoalRight.xml` for the right-side goal:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Goal" Id="GoalRight">
    <Position>(655, 240)</Position>
    <Children>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.RectangleShape2D">
                <Extents>(15, 240)</Extents>
            </Shape>
        </item>
    </Children>
</Object>
```

And a file named `GoalLeft.xml` for the left-side goal:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Object Type="Pong.Goal" Id="GoalLeft">
    <Position>(-15, 240)</Position>
    <Children>
        <item Type="Godot.CollisionShape2D">
            <Shape Type="Godot.RectangleShape2D">
                <Extents>(15, 240)</Extents>
            </Shape>
        </item>
    </Children>
</Object>
```

# Working around non-serializable data

You may have noticed that the `Sprite`s for the ball and paddles have no texture assigned to them - this is because **GDSerializer**'s default `Serializer` implementation does not know how to (de)serialize a `Texture`.

This can be worked around by giving both `Ball` and `Paddle` a field for the texture path:
```cs
private string texturePath;
```

A texture from the given path can be loaded in `_Ready()` like so:
```cs
this.GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>(this.texturePath);
```

Then, for the ball, enter this line under the root XML node:
```xml
<texturePath>res://Ball.png</texturePath>
```

For the right-side paddle:
```xml
<texturePath>res://PaddleRight.png</texturePath>
```

And similarly, for the left-side paddle:
```xml
<texturePath>res://PaddleLeft.png</texturePath>
```

> **Wiki:** Read [Serialization](https://github.com/Carnagion/GDSerializer/wiki/Serialization) and [Deserialization](https://github.com/Carnagion/GDSerializer/wiki/Deserialization) in the **GDSerializer** wiki to understand more about how XML (de)serialization works, and the different ways of customising it.

# Mod startup

Now that all the code and XML data has been completed, the game needs to actually be started once the "Pong-Core" mod is loaded.

To do this, **Modot** offers yet another convenient feature - the `[ModStartup]` attribute. Any static methods annotated with `[ModStartup]` in a mod assembly will be executed after all mods have been loaded - that is, assuming that whoever loaded the mod has chosen to allow execution of mod code.

> **Wiki:** Read [Executing mod code](https://github.com/Carnagion/Modot/wiki/Executing-mod-code) in the **Modot** wiki to understand more about security, the possibility of executing malicious code, and disabling mod code execution.

Go ahead and create a new C# file named `Game.cs`. If this contains any code auto-generated by Godot, clear it, and instead edit it to look like so:
```cs
using System.Xml;

using Godot;
using Godot.Modding;
using Godot.Serialization;

namespace Pong
{
    internal static class Game
    {
    }
}
```

> **Note:** If you use an external editor such as Rider, Visual Studio, or VIsual Studio Code, create the file from within them instead of from the Godot editor - this prevents Godot from adding its auto-generated code and comments to the file.

Add a static method named `OnModStartup` to the `Game` class, and annotate it with `[ModStartup]`:
```cs
[ModStartup]
private static void OnModStartup()
{
}
```

Notice the `void` return type - methods annotated with `[ModStartup]` should not take any parameters, and should ideally return `void`.

> **Note:** It is possible for methods annotated with `[ModStartup]` to return a value, but this return value will be ignored by **Modot**. In some cases, this may cause unexpected issues, such as when executing methods that return `IEnumerable<T>`, or executing iterator methods (i.e. methods that use `yield return` or `yield break`).

Now, within the `OnModStartup()` method, the first step is to obtain a reference to the mod's own XML data:
```cs
Mod pongCore = ModLoader.LoadedMods["Pong-Core"];
XmlDocument data = pongCore.Data!;
```

When **Modot** loads a mod, it combines the data from all of its XML documents into one large XML document, which is stored in memory. This makes it easier for the data to be modified using XML patches - but the data cannot be identified by file paths anymore.

Instead, you may recall the `Id` attributes on the XML data earlier - these are intended to be unique and can assist with finding the necessary XML data:
```cs
XmlNode ballXml = data.SelectSingleNode("//*[@Id=\"Ball\"]")!;
XmlNode paddleLeftXml = data.SelectSingleNode("//*[@Id=\"PaddleLeft\"]")!;
XmlNode paddleRightXml = data.SelectSingleNode("//*[@Id=\"PaddleRight\"]")!;
XmlNode wallTopXml = data.SelectSingleNode("//*[@Id=\"WallTop\"]")!;
XmlNode wallBottomXml = data.SelectSingleNode("//*[@Id=\"WallBottom\"]")!;
XmlNode goalLeftXml = data.SelectSingleNode("//*[@Id=\"GoalLeft\"]")!;
XmlNode goalRightXml = data.SelectSingleNode("//*[@Id=\"GoalRight\"]")!;
```

> **Note:** Finding the data by ID as shown above uses XPath query expressions. In case you are unfamiliar with XPath - it is a system of finding XML nodes according to relative and absolute paths as well as certain properties, much like Godot's node paths.

These XML nodes then need to be deserialized into the actual object instances that they represent. This is done with the help of **GDSerializer**'s `Serializer` class:
```cs
Serializer serializer = new();

Ball ball = serializer.Deserialize<Ball>(ballXml)!;
Paddle paddleLeft = serializer.Deserialize<Paddle>(paddleLeftXml)!;
Paddle paddleRight = serializer.Deserialize<Paddle>(paddleRightXml)!;
Wall wallTop = serializer.Deserialize<Wall>(wallTopXml)!;
Wall wallBottom = serializer.Deserialize<Wall>(wallBottomXml)!;
Goal goalLeft = serializer.Deserialize<Goal>(goalLeftXml)!;
Goal goalRight = serializer.Deserialize<Goal>(goalRightXml)!;
```

# Accessing the scene tree

Now that the necessary nodes have been obtained, they can be added to the scene tree.

However, you may have noticed that all of this is done in a `static` method, and there is no node available that gives access to the scene tree.

Thankfully, Godot provides a way to access the scene tree even from static contexts without a node using the `Engine.GetMainLoop()` method:
```cs
SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
```

> **Note:** If you are using a custom `MainLoop` implementation, accessing the scene tree in this way is not possible. Another workaround must be used instead, such as using a static property in the main project and making all mods dependent on it.

The deserialized objects can then be added to the scene tree:
```cs
sceneTree.Root.AddChild(ball);
sceneTree.Root.AddChild(paddleLeft);
sceneTree.Root.AddChild(paddleRight);
sceneTree.Root.AddChild(wallTop);
sceneTree.Root.AddChild(wallBottom);
sceneTree.Root.AddChild(goalLeft);
sceneTree.Root.AddChild(goalRight);
```

# Exporting as a mod

The final step is to arrange the contents of the "Pong-Core" project into a mod directory, which can then be bundled together with the "Pong" project.

The "Pong-Core" project has three kinds of files that will need to be included - its C# assembly, its XML data, and its textures/assets.

First, under `res://`, create a directory named `Pong-Core`. This will serve as the "Pong-Core" project's mod directory and will be moved to another location later. Move the mod metadata file (`Mod.xml`) into this directory.

Then, create three sub-directories inside this directory - named `Assemblies`, `Data`, and `Resources`. These are the sub-directories **Modot** will search in when looking for C# assemblies, XML files, and resource packs respectively.

If you haven't done so already, compile the C# code of the "Pong-Core" project by clicking the "Build" button in the top-right of the Godot editor. This will generate a C# assembly (`.dll` file) which can be found at `res://.mono/temp/bin/Debug/Pong-Core.dll`. Copy over the assembly file into the `res://Pong-Core/Assemblies` directory.

> **Note:** If you are running the project with the `ExportDebug` or `ExportRelease` configurations instead, replace `Debug` in the above path with the respective configuration.

Next, move all of the "Pong-Core" project's XML data files into the `res://Pong-Core/Data` directory.

And then, the textures. Due to the way Godot's resource importing works, image files must be exported in a resource pack (`.pck` file). To do this, go to `Project` -> `Export`, set up an export template, and pick the "Export as PCK/Zip" option - make sure to disable the "Runnable" option when exporting. Save the export file as `Pong-Core.pck` in `res://Pong-Core/Resources`.

> **Note:** Read [Exporting packs, patches, and mods](https://docs.godotengine.org/en/stable/tutorials/export/exporting_pcks.html#generating-pck-files) in the Godot documentation for detailed instructions on exporting as a `.pck` file.

And finally, move the `res://Pong-Core` mod directory into the mods folder (`res://Mods`) of the "Pong" project. You can now close the "Pong-Core" project.

# Playing the game

You should be able to play the game by opening the "Pong" project and clicking the play button - if you haven't already set `Main.tscn` as the main scene, do so now.

It may take a few moments to load the "Pong-Core" mod and execute its code, but you should eventually see a scene with a ball and two paddles - the ball should automatically start moving.

If your goal was to simply learn how to create a game using **Modot**, then congratulations, you have created your first moddable game!

If you want to learn how to create a patch that users can add to their game, continue reading.

# Patching mods

Sometimes, mods may want to change the contents of other mods, but without having to execute any custom mod code or relying on the user to edit files. **Modot** provides an easy way to do this, using XML patches.

**Modot**'s patching system allows mods to write expressive XML patches that can change the XML data of any other mods that was loaded before them. There are many advantages to using patches like this:
- They do not require custom C# code from mods to be executed, and therefore work even when mod code execution is disabled
- They do not permanently modify the XML data, but rather modify it only in-memory
- They do not require re-compilation of source code

The following section will guide you through creating a simple mod that patches **Pong**'s ball to move towards the left initially (instead of the right).

# Setting up the patch mod

First, navigate to the "Pong" project's `user://Mods` directory (*not* `res://Mods`), and create a new directory named `Pong-DirectionPatch` there. This will serve as the mod directory.

> **Note:** The `user://` directory has different locations depending on the operating system being used. Read [File paths in Godot projects](https://docs.godotengine.org/en/stable/tutorials/io/data_paths.html) to find where the `user://Mods` directory might be.

Inside this directory, create the mod metadata file - named `Mod.xml` - and enter the following data:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Mod>
    <Id>Pong-DirectionPatch</Id>
    <Name>Pong Direction Patch</Name>
    <Author>YOUR NAME</Author>
    <After>
        <item>Pong-Core</item>
    </After>
</Mod>
```

The `<After>` XML node indicates to **Modot** that this mod must be loaded after "Pong-Core".

Next, create a sub-directory inside the mod directory, named `Patches` - this will contain all of the XML patch files. Inside this directory, create a file named `DirectionPatch.xml` and enter the following patch:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Patch Type="Godot.Modding.Patching.TargetedPatch">
    <Targets>//*[@Id="Ball"]</Targets>
    <Patch Type="Godot.Modding.Patching.MultiPatch">
        <Patches>
            <item Type="Godot.Modding.Patching.TargetedPatch">
                <Targets>Direction</Targets>
                <Patch Type="Godot.Modding.Patching.NodeRemovePatch"/>
            </item>
            <item Type="Godot.Modding.Patching.NodeAddPatch">
                <Value>
                    <Direction>(-1, 0)</Direction>
                </Value>
            </item>
        </Patches>
    </Patch>
</Patch>
```

This patch selects any XML matching the XPath `//*[@Id="Ball"]`, removes its `Direction` child node, and adds a new `Direction` node. You may recall that the original direction is (1, 0), i.e. right - this patch will replace it with (-1, 0), i.e. left.

And that's all for the patch. Now run the "Pong" project again - you should see that the ball initially moves towards the left rather than the right. Since this patch is in the `user://Mods` directory, it can be removed and added at any time without having to re-export the project.

This marks the end of the walkthrough, so if you have read all the way till here, then congratulations, you have a working game and patch!