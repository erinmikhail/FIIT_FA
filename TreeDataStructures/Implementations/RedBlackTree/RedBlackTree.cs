using System;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value) { Color = RbColor.Red };
    }

    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        var node = newNode;

        while (node.Parent != null && node.Parent.Color == RbColor.Red)
        {
            if (node.Parent == node.Parent.Parent?.Left)
            {
                var uncle = node.Parent.Parent.Right;

                if (uncle != null && uncle.Color == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    node.Parent.Parent.Color = RbColor.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Right)
                    {
                        node = node.Parent;
                        RotateLeft(node);
                    }

                    node.Parent!.Color = RbColor.Black;
                    node.Parent.Parent!.Color = RbColor.Red;
                    RotateRight(node.Parent.Parent);
                }
            }
            else
            {
                var uncle = node.Parent.Parent?.Left;

                if (uncle != null && uncle.Color == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    node.Parent.Parent!.Color = RbColor.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Left)
                    {
                        node = node.Parent;
                        RotateRight(node);
                    }

                    node.Parent!.Color = RbColor.Black;
                    node.Parent.Parent!.Color = RbColor.Red;
                    RotateLeft(node.Parent.Parent);
                }
            }
        }

        Root!.Color = RbColor.Black;
    }

    protected override void OnNodeRemoved( RbNode<TKey, TValue> logicallyRemovedNode, RbNode<TKey, TValue> physicallyRemovedNode, RbNode<TKey, TValue>? replacementNode, RbNode<TKey, TValue>? replacementParent)
    {
        RbColor originalColorOfRemoved = physicallyRemovedNode.Color;
        if (logicallyRemovedNode != physicallyRemovedNode)
        {
            physicallyRemovedNode.Color = logicallyRemovedNode.Color;
        }

        if (originalColorOfRemoved == RbColor.Black)
        {
            FixupRemove(replacementNode, replacementParent);
        }
    }

    private void FixupRemove(RbNode<TKey, TValue>? x, RbNode<TKey, TValue>? xParent)
    {
        while (x != Root && (x == null || x.Color == RbColor.Black))
        {
            if (x == xParent?.Left)
            {
                var sibling = xParent.Right;

                if (sibling != null && sibling.Color == RbColor.Red)
                {
                    sibling.Color = RbColor.Black;
                    xParent!.Color = RbColor.Red;
                    RotateLeft(xParent);
                    sibling = xParent.Right;
                }

                if ((sibling?.Left == null || sibling.Left.Color == RbColor.Black) &&
                    (sibling?.Right == null || sibling.Right.Color == RbColor.Black))
                {
                    if (sibling != null) sibling.Color = RbColor.Red;
                    x = xParent;
                    xParent = x.Parent;
                }
                else
                {
                    if (sibling?.Right == null || sibling.Right.Color == RbColor.Black)
                    {
                        if (sibling?.Left != null) sibling.Left.Color = RbColor.Black;
                        if (sibling != null)
                        {
                            sibling.Color = RbColor.Red;
                            RotateRight(sibling);
                        }
                        sibling = xParent.Right;
                    }

                    if (sibling != null)
                    {
                        sibling.Color = xParent.Color;
                        if (sibling.Right != null) sibling.Right.Color = RbColor.Black;
                    }
                    xParent!.Color = RbColor.Black;
                    RotateLeft(xParent);
                    x = Root;
                }
            }
            else
            {
                var sibling = xParent?.Left;

                if (sibling != null && sibling.Color == RbColor.Red)
                {
                    sibling.Color = RbColor.Black;
                    xParent!.Color = RbColor.Red;
                    RotateRight(xParent);
                    sibling = xParent.Left;
                }

                if ((sibling?.Left == null || sibling.Left.Color == RbColor.Black) &&
                    (sibling?.Right == null || sibling.Right.Color == RbColor.Black))
                {
                    if (sibling != null) sibling.Color = RbColor.Red;
                    x = xParent;
                    xParent = x?.Parent;
                }
                else
                {
                    if (sibling?.Left == null || sibling.Left.Color == RbColor.Black)
                    {
                        if (sibling?.Right != null) sibling.Right.Color = RbColor.Black;
                        if (sibling != null)
                        {
                            sibling.Color = RbColor.Red;
                            RotateLeft(sibling);
                        }
                        sibling = xParent?.Left;
                    }

                    if (sibling != null)
                    {
                        sibling.Color = xParent!.Color;
                        if (sibling.Left != null) sibling.Left.Color = RbColor.Black;
                    }
                    if (xParent != null) xParent.Color = RbColor.Black;
                    RotateRight(xParent!);
                    x = Root;
                }
            }
        }

        if (x != null)
        {
            x.Color = RbColor.Black;
        }
    }
}