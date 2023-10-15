namespace Chickensoft.PowerUps;

using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Godot;
using SuperNodes.Types;
using Chickensoft.GodotNodeInterfaces;

#pragma warning disable CS8019
using Chickensoft.PowerUps;
using Godot.Collections;
#pragma warning restore CS8019

/// <summary>
/// Node attribute. Apply this to properties or fields that need to be
/// automatically connected to a corresponding node instance in the scene tree.
/// </summary>
[AttributeUsage(
  AttributeTargets.Field | AttributeTargets.Property,
  AllowMultiple = false
)]
public class NodeAttribute : Attribute {
  /// <summary>
  /// Explicit node path or unique identifier that the tagged property or field
  /// should reference. If not provided (or null), the name of the property or
  /// field itself will be converted to PascalCase (with any leading
  /// underscores removed) and used as a unique node identifier. For example,
  /// the reference `Node2D _myNode` would be connected to `%MyNode`.
  /// </summary>
  public string? Path { get; }

  public NodeAttribute(string? path = null) {
    Path = path;
  }

  public const string ID = "global::Chickensoft.PowerUps.NodeAttribute";
}

/// <summary>
/// Interface for a PowerUp that automatically connects declared node
/// references to their corresponding instances in the scene tree.
/// </summary>
public interface IAutoNode : ISuperNode {
  /// <summary>
  /// <para>Adds a child <paramref name="node" />. Nodes can have any number of children, but every child must have a unique name. Child nodes are automatically deleted when the parent node is deleted, so an entire scene can be removed by deleting its topmost node.</para>
  /// <para>If <paramref name="forceReadableName" /> is <c>true</c>, improves the readability of the added <paramref name="node" />. If not named, the <paramref name="node" /> is renamed to its type, and if it shares <see cref="Node.Name" /> with a sibling, a number is suffixed more appropriately. This operation is very slow. As such, it is recommended leaving this to <c>false</c>, which assigns a dummy name featuring <c>@</c> in both situations.</para>
  /// <para>If <paramref name="internal" /> is different than <see cref="Node.InternalMode.Disabled" />, the child will be added as internal node. Such nodes are ignored by methods like <see cref="Node.GetChildren(System.Boolean)" />, unless their parameter <c>include_internal</c> is <c>true</c>. The intended usage is to hide the internal nodes from the user, so the user won't accidentally delete or modify them. Used by some GUI nodes, e.g. <see cref="ColorPicker" />. See <see cref="Node.InternalMode" /> for available modes.</para>
  /// <para><b>Note:</b> If the child node already has a parent, the function will fail. Use <see cref="Node.RemoveChild(Node)" /> first to remove the node from its current parent. For example:</para>
  /// <para><code>
  /// Node childNode = GetChild(0);
  /// if (childNode.GetParent() != null)
  /// {
  /// childNode.GetParent().RemoveChild(childNode);
  /// }
  /// AddChild(childNode);
  /// </code></para>
  /// <para>If you need the child node to be added below a specific node in the list of children, use <see cref="Node.AddSibling(Node,System.Boolean)" /> instead of this method.</para>
  /// <para><b>Note:</b> If you want a child to be persisted to a <see cref="PackedScene" />, you must set <see cref="Node.Owner" /> in addition to calling <see cref="Node.AddChild(Node,System.Boolean,Node.InternalMode)" />. This is typically relevant for <a href="$DOCS_URL/tutorials/plugins/running_code_in_the_editor.html">tool scripts</a> and <a href="$DOCS_URL/tutorials/plugins/editor/index.html">editor plugins</a>. If <see cref="Node.AddChild(Node,System.Boolean,Node.InternalMode)" /> is called without setting <see cref="Node.Owner" />, the newly added <see cref="Node" /> will not be visible in the scene tree, though it will be visible in the 2D/3D view.</para>
  /// <para>This implementation is provided by AutoNode. Because it uses object as the type of the node, it will automatically be used instead when mixed-in with SuperNodes since that's how C# resolves things.</para>
  /// </summary>
  /// <param name="node">The node to add as a child.</param>
  /// <param name="forceReadableName">If <c>true</c>, improves the readability of the added <paramref name="node" />.</param>
  /// <param name="internal">If different than <see cref="Node.InternalMode.Disabled" />, the child will be added as internal node.</param>
  void AddChild(
    object node, bool forceReadableName = false, Node.InternalMode @internal = Node.InternalMode.Disabled
  );

  /// <summary>
  /// <para>Fetches a node. The <see cref="NodePath" /> can be either a relative path (from the current node) or an absolute path (in the scene tree) to a node. If the path does not exist, <c>null</c> is returned and an error is logged. Attempts to access methods on the return value will result in an "Attempt to call &lt;method&gt; on a null instance." error.</para>
  /// <para><b>Note:</b> Fetching absolute paths only works when the node is inside the scene tree (see <see cref="Node.IsInsideTree" />).</para>
  /// <para><b>Example:</b> Assume your current node is Character and the following tree:</para>
  /// <para><code>
  /// /root
  /// /root/Character
  /// /root/Character/Sword
  /// /root/Character/Backpack/Dagger
  /// /root/MyGame
  /// /root/Swamp/Alligator
  /// /root/Swamp/Mosquito
  /// /root/Swamp/Goblin
  /// </code></para>
  /// <para>Possible paths are:</para>
  /// <para><code>
  /// GetNode("Sword");
  /// GetNode("Backpack/Dagger");
  /// GetNode("../Swamp/Alligator");
  /// GetNode("/root/MyGame");
  /// </code></para>
  /// </summary>
  /// <param name="path">The path to the node to return.</param>
  INode GetNode(NodePath path);
}

