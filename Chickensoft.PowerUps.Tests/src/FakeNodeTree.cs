namespace Chickensoft.PowerUps;

using System.Collections.Specialized;
using Chickensoft.GodotNodeInterfaces;
using Godot;

public class FakeNodeTree {
  // Map of node paths to FakeSceneTreeNode instances.
  private readonly OrderedDictionary _fakeSceneTree;

  public FakeNodeTree(
    System.Collections.Generic.Dictionary<string, INode>? nodes = null
  ) {
    _fakeSceneTree = new();

    if (nodes is System.Collections.Generic.Dictionary<string, INode> dict) {
      foreach (var (path, node) in dict) {
        _fakeSceneTree.Add(path, node);
      }
    }
  }

  public void AddChild(
    string name, INode node
  ) => _fakeSceneTree.Add(name, node);

  public INode GetNode(string path, Node caller) =>
   !_fakeSceneTree.Contains(path)
      ? throw new System.Collections.Generic.KeyNotFoundException(
        $"Node '{path}' not found in {caller.Name}'s FakeNodeTree."
      )
      : (INode)_fakeSceneTree[path]!;

  public INode FindParent(string pattern) => null!;
}
