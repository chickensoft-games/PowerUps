# 🔋 PowerUps

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage]

 <!-- ![line coverage][line-coverage] ![branch coverage][branch-coverage] -->

A collection of power-ups for your C# Godot game scripts that work with the [SuperNodes] source generator.

---

<p align="center">
<img alt="Chickensoft.PowerUps" src="Chickensoft.PowerUps/icon.png" width="200">
</p>

Currently, two PowerUps are provided by this package: `AutoNode` and `AutoDispose`.

- 🌲 AutoNode: automatically connect fields and properties to their corresponding nodes in the scene tree.
- 🚮 AutoDispose: automatically dispose of disposable properties owned by your script when it exits the scene tree.

> Chickensoft also maintains a third PowerUp for dependency injection called `AutoInject` that resides in its own [AutoInject] repository.

## 📦 Installation

Unlike most nuget packages, PowerUps are provided as source-only nuget packages that actually inject the source code for the PowerUp into your project.

> Injecting the code directly into the project referencing the PowerUp allows the [SuperNodes] source generator to see the code and generate the glue needed to make everything work without reflection.

To use the PowerUps, add the following to your `.csproj` file. Be sure to get the latest versions for each package on [Nuget]. Note that the `AutoNode` PowerUp requires the [GodotNodeInterfaces] package so that you can access Godot nodes by interface, rather than the concrete type, which facilitates unit testing.

```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.SuperNodes" Version="1.6.0" PrivateAssets="all" OutputItemType="analyzer" />
    <PackageReference Include="Chickensoft.SuperNodes.Types" Version="1.6.0" />
    <PackageReference Include="Chickensoft.PowerUps" Version="1.1.0" PrivateAssets="all" />
    <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="1.1.0-godot4.2.0-beta.1" />
    <!-- ^ Or whatever the latest versions are. -->
</ItemGroup>
```

## 🌲 AutoNode

The `AutoNode` PowerUp automatically connects fields and properties in your script to a declared node path or unique node name in the scene tree whenever the scene is instantiated.

Simply apply the `[Node]` attribute to any field or property in your script that you want to automatically connect to a node in your scene.

If you don't specify a node path, the name of the field or property will be converted to a [unique node identifier][unique-nodes] name in PascalCase. For best results, consider using PascalCase for node names in the scene tree.

```csharp
using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoNode))]
public partial class MyNode : Node2D {
  public override partial void _Notification(int what);

  // Automatically connect this field to the provided node path
  [Node("Path/To/MyNode")]
  private Node2D _myNode = default!;

  // If you don't specify a node path, AutoNode converts the name of the 
  // field to a unique node path specifier in PascalCase: %MyUniqueNode
  [Node]
  private Node2D _myUniqueNode = default!;

  // Just specify the unique name if it differs in more than just casing from
  // the field/property name.
  [Node("%OtherUniqueName")]
  private Node2D _differentName = default!;
}
```

## 🚮 AutoDispose

The `AutoDispose` PowerUp will automatically dispose of writeable properties on your script that implement `IDisposable` but do not inherit `Godot.Node` when the node exits the scene tree, preventing the need for manually cleaning up disposable, non-node objects that your script owns.

AutoDispose was designed to work nicely with the dependency injection system from [AutoInject] and disposable objects, like the bindings from [LogicBlocks].

```csharp
using Chickensoft.AutoInject;
using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

// Note that Dependent is a PowerUp provided by AutoInject.

[SuperNode(typeof(Dependent), typeof(AutoDispose))]
public partial class MyNode : Node2D {
  public override partial void _Notification(int what);

  // Use AutoInject to lookup the nearest SomeDisposable object provided above
  // us in the scene tree.
  //
  // Since this is a read-only property, it will not have Dispose called on it
  // by AutoDispose when we exit the scene tree. This is desirable since this
  // script doesn't own this object — it just needs to use it.
  [Dependency]
  public SomeDisposable DisposableDependency => DependOn<SomeDisposable>();

  // This object will automatically have Dispose called on it by AutoDispose 
  // when we exit the scene tree since it is a writeable property (which tells
  // AutoDispose this script owns it and should dispose of it).
  public MyDisposable MyDisposableObject { get; set; } = default!;

  // Even though Godot nodes are disposable, this won't be disposed since it
  // inherits from Godot.Node. Godot manages node references automatically.
  public Node2D OtherNode { get; set; } = default!;

  // ...
```

---

🐣 Package generated from a 🐤 Chickensoft Template — <https://chickensoft.games>

[chickensoft-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/discord_badge.svg
[discord]: https://discord.gg/gSjaPgMmYW
[read-the-docs-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/read_the_docs_badge.svg
[docs]: https://chickensoft.games/docsickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white
[line-coverage]: Chickensoft.PowerUps.Tests/badges/line_coverage.svg
<!-- [branch-coverage]: Chickensoft.PowerUps.Tests/badges/branch_coverage.svg -->

[SuperNodes]: https://github.com/chickensoft-games/SuperNodes
[AutoInject]: https://github.com/chickensoft-games/AutoInject
[LogicBlocks]: https://github.com/chickensoft-games/LogicBlocks
[Nuget]: https://www.nuget.org/packages?q=Chickensoft
[unique-nodes]: https://docs.godotengine.org/en/stable/tutorials/scripting/scene_unique_nodes.html

[GodotNodeInterfaces]: https://github.com/chickensoft-games/GodotNodeInterfaces
