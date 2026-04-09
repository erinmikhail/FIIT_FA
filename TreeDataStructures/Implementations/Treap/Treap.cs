using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }

        if (Comparer.Compare(root.Key, key) <= 0)
        {
            var (left, right) = Split(root.Right, key);
            root.Right = left;
            if (left != null) left.Parent = root;
            if (right != null) right.Parent = null;
            return (root, right);
        }
        else
        {
            var (left, right) = Split(root.Left, key);
            root.Left = right;
            if (right != null) right.Parent = root;
            if (left != null) left.Parent = null;
            return (left, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if (left.Right != null) left.Right.Parent = left;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            if (right.Left != null) right.Left.Parent = right;
            return right;
        }
    }

    public override void Add(TKey key, TValue value)
    {
        var existingNode = FindNode(key);
        if (existingNode != null)
        {
            existingNode.Value = value;
            return;
        }

        var (left, right) = Split(Root, key);
        
        var newNode = CreateNode(key, value);

        var temp = Merge(left, newNode);
        Root = Merge(temp, right);
        
        if (Root != null) Root.Parent = null;
        
        Count++;
    }

    public override bool Remove(TKey key)
    {
        var node = FindNode(key);
        if (node == null) return false;

        var mergedChildren = Merge(node.Left, node.Right);

        Transplant(node, mergedChildren); 
        
        Count--;
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }

    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode) { }
    protected override void OnNodeRemoved(TreapNode<TKey, TValue> logicallyRemovedNode, TreapNode<TKey, TValue> physicallyRemovedNode, TreapNode<TKey, TValue>? replacementNode, TreapNode<TKey, TValue>? replacementParent) { }
}