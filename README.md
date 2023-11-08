# 🔋 PowerUps

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage]

A collection of power-ups for your C# Godot game scripts that work with the [SuperNodes] source generator.

---

<p align="center">
<img alt="Chickensoft.PowerUps" src="Chickensoft.PowerUps/icon.png" width="200">
</p>

Currently, two PowerUps are provided by this package: `AutoNode` and `AutoSetup`.

- 🌲 AutoNode: automatically connect fields and properties to their corresponding nodes in the scene tree — also provides access to nodes via their interfaces using [GodotNodeInterfaces].
- 🛠 AutoSetup: provides a mechanism for late, two-phase initialization in Godot node scripts to facilitate unit-testing.

> Chickensoft also maintains a third PowerUp for dependency injection called `AutoInject` that resides in its own [AutoInject] repository.

## 📦 Installation

Unlike most nuget packages, PowerUps are provided as source-only nuget packages that actually inject the source code for the PowerUp into your project.

> Injecting the code directly into the project referencing the PowerUp allows the [SuperNodes] source generator to see the code and generate the glue needed to make everything work without reflection.

To use the PowerUps, add the following to your `.csproj` file. Be sure to get the latest versions for each package on [Nuget]. Note that the `AutoNode` PowerUp requires the [GodotNodeInterfaces] package so that you can access Godot nodes by interface, rather than the concrete type, which facilitates unit testing.

```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.SuperNodes" Version="1.6.1" PrivateAssets="all" OutputItemType="analyzer" />
    <PackageReference Include="Chickensoft.SuperNodes.Types" Version="1.6.1" />
    <PackageReference Include="Chickensoft.PowerUps" Version="2.2.0-godot4.2.0-beta.5" PrivateAssets="all" />
    <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="2.0.0-godot4.2.0-beta.5 " />
    <!-- ^ Or whatever the latest versions are. -->
</ItemGroup>
```

## 🌲 AutoNode

The `AutoNode` PowerUp automatically connects fields and properties in your script to a declared node path or unique node name in the scene tree whenever the scene is instantiated, without reflection. It can also be used to connect nodes as interfaces, instead of concrete node types.

Simply apply the `[Node]` attribute to any field or property in your script that you want to automatically connect to a node in your scene.

If you don't specify a node path in the `[Node]` attribute, the name of the field or property will be converted to a [unique node identifier][unique-nodes] name in PascalCase. For example, the field name below `_my_unique_node` is converted to the unique node path name `%MyUniqueNode` by converting the property name to PascalCase and prefixing the percent sign indicator. Likewise, the property name `MyUniqueNode` is converted to `%MyUniqueNode`, which isn't much of a conversion since the property name is already in PascalCase.

For best results, use PascalCase for your node names in the scene tree (which Godot tends to do by default, anyways).

In the example below, we're using [GodotNodeInterfaces] to reference nodes as their interfaces instead of their concrete Godot types. This allows us to write a unit test where we fake the nodes in the scene tree by substituting mock nodes, allowing us to test a single node script at a time without polluting our test coverage.

```csharp
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoNode))]
public partial class MyNode : Node2D {
  public override partial void _Notification(int what);

  [Node("Path/To/SomeNode")]
  public INode2D SomeNode { get; set; } = default!;

  [Node] // Connects to "%MyUniqueNode" since no path was specified.
  public INode2D MyUniqueNode { get; set; } = default!;

  [Node("%OtherUniqueName")]
  public INode2D DifferentName { get; set; } = default!;

  [Node] // Connects to "%MyUniqueNode" since no path was specified.
  internal INode2D _my_unique_node = default!;
}
```

### 🧪 Testing

We can easily write a test for the example above by substituting mock nodes:

```csharp
using System.Threading.Tasks;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using GodotTestDriver;
using Moq;
using Shouldly;

public class MyNodeTest : TestClass {
  private Fixture _fixture = default!;
  private MyNode _scene = default!;

  private Mock<INode2D> _someNode = default!;
  private Mock<INode2D> _myUniqueNode = default!;
  private Mock<INode2D> _otherUniqueNode = default!;

  public MyNodeTest(Node testScene) : base(testScene) { }

  [Setup]
  public async Task Setup() {
    _fixture = new(TestScene.GetTree());

    _someNode = new();
    _myUniqueNode = new();
    _otherUniqueNode = new();

    _scene = new MyNode();
    _scene.FakeNodeTree(new() {
      ["Path/To/SomeNode"] = _someNode.Object,
      ["%MyUniqueNode"] = _myUniqueNode.Object,
      ["%OtherUniqueName"] = _otherUniqueNode.Object,
    });

    await _fixture.AddToRoot(_scene);
  }

  [Cleanup]
  public async Task Cleanup() => await _fixture.Cleanup();

  [Test]
  public void UsesFakeNodeTree() {
    // Making a new instance of a node without instantiating a scene doesn't
    // trigger NotificationSceneInstantiated, so if we want to make sure our
    // AutoNodes get hooked up and use the FakeNodeTree, we need to do it manually.
    _scene._Notification((int)Node.NotificationSceneInstantiated);

    _scene.SomeNode.ShouldBe(_someNode.Object);
    _scene.MyUniqueNode.ShouldBe(_myUniqueNode.Object);
    _scene.DifferentName.ShouldBe(_otherUniqueNode.Object);
    _scene._my_unique_node.ShouldBe(_myUniqueNode.Object);
  }
}
```

## 🛠 AutoSetup

The `AutoSetup` will conditionally call the `void Setup()` method your node script has if from `_Ready` if (and only if) the `IsTesting` field it adds to your node is false. Conditionally calling a setup method allows you to split your node's late member initialization into two-phases, allowing nodes to be unit tested. If writing tests for your node, simply initialize any members that would need to be mocked in a test in your `Setup()` method.

```csharp
using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoSetup))]
public partial class MyNode : Node2D {
  public override partial void _Notification(int what);

  public MyObject Obj { get; set; } = default!;

  public void Setup() {
    // Setup is called from the Ready notification if our IsTesting property
    // (added by AutoSetup) is false.

    // Initialize values which would be mocked in a unit testing method.
    Obj = new MyObject();
  }

  public void OnReady() {
    // Guaranteed to be called after Setup()

    // Use object we setup in Setup() method (or, if we're running in a unit 
    // test, this will use whatever the test supplied)
    Obj.DoSomething();
  }
}
```

> 💡 [AutoInject] provides this functionality out-of-the-box for nodes that also need late, two-phase initialization. It also supplies an `IsTesting` property but will call the `Setup()` method after dependencies have been resolved (but before `OnResolved() is called`). If you're using AutoInject, note that you can either use the `AutoSetup` or `Dependent` PowerUp on a node script, but not both.

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
[Nuget]: https://www.nuget.org/packages?q=Chickensoft
[unique-nodes]: https://docs.godotengine.org/en/stable/tutorials/scripting/scene_unique_nodes.html

[GodotNodeInterfaces]: https://github.com/chickensoft-games/GodotNodeInterfaces
