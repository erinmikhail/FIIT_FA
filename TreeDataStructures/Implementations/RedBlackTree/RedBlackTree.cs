using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        var node = new RbNode<TKey, TValue>(key, value);
        node.Color = RbColor.Red;
        return node;
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
         RbNode<TKey, TValue>? node = newNode;
        
        while (node.Parent != null && node.Parent.Color == RbColor.Red)
        {
            if (node.Parent == node.Parent.Parent?.Left)
            {
                RbNode<TKey, TValue>? uncle = node.Parent.Parent.Right;
                
                if (uncle != null && uncle.Color == RbColor.Red)
                {
                    node.Parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    if (node.Parent.Parent != null)
                    {
                        node.Parent.Parent.Color = RbColor.Red;
                    }
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Right)
                    {

                        node = node.Parent;
                        RotateLeft(node);
                    }

                    node.Parent.Color = RbColor.Black;
                    if (node.Parent.Parent != null)
                    {
                        node.Parent.Parent!.Color = RbColor.Red;
                    }
                    RotateRight(node.Parent.Parent);
                }
            }
            else
            {
                RbNode<TKey, TValue>? uncle = node.Parent.Parent?.Left;
                
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

                    node.Parent.Color = RbColor.Black;
                    if (node.Parent.Parent != null)
                    { 
                        node.Parent.Parent!.Color = RbColor.Red;
                    }
                    RotateLeft(node.Parent.Parent);
                }
            }
        }

        Root!.Color = RbColor.Black;
    }
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
         if (child == null) return;
        
        RbNode<TKey, TValue> node = child;
        
        while (node != Root && node.Color == RbColor.Black)
        {
            if (node == node.Parent?.Left)
            {
                RbNode<TKey, TValue>? sibling = node.Parent.Right;

                if (sibling != null && sibling.Color == RbColor.Red)
                {
                    sibling.Color = RbColor.Black;
                    node.Parent.Color = RbColor.Red;
                    RotateLeft(node.Parent);
                    sibling = node.Parent.Right;
                }

                if (sibling != null)
                {
                    bool leftBlack = sibling.Left?.Color == RbColor.Black;
                    bool rightBlack = sibling.Right?.Color == RbColor.Black;
                    
                    if (leftBlack && rightBlack)
                    {
                        sibling.Color = RbColor.Red;
                        node = node.Parent;
                    }
                    else
                    {
                        if (sibling.Right?.Color == RbColor.Black)
                        {
                            if (sibling.Left != null)
                                sibling.Left.Color = RbColor.Black;
                            sibling.Color = RbColor.Red;
                            RotateRight(sibling);
                            sibling = node.Parent.Right;
                        }

                        sibling.Color = node.Parent.Color;
                        node.Parent.Color = RbColor.Black;
                        if (sibling.Right != null)
                            sibling.Right.Color = RbColor.Black;
                        RotateLeft(node.Parent);
                        node = Root!;
                    }
                }
                else
                {
                    node = node.Parent;
                }
            }
            else
            {
                RbNode<TKey, TValue>? sibling = node.Parent?.Left;

                if (sibling != null && sibling.Color == RbColor.Red)
                {
                    sibling.Color = RbColor.Black;
                    node.Parent!.Color = RbColor.Red;
                    RotateRight(node.Parent);
                    sibling = node.Parent.Left;
                }

                if (sibling != null)
                {
                    bool leftBlack = sibling.Left?.Color == RbColor.Black;
                    bool rightBlack = sibling.Right?.Color == RbColor.Black;
                    
                    if (leftBlack && rightBlack)
                    {
                        sibling.Color = RbColor.Red;
                        node = node.Parent!;
                    }
                    else
                    {
                        if (sibling.Left?.Color == RbColor.Black)
                        {
                            if (sibling.Right != null)
                                sibling.Right.Color = RbColor.Black;
                            sibling.Color = RbColor.Red;
                            RotateLeft(sibling);
                            sibling = node.Parent?.Left;
                        }

                        if (sibling != null)
                        {
                            sibling.Color = node.Parent!.Color;
                            node.Parent.Color = RbColor.Black;
                            if (sibling.Left != null)
                                sibling.Left.Color = RbColor.Black;
                            RotateRight(node.Parent);
                        }
                        node = Root!;
                    }
                }
                else
                {
                    node = node.Parent!;
                }
            }
        }
        
        node.Color = RbColor.Black;
    }
}