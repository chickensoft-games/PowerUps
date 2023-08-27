namespace Chickensoft.PowerUps.Tests.Fixtures;

using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoNode))]
public partial class AutoNodeMissingTestScene : Node2D {
  public override partial void _Notification(int what);

  [Node("NonExistentNode")]
  public Node2D MyNode { get; set; } = default!;
}
