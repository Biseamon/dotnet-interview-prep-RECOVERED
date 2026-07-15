namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 10 — Graphs. Traversal (DFS/BFS) over grids and adjacency lists, plus
// connectivity. The workhorses of many "hard" interview rounds.
internal static partial class AlgorithmsContent
{
    private static LessonSeed GraphsLesson => new()
    {
        Slug = "graphs",
        Title = "Graphs",
        Order = 10,
        MarkdownContent =
            """
            ## Graphs

            Model connections as nodes + edges. Two traversals cover most problems:
            - **DFS** — go deep, great for flood-fill and connectivity.
            - **BFS** — go level by level, great for shortest paths in unweighted graphs.

            Grids are graphs too (each cell connects to its neighbours). Always track
            **visited** to avoid cycles/re-work.
            """,
        Exercises =
        [
            NumberOfIslands,
            CountComponents,
        ],
    };

    private static ExerciseSeed NumberOfIslands => new()
    {
        Slug = "number-of-islands",
        Title = "Number of Islands",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given a grid of `1` (land) and `0` (water), count the islands. Land cells
            connect horizontally/vertically. DFS/flood-fill from each unvisited land cell,
            sinking the whole island so you don't count it twice.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: DFS flood-fill each island; count how many you start.
                public static int NumIslands(int[][] grid)
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
                    r.Check("one big island -> 1", () =>
                    {
                        var grid = new[]
                        {
                            new[]{1,1,0,0},
                            new[]{1,1,0,0},
                            new[]{0,0,1,0},
                            new[]{0,0,0,0},
                        };
                        Assert.Equal(2, Solution.NumIslands(grid));
                    });
                    r.Check("three separate islands -> 3", () =>
                    {
                        var grid = new[]
                        {
                            new[]{1,0,1},
                            new[]{0,0,0},
                            new[]{1,0,0},
                        };
                        Assert.Equal(3, Solution.NumIslands(grid));
                    });
                    r.Check("all water -> 0", () =>
                        Assert.Equal(0, Solution.NumIslands(new[]{ new[]{0,0}, new[]{0,0} })));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int NumIslands(int[][] grid)
                {
                    if (grid.Length == 0) return 0;
                    int rows = grid.Length, cols = grid[0].Length, count = 0;

                    void Sink(int r, int c)
                    {
                        // Out of bounds or water -> stop.
                        if (r < 0 || c < 0 || r >= rows || c >= cols || grid[r][c] == 0) return;
                        grid[r][c] = 0;          // mark visited by sinking
                        Sink(r + 1, c); Sink(r - 1, c);
                        Sink(r, c + 1); Sink(r, c - 1);
                    }

                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            if (grid[r][c] == 1) { count++; Sink(r, c); } // new island found

                    return count;
                }
            }
            """,
        Hints =
        [
            "Scan every cell; when you hit unvisited land, that's a new island.",
            "Flood-fill (DFS) from it, sinking all connected land to 0 so it's not recounted.",
            "Recurse into the four neighbours, guarding array bounds.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "one big island -> 1", IsHidden = false },
            new TestCaseSeed { Name = "three separate islands -> 3", IsHidden = false },
            new TestCaseSeed { Name = "all water -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed CountComponents => new()
    {
        Slug = "count-connected-components",
        Title = "Count Connected Components",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given `n` nodes (`0..n-1`) and an undirected `edges` list, count the connected
            components. Build an adjacency list, then DFS/BFS from each unvisited node —
            each traversal covers one whole component.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: build adjacency, DFS unvisited nodes, count components.
                public static int CountComponents(int n, int[][] edges)
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
                    r.Check("5 nodes, [[0,1],[1,2],[3,4]] -> 2", () =>
                        Assert.Equal(2, Solution.CountComponents(5, new[]{ new[]{0,1}, new[]{1,2}, new[]{3,4} })));
                    r.Check("5 nodes, [[0,1],[1,2],[2,3],[3,4]] -> 1", () =>
                        Assert.Equal(1, Solution.CountComponents(5, new[]{ new[]{0,1}, new[]{1,2}, new[]{2,3}, new[]{3,4} })));
                    r.Check("3 nodes, no edges -> 3", () =>
                        Assert.Equal(3, Solution.CountComponents(3, new int[0][])));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static int CountComponents(int n, int[][] edges)
                {
                    // Adjacency list for an undirected graph.
                    var adj = new List<int>[n];
                    for (int i = 0; i < n; i++) adj[i] = new List<int>();
                    foreach (var e in edges)
                    {
                        adj[e[0]].Add(e[1]);
                        adj[e[1]].Add(e[0]);
                    }

                    var visited = new bool[n];
                    int components = 0;

                    void Dfs(int node)
                    {
                        visited[node] = true;
                        foreach (var next in adj[node])
                            if (!visited[next]) Dfs(next);
                    }

                    for (int i = 0; i < n; i++)
                        if (!visited[i]) { components++; Dfs(i); } // each start = one component

                    return components;
                }
            }
            """,
        Hints =
        [
            "Build an adjacency list; add both directions for undirected edges.",
            "DFS from every node you haven't visited yet.",
            "Each DFS you START marks a new component.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "5 nodes, [[0,1],[1,2],[3,4]] -> 2", IsHidden = false },
            new TestCaseSeed { Name = "5 nodes, [[0,1],[1,2],[2,3],[3,4]] -> 1", IsHidden = false },
            new TestCaseSeed { Name = "3 nodes, no edges -> 3", IsHidden = true },
        ],
    };
}