/// <summary>
/// Apply this PowerUp to your SuperNode to automatically connect declared node
/// references to their corresponding instances in the scene tree.
/// </summary>
[PowerUp]
public abstract partial class AutoNode : Node, IAutoNode {
  private FakeNodeTree? _fakeNodeTree;

  /// <summary>
  /// Initialize the fake node tree for unit testing.
  /// </summary>
  /// <param name="nodes">Map of node paths to mock nodes.</param>
  public void FakeNodeTree(
    System.Collections.Generic.Dictionary<string, INode>? nodes
  ) {
    _fakeNodeTree = new FakeNodeTree(nodes);

    Node child = new Node3D();
    AddChild(child);
  }

  public void AddChild(
    object node, bool forceReadableName = false, InternalMode @internal = InternalMode.Disabled
  ) {
    if (node is IGodotNodeAdapter adapter) {
      // If it's an adapter, we can add the underlying node directly.
      base.AddChild(adapter.OriginalNode, forceReadableName, @internal);
      return;
    }

    if (node is Node godotNode) {
      // If it's a Godot node, we can add it directly.
      base.AddChild(godotNode, forceReadableName, @internal);
      return;
    }

    if (node is INode iNode) {
      // We can only add nodes by interface only when we are in a test
      // environment, so check to see if that's been setup.
      if (_fakeNodeTree is not FakeNodeTree fakeNodeTree) {
        throw new InvalidOperationException(
          "Fake node tree has not been initialized. If you are attempting to " +
          "unit test a node scene, make sure that you have called " +
          "node.FakeNodeTree() to initialize the fake node tree."
        );
      }

      // We are running in a test environment.
      fakeNodeTree.AddChild(iNode.Name, iNode);
      return;
    }

    throw new InvalidOperationException(
      $"Cannot add child of type {node.GetType()} to {GetType()}."
    );
  }

  public new INode GetNode(NodePath path) {
    if (_fakeNodeTree is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.GetNode(path, this);
    }
    var node = base.GetNode(path);
    return GodotNodes.Adapt(node);
  }

  #region ISuperNode

  // These don't need to be copied over since we will be copied into an
  // ISuperNode.

  [PowerUpIgnore]
  public ImmutableDictionary<string, ScriptPropertyOrField> PropertiesAndFields
      => throw new NotImplementedException();
  [PowerUpIgnore]
  public TResult GetScriptPropertyOrFieldType<TResult>(
      string scriptProperty, ITypeReceiver<TResult> receiver
    ) => throw new NotImplementedException();
  [PowerUpIgnore]
  public dynamic GetScriptPropertyOrField(string scriptProperty) =>
      throw new NotImplementedException();
  [PowerUpIgnore]
  public void SetScriptPropertyOrField(string scriptProperty, dynamic? value) =>
      throw new NotImplementedException();

  #endregion ISuperNode

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void OnAutoNode(int what) {
    if (what == NotificationSceneInstantiated) {
      AutoNodeConnector.ConnectNodes(PropertiesAndFields, this);
    }
  }
}

public static class AutoNodeConnector {
  public class NodeChecker : ITypeReceiver<bool> {
    public object Node { get; set; } = default!;

    public bool Receive<T>() => Node is T;
  }

  [ThreadStatic]
  private static NodeChecker? _checker;

  public static void ConnectNodes(
      ImmutableDictionary<string, ScriptPropertyOrField> propertiesAndFields,
      IAutoNode superNode
    ) {
    var superNodeNode = (Node)superNode;
    foreach (var (name, propertyOrField) in propertiesAndFields) {
      if (
        !propertyOrField.Attributes.TryGetValue(
          NodeAttribute.ID, out var nodeAttributes
        )
      ) {
        continue;
      }
      var nodeAttribute = nodeAttributes[0];

      var path = nodeAttribute.ArgumentExpressions[0] as string ??
        AsciiToPascalCase(name);

      var node = superNode.GetNode(path) ?? throw new InvalidOperationException(
          $"Node {path} does not exist in scene tree for property {name} of " +
          $"type {propertyOrField.Type} on {superNodeNode.Name}."
        );

      // Apparently the correct way to initialize a thread-static field.
      _checker ??= new NodeChecker();

      // See if the node is the expected type.
      _checker.Node = node;
      var isValidAssignment =
        superNode.GetScriptPropertyOrFieldType(name, _checker);

      if (!isValidAssignment) {
        throw new InvalidCastException(
          $"Cannot assign node of type {node.GetType()} to property " +
          $"{name} of type {propertyOrField.Type} on {superNodeNode.Name}."
        );
      }

      superNode.SetScriptPropertyOrField(name, node);
    }
  }

  /// <summary>
  /// <para>
  /// Converts an ASCII string to PascalCase. This looks insane, but it is the
  /// fastest out of all the benchmarks I did.
  /// </para>
  /// <para>
  /// Since messing with strings can be slow and looking up nodes is a common
  /// operation, this is a good place to optimize. No heap allocations!
  /// </para>
  /// <para>
  /// Removes underscores, always capitalizes the first letter, and capitalizes
  /// the first letter after an underscore.
  /// </para>
  /// </summary>
  /// <param name="input">Input string.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string AsciiToPascalCase(string input) {
    var span = input.AsSpan();
    Span<char> output = stackalloc char[span.Length];
    var outputIndex = 0;

    for (var i = 0; i < span.Length; i++) {
      var c = span[i];

      if (c == '_') { continue; }

      output[outputIndex++] = i == 0 || span[i - 1] == '_'
        ? (char)(c & 0xDF)
        : c;
    }

    return new string(output[..outputIndex]);
  }
}
