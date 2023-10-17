namespace Chickensoft.PowerUps.Tests.Fixtures;

using Chickensoft.GodotNodeInterfaces;
using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoNode))]
public partial class AutoNodeTestScene : Node2D {
  public override partial void _Notification(int what);

  [Node("Path/To/MyNode")]
  public INode2D MyNode { get; set; } = default!;

  [Node]
  public INode2D MyUniqueNode { get; set; } = default!;

  [Node("%OtherUniqueName")]
  public INode2D DifferentName { get; set; } = default!;

  [Node]
  internal INode2D _my_unique_node = default!;

  [Other]
  public INode2D SomeOtherNodeReference = default!;
}
