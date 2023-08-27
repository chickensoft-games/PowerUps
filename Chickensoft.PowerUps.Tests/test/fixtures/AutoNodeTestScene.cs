namespace Chickensoft.PowerUps.Tests.Fixtures;

using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoNode))]
public partial class AutoNodeTestScene : Node2D {
  public override partial void _Notification(int what);

  [Node("Path/To/MyNode")]
  public Node2D MyNode { get; set; } = default!;

  [Node]
  public Node2D MyUniqueNode { get; set; } = default!;

  [Node("%OtherUniqueName")]
  public Node2D DifferentName { get; set; } = default!;

  [Node]
  internal Node2D _my_unique_node = default!;

  [Other]
  public Node2D SomeOtherNodeReference = default!;
}
