namespace Chickensoft.PowerUps.Tests.Fixtures;

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
