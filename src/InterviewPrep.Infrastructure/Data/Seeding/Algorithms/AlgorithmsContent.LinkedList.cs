namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 4 — Linked Lists. Pointer manipulation: reversing, merging, and cycle
// detection. `ListNode` is provided by the grader (val + next).
internal static partial class AlgorithmsContent
{
    private static LessonSeed LinkedListLesson => new()
    {
        Slug = "linked-lists",
        Title = "Linked Lists",
        Order = 4,
        MarkdownContent =
            """
            ## Linked Lists

            A node holds a `val` and a `next` pointer. Interview staples:
            - **Reverse** by re-pointing `next` as you walk (track prev/curr).
            - **Merge** two sorted lists with a dummy head.
            - **Detect a cycle** with fast/slow pointers (Floyd's algorithm).

            The `ListNode` class (`int val; ListNode next;`) is provided for you.
            """,
        Exercises =
        [
            ReverseLinkedList,
            MergeTwoSortedLists,
            LinkedListCycle,
        ],
    };

    private static ExerciseSeed ReverseLinkedList => new()
    {
        Slug = "reverse-linked-list",
        Title = "Reverse a Linked List",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Reverse a singly linked list and return the new head. Iterative approach:
            walk the list re-pointing each node's `next` to the previous node. O(n)
            time, O(1) space. (`ListNode` is provided.)
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: reverse the list by re-pointing next. Return the new head.
                public static ListNode? Reverse(ListNode? head)
                {
                    return null;
                }
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,3,4,5] -> [5,4,3,2,1]", () =>
                        Assert.Equal("5,4,3,2,1", ToStr(Solution.Reverse(Build(new[]{1,2,3,4,5})))));
                    r.Check("[1,2] -> [2,1]", () =>
                        Assert.Equal("2,1", ToStr(Solution.Reverse(Build(new[]{1,2})))));
                    r.Check("[] -> []", () =>
                        Assert.Equal("", ToStr(Solution.Reverse(Build(new int[0])))));
                    return r.ToJson();
                }

                private static ListNode? Build(int[] xs)
                {
                    ListNode? head = null;
                    for (int i = xs.Length - 1; i >= 0; i--) head = new ListNode(xs[i], head);
                    return head;
                }
                private static string ToStr(ListNode? n)
                {
                    var parts = new List<string>();
                    for (; n != null; n = n.next) parts.Add(n.val.ToString());
                    return string.Join(",", parts);
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static ListNode? Reverse(ListNode? head)
                {
                    ListNode? prev = null;
                    while (head != null)
                    {
                        var next = head.next; // save the rest of the list
                        head.next = prev;     // reverse this link
                        prev = head;          // advance prev
                        head = next;          // advance head
                    }
                    return prev; // prev is the new head
                }
            }
            """,
        Hints =
        [
            "Keep three references: prev, current (head), and the saved next.",
            "For each node: save next, point current.next at prev, then move both forward.",
            "When you fall off the end, prev is the new head.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3,4,5] -> [5,4,3,2,1]", IsHidden = false },
            new TestCaseSeed { Name = "[] -> []", IsHidden = true },
        ],
    };

    private static ExerciseSeed MergeTwoSortedLists => new()
    {
        Slug = "merge-two-sorted-lists",
        Title = "Merge Two Sorted Lists",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Merge two sorted linked lists into one sorted list and return its head.
            Use a **dummy head** and splice the smaller current node each step. O(n+m).
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: merge two sorted lists using a dummy head.
                public static ListNode? Merge(ListNode? a, ListNode? b)
                {
                    return null;
                }
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,4]+[1,3,4] -> [1,1,2,3,4,4]", () =>
                        Assert.Equal("1,1,2,3,4,4", ToStr(Solution.Merge(Build(new[]{1,2,4}), Build(new[]{1,3,4})))));
                    r.Check("[]+[] -> []", () =>
                        Assert.Equal("", ToStr(Solution.Merge(Build(new int[0]), Build(new int[0])))));
                    r.Check("[]+[0] -> [0]", () =>
                        Assert.Equal("0", ToStr(Solution.Merge(Build(new int[0]), Build(new[]{0})))));
                    return r.ToJson();
                }

                private static ListNode? Build(int[] xs)
                {
                    ListNode? head = null;
                    for (int i = xs.Length - 1; i >= 0; i--) head = new ListNode(xs[i], head);
                    return head;
                }
                private static string ToStr(ListNode? n)
                {
                    var parts = new List<string>();
                    for (; n != null; n = n.next) parts.Add(n.val.ToString());
                    return string.Join(",", parts);
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static ListNode? Merge(ListNode? a, ListNode? b)
                {
                    var dummy = new ListNode();  // sentinel so we never special-case the head
                    var tail = dummy;
                    while (a != null && b != null)
                    {
                        if (a.val <= b.val) { tail.next = a; a = a.next; }
                        else                { tail.next = b; b = b.next; }
                        tail = tail.next;
                    }
                    tail.next = a ?? b; // attach whatever remains (already sorted)
                    return dummy.next;
                }
            }
            """,
        Hints =
        [
            "A dummy head node avoids special-casing the first element.",
            "Keep a tail pointer; each step append the smaller of the two current nodes.",
            "When one list runs out, attach the remainder of the other.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,4]+[1,3,4] -> [1,1,2,3,4,4]", IsHidden = false },
            new TestCaseSeed { Name = "[]+[0] -> [0]", IsHidden = false },
        ],
    };

    private static ExerciseSeed LinkedListCycle => new()
    {
        Slug = "linked-list-cycle",
        Title = "Linked List Cycle",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return `true` if the linked list has a cycle. Use **Floyd's fast/slow
            pointers**: if a fast pointer (2 steps) ever meets a slow pointer (1 step),
            there's a loop. O(n) time, O(1) space.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: fast/slow pointers. Return true if they ever meet.
                public static bool HasCycle(ListNode? head)
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

                    r.Check("no cycle -> false", () =>
                    {
                        var head = new ListNode(1, new ListNode(2, new ListNode(3)));
                        Assert.False(Solution.HasCycle(head));
                    });
                    r.Check("cycle -> true", () =>
                    {
                        var third = new ListNode(3);
                        var head = new ListNode(1, new ListNode(2, third));
                        third.next = head; // create a loop back to the start
                        Assert.True(Solution.HasCycle(head));
                    });
                    r.Check("empty -> false", () => Assert.False(Solution.HasCycle(null)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static bool HasCycle(ListNode? head)
                {
                    ListNode? slow = head, fast = head;
                    while (fast != null && fast.next != null)
                    {
                        slow = slow!.next;      // 1 step
                        fast = fast.next.next;  // 2 steps
                        if (slow == fast) return true; // they met -> cycle
                    }
                    return false; // fast reached the end -> no cycle
                }
            }
            """,
        Hints =
        [
            "Two runners at different speeds on a loop will eventually meet.",
            "Advance slow by 1 and fast by 2 each iteration.",
            "If fast (or fast.next) hits null, there's no cycle.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "no cycle -> false", IsHidden = false },
            new TestCaseSeed { Name = "cycle -> true", IsHidden = false },
        ],
    };
}
