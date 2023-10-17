namespace Chickensoft.PowerUps;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Chickensoft.GodotNodeInterfaces;
using Godot;

public class FakeNodeTree {
  // Map of node paths to FakeSceneTreeNode instances.
  private readonly OrderedDictionary _nodes;

  private readonly Node _parent;

  private int _nextId;

  public FakeNodeTree(
    Node parent,
    Dictionary<string, INode>? nodes = null
  ) {
    _parent = parent;
    _nodes = new();

    if (nodes is Dictionary<string, INode> dict) {
      foreach (var (path, node) in dict) {
        _nodes.Add(path, node);
      }
    }
  }

  public void AddChild(INode node) {
    var name = "";
    // We use try/catch to check node name since not all node mocks may
    // have stubbed the Name property.
    try {
      name = node.Name;
    }
    catch { }

    if (string.IsNullOrEmpty(name)) {
      name = node.GetType().Name + "@" + _nextId++;
    }

    _nodes.Add(name, node);
  }

  public INode GetNode(string path) =>
   !_nodes.Contains(path)
      ? throw new KeyNotFoundException(
        $"Node '{path}' not found in {_parent.Name}'s FakeNodeTree."
      )
      : (INode)_nodes[path]!;

  public INode? FindChild(string pattern) {
    foreach (string path in _nodes.Keys) {
      var node = (INode)_nodes[path]!;
      var name = "";
      // We use try/catch to check node name since not all node mocks may
      // have stubbed the Name property.
      try {
        name = node.Name;
      }
      catch { }

      if (!string.IsNullOrEmpty(name) && name.Match(pattern)) {
        return node;
      }
    }

    return null;
  }

  public bool HasNode(NodePath path) => _nodes.Contains((string)path);

  public INode[] FindChildren(string pattern) {
    var children = new List<INode>();

    foreach (string path in _nodes.Keys) {
      var node = (INode)_nodes[path]!;
      var name = "";
      try {
        name = node.Name;
      }
      catch { }

      if (!string.IsNullOrEmpty(name) && name.Match(pattern)) {
        children.Add((INode)_nodes[path]!);
      }
    }

    return children.ToArray();
  }

  public T GetChild<T>(int index) where T : class, INode {
    var actualIndex = index;
    if (actualIndex < 0) {
      // Negative indices access the children from the last one.
      actualIndex = _nodes.Count + actualIndex;
    }
    return (T)_nodes[actualIndex]!;
  }

  public int GetChildCount() => _nodes.Count;

  public INode[] GetChildren() => _nodes.Values.Cast<INode>().ToArray();

  public void RemoveChild(INode node) {
    var path = _nodes
      .Keys
      .Cast<string>()
      .First(k => _nodes[k] == node);
    _nodes.Remove(path);
  }

  public Dictionary<string, INode> GetAllNodes() {
    var nodes = new Dictionary<string, INode>();

    foreach (string path in _nodes.Keys) {
      var node = (INode)_nodes[path]!;
      nodes.Add(path, node);
    }

    return nodes;
  }
}
