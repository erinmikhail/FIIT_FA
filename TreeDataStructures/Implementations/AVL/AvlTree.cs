using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        AvlNode<TKey, TValue>? current = newNode;
        
        while (current != null)
        {
            // Обновление высоты узла
            int leftHeight = current.Left?.Height ?? 0;
            int rightHeight = current.Right?.Height ?? 0;
            current.Height = 1 + Math.Max(leftHeight, rightHeight);

            int balance = leftHeight - rightHeight;

            if (balance > 1)
            {
                int leftBalance = (current.Left?.Left?.Height ?? 0) - (current.Left?.Right?.Height ?? 0);
                if (leftBalance < 0)
                {
                    RotateLeft(current.Left!);
                }
                RotateRight(current);

                if (current.Parent?.Left != null)
                {
                    leftHeight = current.Parent.Left.Left?.Height ?? 0;
                    rightHeight = current.Parent.Left.Right?.Height ?? 0;
                    current.Parent.Left.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent?.Right != null)
                {
                    leftHeight = current.Parent.Right.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right.Right?.Height ?? 0;
                    current.Parent.Right.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent != null)
                {
                    leftHeight = current.Parent.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right?.Height ?? 0;
                    current.Parent.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
            }
            else if (balance < -1)
            {
                int rightBalance = (current.Right?.Left?.Height ?? 0) - (current.Right?.Right?.Height ?? 0);
                if (rightBalance > 0)
                {
                    RotateRight(current.Right!);
                }
                RotateLeft(current);

                if (current.Parent?.Left != null)
                {
                    leftHeight = current.Parent.Left.Left?.Height ?? 0;
                    rightHeight = current.Parent.Left.Right?.Height ?? 0;
                    current.Parent.Left.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent?.Right != null)
                {
                    leftHeight = current.Parent.Right.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right.Right?.Height ?? 0;
                    current.Parent.Right.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent != null)
                {
                    leftHeight = current.Parent.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right?.Height ?? 0;
                    current.Parent.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
            }
            
            current = current.Parent;
        }
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        AvlNode<TKey, TValue>? current = parent;
        
        while (current != null)
        {
            int leftHeight = current.Left?.Height ?? 0;
            int rightHeight = current.Right?.Height ?? 0;
            current.Height = 1 + Math.Max(leftHeight, rightHeight);

            int balance = leftHeight - rightHeight;

            if (balance > 1)
            {
                int leftBalance = (current.Left?.Left?.Height ?? 0) - (current.Left?.Right?.Height ?? 0);
                if (leftBalance < 0)
                {
                    RotateLeft(current.Left!);
                }
                RotateRight(current);

                if (current.Parent?.Left != null)
                {
                    leftHeight = current.Parent.Left.Left?.Height ?? 0;
                    rightHeight = current.Parent.Left.Right?.Height ?? 0;
                    current.Parent.Left.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent?.Right != null)
                {
                    leftHeight = current.Parent.Right.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right.Right?.Height ?? 0;
                    current.Parent.Right.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent != null)
                {
                    leftHeight = current.Parent.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right?.Height ?? 0;
                    current.Parent.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
            }
            else if (balance < -1)
            {
                int rightBalance = (current.Right?.Left?.Height ?? 0) - (current.Right?.Right?.Height ?? 0);
                if (rightBalance > 0)
                {
                    RotateRight(current.Right!);
                }
                RotateLeft(current);

                if (current.Parent?.Left != null)
                {
                    leftHeight = current.Parent.Left.Left?.Height ?? 0;
                    rightHeight = current.Parent.Left.Right?.Height ?? 0;
                    current.Parent.Left.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent?.Right != null)
                {
                    leftHeight = current.Parent.Right.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right.Right?.Height ?? 0;
                    current.Parent.Right.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
                if (current.Parent != null)
                {
                    leftHeight = current.Parent.Left?.Height ?? 0;
                    rightHeight = current.Parent.Right?.Height ?? 0;
                    current.Parent.Height = 1 + Math.Max(leftHeight, rightHeight);
                }
            }
            
            current = current.Parent;
        }
    }
}