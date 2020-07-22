namespace DawnlikeProcessor
{
    using System.Collections.Generic;
    using System.Linq;

    public class Packer
    {
        public class Node<T>
        {
            public readonly T data;
            internal int x = -1,
                         y = -1;
            public int w,
                       h;
            internal bool used;
            public Point fit = new Point(-1, -1);
            internal Node<T> down;
            internal Node<T> right;

            public Node() { }

            public Node(T data, int w, int h)
            {
                this.data = data;
                this.w = w;
                this.h = h;
            }

            internal Node(int x, int y, int w, int h)
            {
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
            }
        }

        private object root;

        public void Fit<T>(IEnumerable<Node<T>> blocks)
        {
            var enumerable = blocks as Node<T>[] ?? blocks.ToArray();
            if (!enumerable.Any()) return;
            var w = enumerable.First().w;
            var h = enumerable.First().h;
            this.root = new Node<T>(0, 0, w, h);
            foreach (var block in enumerable)
            {
                Node<T> node;
                var f = (node = this.FindNode((Node<T>)this.root, block.w, block.h)) != null ? SplitNode(node, block.w, block.h) : this.GrowNode<T>(block.w, block.h);
                block.fit = new Point(f.x, f.y);
            }
        }

        private Node<T> FindNode<T>(Node<T> r, int w, int h)
        {
            while (true)
            {
                if (r.used)
                {
                    var right = this.FindNode(r.right, w, h);
                    if (right != null) return right;
                    r = r.down;
                    continue;
                }

                if (w <= r.w && h <= r.h)
                {
                    return r;
                }

                return null;
            }
        }

        private static Node<T> SplitNode<T>(Node<T> node, int w, int h)
        {
            node.used = true;
            node.down = new Node<T>(node.x, node.y + h, node.w, node.h - h);
            node.right = new Node<T>(node.x + w, node.y, node.w - w, h);
            return node;
        }

        private Node<T> GrowNode<T>(int w, int h)
        {
            var r = (Node<T>)this.root;
            var canGrowDown = w <= r.w;
            var canGrowRight = h <= r.h;

            var shouldGrowRight = canGrowRight && (r.h >= (r.w + w));
            var shouldGrowDown = canGrowDown && (r.w >= (r.h + h));

            if (shouldGrowRight)
            {
                return this.GrowRight<T>(w, h);
            }

            if (shouldGrowDown)
            {
                return this.GrowDown<T>(w, h);
            }

            if (canGrowRight)
            {
                return this.GrowRight<T>(w, h);
            }

            if (canGrowDown)
            {
                return this.GrowDown<T>(w, h);
            }
            return null;
        }

        private Node<T> GrowRight<T>(int w, int h)
        {
            var r = (Node<T>)this.root;
            this.root = new Node<T>
            {
                used = true,
                x = 0,
                y = 0,
                w = r.w + w,
                h = r.h,
                down = r,
                right = new Node<T>
                {
                    x = r.w,
                    y = 0,
                    w = w,
                    h = r.h
                }
            };

            Node<T> node;
            return (node = this.FindNode((Node<T>)this.root, w, h)) != null ? SplitNode(node, w, h) : null;
        }

        private Node<T> GrowDown<T>(int w, int h)
        {
            var r = (Node<T>)this.root;
            this.root = new Node<T>
            {
                used = true,
                x = 0,
                y = 0,
                w = r.w,
                h = r.h + h,
                down = r,
                right = new Node<T>
                {
                    x = 0,
                    y = r.h,
                    w = r.w,
                    h = h
                }
            };

            Node<T> node;
            return (node = this.FindNode((Node<T>)this.root, w, h)) != null ? SplitNode(node, w, h) : null;
        }
    }
}