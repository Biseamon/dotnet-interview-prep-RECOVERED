namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 5 — Binary Trees. Recursion and BFS/DFS traversals. `TreeNode` (val,
// left, right) is provided by the grader.
internal static partial class AlgorithmsContent
{
    private static LessonSeed TreesLesson => new()
    {
        Slug = "trees",
        Title = "Binary Trees",
        Order = 5,
        MarkdownContent =
            """
            ## Binary Trees

            Most tree problems are a two-line **recursion**: do something with the node,
            recurse left and right, combine. Breadth-first (level order) uses a
            **queue**. Keep the base case (`null` node) front of mind.

            The `TreeNode` class (`int val; TreeNode left, right;`) is provided.
            """,
        Exercises =
        [
            MaxDepth,
            InvertTree,
            SameTree,
            LevelOrder,
        ],
    };

    private static ExerciseSeed MaxDepth => new()
    {
        Slug = "max-depth-binary-tree",
        Title = "Maximum Depth of Binary Tree",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return the maximum depth (number of nodes on the longest root-to-leaf path)
            of a binary tree. A one-line recursion: `1 + max(depth(left), depth(right))`,
            with depth of `null` = 0.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: recursive depth. null -> 0.
                public static int MaxDepth(TreeNode? root)
                {
                    return 0;
                }
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[3,9,20,null,null,15,7] -> 3", () =>
                    {
                        var root = new TreeNode(3,
                            new TreeNode(9),
                            new TreeNode(20, new TreeNode(15), new TreeNode(7)));
                        Assert.Equal(3, Solution.MaxDepth(root));
                    });
                    r.Check("single node -> 1", () => Assert.Equal(1, Solution.MaxDepth(new TreeNode(1))));
                    r.Check("empty -> 0", () => Assert.Equal(0, Solution.MaxDepth(null)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int MaxDepth(TreeNode? root)
                {
                    if (root == null) return 0; // base case
                    return 1 + Math.Max(MaxDepth(root.left), MaxDepth(root.right));
                }
            }
            """,
        Hints =
        [
            "The depth of an empty tree is 0 — that's your base case.",
            "A node's depth is 1 plus the deeper of its two subtrees.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[3,9,20,null,null,15,7] -> 3", IsHidden = false },
            new TestCaseSeed { Name = "empty -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed InvertTree => new()
    {
        Slug = "invert-binary-tree",
        Title = "Invert Binary Tree",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Invert a binary tree (mirror it): swap every node's left and right children.
            Return the root. Recurse, swapping as you go.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: swap children recursively.
                public static TreeNode? Invert(TreeNode? root)
                {
                    return root;
                }
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;
            using System.Text;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[4,2,7,1,3,6,9] -> [4,7,2,9,6,3,1]", () =>
                    {
                        var root = new TreeNode(4,
                            new TreeNode(2, new TreeNode(1), new TreeNode(3)),
                            new TreeNode(7, new TreeNode(6), new TreeNode(9)));
                        Assert.Equal("4,7,2,9,6,3,1", Bfs(Solution.Invert(root)));
                    });
                    r.Check("empty -> empty", () => Assert.Equal("", Bfs(Solution.Invert(null))));
                    return r.ToJson();
                }

                // Level-order serialization (skips nulls) for comparison.
                private static string Bfs(TreeNode? root)
                {
                    if (root == null) return "";
                    var q = new Queue<TreeNode>();
                    q.Enqueue(root);
                    var sb = new StringBuilder();
                    while (q.Count > 0)
                    {
                        var n = q.Dequeue();
                        if (sb.Length > 0) sb.Append(',');
                        sb.Append(n.val);
                        if (n.left != null) q.Enqueue(n.left);
                        if (n.right != null) q.Enqueue(n.right);
                    }
                    return sb.ToString();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static TreeNode? Invert(TreeNode? root)
                {
                    if (root == null) return null;
                    (root.left, root.right) = (Invert(root.right), Invert(root.left)); // swap + recurse
                    return root;
                }
            }
            """,
        Hints =
        [
            "Base case: inverting null gives null.",
            "Swap the left and right child references.",
            "Recurse into both children (before or after the swap — just be consistent).",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[4,2,7,1,3,6,9] -> [4,7,2,9,6,3,1]", IsHidden = false },
            new TestCaseSeed { Name = "empty -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed SameTree => new()
    {
        Slug = "same-tree",
        Title = "Same Tree",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given two binary trees, return `true` if they are structurally identical
            with equal values. Recurse both in lockstep.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: return true if both trees match in structure and values.
                public static bool IsSameTree(TreeNode? p, TreeNode? q)
                {
                    return false;
                }
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,3] == [1,2,3] -> true", () =>
                    {
                        var a = new TreeNode(1, new TreeNode(2), new TreeNode(3));
                        var b = new TreeNode(1, new TreeNode(2), new TreeNode(3));
                        Assert.True(Solution.IsSameTree(a, b));
                    });
                    r.Check("[1,2] vs [1,null,2] -> false", () =>
                    {
                        var a = new TreeNode(1, new TreeNode(2), null);
                        var b = new TreeNode(1, null, new TreeNode(2));
                        Assert.False(Solution.IsSameTree(a, b));
                    });
                    r.Check("both empty -> true", () => Assert.True(Solution.IsSameTree(null, null)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static bool IsSameTree(TreeNode? p, TreeNode? q)
                {
                    if (p == null && q == null) return true;   // both empty -> equal
                    if (p == null || q == null) return false;  // one empty -> differ
                    return p.val == q.val
                        && IsSameTree(p.left, q.left)
                        && IsSameTree(p.right, q.right);
                }
            }
            """,
        Hints =
        [
            "Two nulls are equal; one null and one node are not.",
            "Otherwise the values must match AND both subtrees must match.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3] == [1,2,3] -> true", IsHidden = false },
            new TestCaseSeed { Name = "[1,2] vs [1,null,2] -> false", IsHidden = false },
        ],
    };

    private static ExerciseSeed LevelOrder => new()
    {
        Slug = "binary-tree-level-order",
        Title = "Binary Tree Level Order Traversal",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the values of a binary tree level by level, top to bottom, as a
            flattened array (left to right within each level). Use a **queue** (BFS).
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: BFS with a queue, flattening levels left-to-right.
                public static int[] LevelOrder(TreeNode? root)
                {
                    return new int[0];
                }
            }
            """,
        HarnessCode =
            """
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[3,9,20,null,null,15,7] -> 3,9,20,15,7", () =>
                    {
                        var root = new TreeNode(3,
                            new TreeNode(9),
                            new TreeNode(20, new TreeNode(15), new TreeNode(7)));
                        Assert.Equal("3,9,20,15,7", string.Join(",", Solution.LevelOrder(root)));
                    });
                    r.Check("empty -> ''", () => Assert.Equal("", string.Join(",", Solution.LevelOrder(null))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static int[] LevelOrder(TreeNode? root)
                {
                    var result = new List<int>();
                    if (root == null) return result.ToArray();

                    var queue = new Queue<TreeNode>();
                    queue.Enqueue(root);
                    while (queue.Count > 0)
                    {
                        var node = queue.Dequeue();
                        result.Add(node.val);
                        if (node.left != null) queue.Enqueue(node.left);
                        if (node.right != null) queue.Enqueue(node.right);
                    }
                    return result.ToArray();
                }
            }
            """,
        Hints =
        [
            "Breadth-first traversal uses a FIFO queue.",
            "Dequeue a node, record its value, enqueue its non-null children.",
            "Enqueue left before right to keep left-to-right order.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[3,9,20,null,null,15,7] -> 3,9,20,15,7", IsHidden = false },
            new TestCaseSeed { Name = "empty -> ''", IsHidden = true },
        ],
    };
}
