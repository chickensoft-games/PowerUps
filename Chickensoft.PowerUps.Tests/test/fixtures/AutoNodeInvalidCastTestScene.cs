namespace Chickensoft.PowerUps.Tests.Fixtures;

using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoNode))]
public partial class AutoNodeInvalidCastTestScene : Node2D {
  public override partial void _Notification(int what);

  [Node("Node3D")]
  public Node2D Node { get; set; } = default!;
}
