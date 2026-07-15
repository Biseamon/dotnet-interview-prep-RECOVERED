namespace InterviewPrep.Infrastructure.Data.Seeding.EfCore;

// The "EF Core & Data" topic. The interview-critical ideas — projections to avoid
// over-fetching, fixing N+1, pagination, and GROUP BY aggregation — are taught as
// LINQ-over-in-memory exercises. The query LOGIC is identical to what EF Core
// translates to SQL; only the data source differs.
internal static partial class EfCoreContent
{
    // Shared sample entities, provided in every exercise's starter so learners can
    // reference them. (Defined per-exercise to keep each self-contained.)
    public static TopicSeed Topic => new()
    {
        Slug = "ef-core",
        Name = "EF Core & Data",
        Description = "Projections, the N+1 problem, pagination, and GROUP BY aggregation — the query skills that matter.",
        Order = 8,
        Lessons =
        [
            QueryingLesson,
            JoinsAndAggregationLesson,
            AdvancedQueryLesson,
        ],
    };

    private static LessonSeed QueryingLesson => new()
    {
        Slug = "querying",
        Title = "Querying & Performance",
        Order = 1,
        MarkdownContent =
            """
            ## EF Core & Data: Query Like the SQL It Becomes

            EF Core is a translator. You write LINQ against `IQueryable<T>`; the provider
            walks that expression tree and emits SQL. The single most useful mental model
            for interviews is: **"what SQL does this LINQ become, and how much data crosses
            the wire?"** Almost every EF performance question reduces to that.

            ### Over-fetching and projection

            The default trap is pulling whole entities when you need two columns. If you
            materialize `Product` objects, EF emits `SELECT * FROM Products` — every column,
            every row, then you throw most of it away in memory. Instead **project** early
            with `Select` into a small DTO or tuple:

            ```csharp
            // Reads only Name: SELECT p.Name FROM Products p WHERE p.Price > @min
            db.Products.Where(p => p.Price > min).Select(p => p.Name);
            ```

            The rule of thumb interviewers want to hear: *filter first, project last, and
            never select more columns than you render.*

            ### The N+1 problem — the classic red flag

            N+1 is the bug interviewers probe for most. You run **one** query to load a list
            (say, customers), then loop and run **one more query per row** to load related
            data (each customer's orders). For 500 customers that's 501 round trips to the
            database — each with network latency. The symptom is a page that's fine with test
            data and collapses in production.

            ```csharp
            // N+1: one query for customers, then a query PER customer inside the loop
            foreach (var c in db.Customers)                     // 1 query
                Console.WriteLine(db.Orders.Count(o => o.CustomerId == c.Id)); // N queries
            ```

            The fix is to fetch related data in **one** query — in EF via `Include` or a
            `GroupBy`/join projection, and in plain LINQ (as here) by building a lookup
            once and reading from it in O(1). One pass, not N passes.

            ### Pagination

            `Skip((page-1)*size).Take(size)` becomes `OFFSET/FETCH`. It's correct but gets
            slow deep into a table because the database still counts and discards every
            skipped row. We revisit that limitation with keyset pagination later.

            ### Aggregation

            `GroupBy` + `Sum`/`Count`/`Max` becomes SQL `GROUP BY` with aggregate functions.
            Push aggregation into the query so the database returns a handful of totals, not
            thousands of rows for you to sum in C#.

            These exercises use in-memory LINQ, but the operator shape is exactly what EF
            emits — learn the LINQ and you've learned the SQL.
            """,
        Exercises =
        [
            Projection,
            NPlusOne,
            Pagination,
            GroupAggregate,
            DistinctProjection,
            NPlusOneDto,
        ],
    };

    private static LessonSeed JoinsAndAggregationLesson => new()
    {
        Slug = "joins-and-aggregation",
        Title = "Joins, Left-Joins & Grouped Aggregation",
        Order = 2,
        MarkdownContent =
            """
            ## Joins and Grouped Aggregation

            Once you can filter and project, the next tier of query skill is **combining
            tables** and **summarizing groups**. These map directly onto SQL, and getting
            the edge cases right (rows with no match, groups below a threshold, ties) is
            what separates a passing answer from a great one.

            ### Inner join vs. left join

            An **inner join** keeps only rows that have a match on both sides. A **left
            join** keeps every row from the left side even when the right side has nothing —
            the missing side comes back as `null`, and counts/sums over it should be **zero**,
            not absent. Interviewers love left-join-with-count-including-zeros because the
            naive `GroupBy` over the child table silently drops parents that have no
            children. In LINQ you express a left join with `GroupJoin` + `SelectMany` +
            `DefaultIfEmpty`, or (as we do here) by grouping the child rows into a lookup and
            iterating the **parent** list so zero-count parents survive.

            ```csharp
            // Left join: every customer appears, even with 0 orders
            var byCust = orders.ToLookup(o => o.CustomerId);
            var rows = customers.Select(c => (c.Name, Count: byCust[c.Id].Count()));
            ```

            ### GROUP BY ... HAVING

            `HAVING` filters **groups after aggregation** — "categories whose total exceeds
            1000" — as opposed to `WHERE`, which filters **rows before** grouping. In LINQ
            that's simply a `Where` placed *after* the `GroupBy`/`Select` of aggregates.
            Mixing the two up is a common interview slip: filter rows with `Where` before
            `GroupBy`, filter aggregated groups with `Where` after.

            ### Conditional aggregation (pivot)

            SQL's `SUM(CASE WHEN ... THEN amount ELSE 0 END)` pivots rows into columns — e.g.
            per customer, how much was `paid` vs `refunded`. In LINQ each "column" is a
            filtered aggregate inside the group projection: `g.Where(x => x.Status == "paid")
            .Sum(...)`. This turns a tall table into one wide row per key.

            Watch the edge cases in every one of these: empty input, a group with a single
            row, and ties where two groups share the same total.
            """,
        Exercises =
        [
            LeftJoinCounts,
            GroupByHaving,
            ConditionalAggregation,
        ],
    };

    private static LessonSeed AdvancedQueryLesson => new()
    {
        Slug = "advanced-queries",
        Title = "Keyset Paging, Top-N & Anti-Joins",
        Order = 3,
        MarkdownContent =
            """
            ## Advanced Query Patterns

            These are the multi-step problems senior interviews reach for. Each one layers
            several operators and has a sharp edge case.

            ### Keyset (seek) pagination

            `Skip/Take` pagination degrades deep into a table: to serve page 10,000 the
            database must still scan and discard 100,000 rows every request, and if rows are
            inserted between page loads, items shift and you get duplicates or gaps. **Keyset
            pagination** fixes both. Instead of an offset you remember the **last key you
            saw** (a bookmark) and ask for "the next N rows *after* this key":

            ```sql
            SELECT * FROM Items WHERE Id > @lastSeenId ORDER BY Id LIMIT @size;
            ```

            It's O(page size) regardless of depth because an index seek jumps straight to the
            boundary — no counting skipped rows. The trade-off: you can only go forward/back
            relative to a key, not jump to an arbitrary page number. The subtle bit is the
            ordering key must be **unique and stable** (usually the primary key, or a
            composite `(sortColumn, id)` to break ties).

            ### Top-N per group

            "The 2 highest-paid employees in each department" is `ROW_NUMBER() OVER
            (PARTITION BY dept ORDER BY salary DESC)` filtered to `<= N`. In LINQ:
            `GroupBy(dept)` then, inside each group, `OrderByDescending(salary).Take(N)`.
            The edge cases are groups with fewer than N members (take them all) and ties on
            the boundary (decide with a stable secondary sort).

            ### Anti-join

            An **anti-join** returns rows from one set that have **no** match in another —
            "customers who never ordered", "products never sold". SQL expresses it as `NOT
            EXISTS` or `LEFT JOIN ... WHERE right IS NULL`. In LINQ, build a `HashSet` of the
            keys that *do* have a match, then filter the left side to those **not** in the
            set. Doing it with `Where(x => !others.Any(...))` is the N+1-flavored quadratic
            trap; the `HashSet` makes it linear.

            Master these three and you can answer most "how would you write this query"
            whiteboard prompts.
            """,
        Exercises =
        [
            MultiKeySort,
            KeysetPagination,
            TopNPerGroup,
            AntiJoin,
        ],
    };

    private static ExerciseSeed Projection => new()
    {
        Slug = "ef-projection",
        Title = "Project to a DTO (Avoid Over-Fetching)",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given `Product` entities, return just the names of products priced over a
            threshold. Projecting with `Select` (rather than returning whole entities) is
            what lets EF Core emit `SELECT Name` instead of `SELECT *`.
            """,
        StarterCode =
            """
            using System.Linq;

            // PROVIDED — do not modify:
            public record Product(int Id, string Name, decimal Price);

            public static class Solution
            {
                // TODO: return the Names of products with Price > minPrice.
                public static string[] ExpensiveNames(Product[] products, decimal minPrice)
                {
                    return new string[0];
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
                    var products = new[]
                    {
                        new Product(1, "pen", 2m),
                        new Product(2, "desk", 150m),
                        new Product(3, "chair", 80m),
                    };
                    r.Check("names over 50", () =>
                        Assert.Equal("desk,chair", string.Join(",", Solution.ExpensiveNames(products, 50m))));
                    r.Check("nothing over 500 -> empty", () =>
                        Assert.Equal("", string.Join(",", Solution.ExpensiveNames(products, 500m))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public record Product(int Id, string Name, decimal Price);

            public static class Solution
            {
                public static string[] ExpensiveNames(Product[] products, decimal minPrice) =>
                    products.Where(p => p.Price > minPrice)  // SQL WHERE
                            .Select(p => p.Name)             // SQL SELECT Name (projection)
                            .ToArray();
            }
            """,
        Hints =
        [
            "Filter with Where(p => p.Price > minPrice).",
            "Project to just the name with Select(p => p.Name).",
            "This is why projecting matters: EF would read only the Name column.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "names over 50", IsHidden = false },
            new TestCaseSeed { Name = "nothing over 500 -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed NPlusOne => new()
    {
        Slug = "n-plus-one",
        Title = "Fix the N+1 Problem",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return each customer's order count as `"name:count"` strings. The naive way
            scans all orders **per customer** (that's the N+1 shape). Do it in a single
            pass by first building a lookup of counts by customerId — O(n+m), not O(n·m).
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Customer(int Id, string Name);
            public record Order(int Id, int CustomerId);

            public static class Solution
            {
                // TODO: return "name:count" per customer, without scanning orders per customer.
                public static string[] OrderCounts(Customer[] customers, Order[] orders)
                {
                    return new string[0];
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
                    var customers = new[] { new Customer(1, "ann"), new Customer(2, "bob") };
                    var orders = new[]
                    {
                        new Order(10, 1), new Order(11, 1), new Order(12, 2),
                    };
                    r.Check("counts per customer", () =>
                        Assert.Equal("ann:2,bob:1", string.Join(",", Solution.OrderCounts(customers, orders))));
                    r.Check("customer with no orders -> 0", () =>
                    {
                        var cs = new[] { new Customer(1, "ann"), new Customer(3, "cid") };
                        Assert.Equal("ann:2,cid:0", string.Join(",", Solution.OrderCounts(cs, orders)));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Customer(int Id, string Name);
            public record Order(int Id, int CustomerId);

            public static class Solution
            {
                public static string[] OrderCounts(Customer[] customers, Order[] orders)
                {
                    // ONE pass over orders builds the lookup — no per-customer rescan.
                    var counts = orders.GroupBy(o => o.CustomerId)
                                       .ToDictionary(g => g.Key, g => g.Count());

                    return customers
                        .Select(c => $"{c.Name}:{counts.GetValueOrDefault(c.Id)}")
                        .ToArray();
                }
            }
            """,
        Hints =
        [
            "Scanning `orders.Count(o => o.CustomerId == c.Id)` inside a customer loop is the N+1 shape.",
            "Build a Dictionary<customerId, count> once with GroupBy/ToDictionary.",
            "Then look up each customer's count in O(1); default to 0 when missing.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "counts per customer", IsHidden = false },
            new TestCaseSeed { Name = "customer with no orders -> 0", IsHidden = false },
        ],
    };

    private static ExerciseSeed Pagination => new()
    {
        Slug = "ef-pagination",
        Title = "Pagination with Skip/Take",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return one page of results. Given 1-based `pageNumber` and `pageSize`, use
            `Skip`/`Take` (what EF translates to `OFFSET`/`FETCH`). Page 1 is the first
            `pageSize` items.
            """,
        StarterCode =
            """
            using System.Linq;

            public static class Solution
            {
                // TODO: return the requested page using Skip/Take (pageNumber is 1-based).
                public static int[] Page(int[] items, int pageNumber, int pageSize)
                {
                    return new int[0];
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
                    var items = new[] {1,2,3,4,5,6,7};
                    r.Check("page 1 size 3 -> 1,2,3", () =>
                        Assert.Equal("1,2,3", string.Join(",", Solution.Page(items, 1, 3))));
                    r.Check("page 2 size 3 -> 4,5,6", () =>
                        Assert.Equal("4,5,6", string.Join(",", Solution.Page(items, 2, 3))));
                    r.Check("page 3 size 3 -> 7 (partial)", () =>
                        Assert.Equal("7", string.Join(",", Solution.Page(items, 3, 3))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public static class Solution
            {
                public static int[] Page(int[] items, int pageNumber, int pageSize) =>
                    items.Skip((pageNumber - 1) * pageSize) // OFFSET
                         .Take(pageSize)                    // FETCH NEXT
                         .ToArray();
            }
            """,
        Hints =
        [
            "Skip past the earlier pages: (pageNumber - 1) * pageSize items.",
            "Take pageSize items for the current page.",
            "The last page may be partial — Take handles that automatically.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "page 1 size 3 -> 1,2,3", IsHidden = false },
            new TestCaseSeed { Name = "page 2 size 3 -> 4,5,6", IsHidden = false },
            new TestCaseSeed { Name = "page 3 size 3 -> 7 (partial)", IsHidden = true },
        ],
    };

    private static ExerciseSeed GroupAggregate => new()
    {
        Slug = "ef-group-aggregate",
        Title = "GROUP BY Aggregation",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given line items, return total revenue per category as sorted `"category=total"`
            strings. This is `GroupBy` + `Sum` — exactly what EF translates to SQL
            `GROUP BY category`.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record LineItem(string Category, decimal Amount);

            public static class Solution
            {
                // TODO: total Amount per Category, formatted "category=total", sorted by category.
                public static string[] RevenueByCategory(LineItem[] items)
                {
                    return new string[0];
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
                    var items = new[]
                    {
                        new LineItem("books", 10m),
                        new LineItem("toys", 5m),
                        new LineItem("books", 15m),
                    };
                    r.Check("sums per category, sorted", () =>
                        Assert.Equal("books=25,toys=5", string.Join(",", Solution.RevenueByCategory(items))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record LineItem(string Category, decimal Amount);

            public static class Solution
            {
                public static string[] RevenueByCategory(LineItem[] items) =>
                    items.GroupBy(i => i.Category)                    // SQL GROUP BY Category
                         .OrderBy(g => g.Key)
                         .Select(g => $"{g.Key}={g.Sum(i => i.Amount)}") // SUM(Amount)
                         .ToArray();
            }
            """,
        Hints =
        [
            "GroupBy(i => i.Category) forms one group per category.",
            "Sum each group's Amount.",
            "OrderBy the key so output is deterministic; format as \"category=total\".",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "sums per category, sorted", IsHidden = false },
        ],
    };

    private static ExerciseSeed DistinctProjection => new()
    {
        Slug = "ef-distinct-projection",
        Title = "DISTINCT After Projection",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given order rows (each tied to a city), return the **distinct** cities that have
            at least one order, sorted alphabetically. Project to the city first, then
            de-duplicate — this is SQL `SELECT DISTINCT City ORDER BY City`. Projecting
            before `Distinct` matters: DISTINCT over whole rows would keep near-duplicates.
            """,
        StarterCode =
            """
            using System.Linq;

            // PROVIDED — do not modify:
            public record Order(int Id, string City);

            public static class Solution
            {
                // TODO: return the distinct cities (sorted A->Z) that appear in orders.
                public static string[] Cities(Order[] orders)
                {
                    return new string[0];
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
                    var orders = new[]
                    {
                        new Order(1, "oslo"),
                        new Order(2, "bergen"),
                        new Order(3, "oslo"),
                        new Order(4, "bergen"),
                    };
                    r.Check("distinct cities sorted", () =>
                        Assert.Equal("bergen,oslo", string.Join(",", Solution.Cities(orders))));
                    r.Check("empty input -> empty", () =>
                        Assert.Equal("", string.Join(",", Solution.Cities(new Order[0]))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public record Order(int Id, string City);

            public static class Solution
            {
                public static string[] Cities(Order[] orders) =>
                    orders.Select(o => o.City)   // SELECT City (project first)
                          .Distinct()            // DISTINCT
                          .OrderBy(c => c)       // ORDER BY City
                          .ToArray();
            }
            """,
        Hints =
        [
            "Select the City before calling Distinct, not after materializing whole rows.",
            "Distinct() removes the duplicate city values.",
            "OrderBy the string to get a stable, alphabetical result.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "distinct cities sorted", IsHidden = false },
            new TestCaseSeed { Name = "empty input -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed NPlusOneDto => new()
    {
        Slug = "ef-n-plus-one-dto",
        Title = "Fix N+1 by Joining Into a DTO",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Build one `OrderView` per order combining the order's `Total` with its
            customer's `Name` — the shape you'd return from an API. The naive version looks
            up the customer **per order** (N+1). Instead build a customer lookup **once**,
            then project each order into the DTO in a single pass. Return them as
            `"name:total"` strings sorted by the order `Id`.

            Assume every order references a customer that exists.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Customer(int Id, string Name);
            public record Order(int Id, int CustomerId, decimal Total);
            public record OrderView(string CustomerName, decimal Total);

            public static class Solution
            {
                // TODO: join each order to its customer via a one-time lookup, return
                // "name:total" sorted by order Id.
                public static string[] Views(Customer[] customers, Order[] orders)
                {
                    return new string[0];
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
                    var customers = new[] { new Customer(1, "ann"), new Customer(2, "bob") };
                    var orders = new[]
                    {
                        new Order(10, 1, 30m),
                        new Order(11, 2, 5m),
                        new Order(12, 1, 12m),
                    };
                    r.Check("joined views sorted by id", () =>
                        Assert.Equal("ann:30,bob:5,ann:12",
                            string.Join(",", Solution.Views(customers, orders))));
                    r.Check("no orders -> empty", () =>
                        Assert.Equal("", string.Join(",", Solution.Views(customers, new Order[0]))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Customer(int Id, string Name);
            public record Order(int Id, int CustomerId, decimal Total);
            public record OrderView(string CustomerName, decimal Total);

            public static class Solution
            {
                public static string[] Views(Customer[] customers, Order[] orders)
                {
                    // Build the lookup ONCE — no per-order customer scan (kills the N+1).
                    var byId = customers.ToDictionary(c => c.Id);

                    return orders
                        .OrderBy(o => o.Id)
                        .Select(o => new OrderView(byId[o.CustomerId].Name, o.Total))
                        .Select(v => $"{v.CustomerName}:{v.Total}")
                        .ToArray();
                }
            }
            """,
        Hints =
        [
            "Building a Dictionary<Id, Customer> once replaces the per-order lookup.",
            "Order the orders by Id before projecting for a deterministic result.",
            "Project into OrderView, then format as \"name:total\".",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "joined views sorted by id", IsHidden = false },
            new TestCaseSeed { Name = "no orders -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed LeftJoinCounts => new()
    {
        Slug = "ef-left-join-counts",
        Title = "Left Join With Counts (Including Zeros)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return every author with the number of posts they wrote, as `"name:count"`
            sorted by author name. Authors with **no** posts must still appear with `0` —
            that's the left-join behavior. A plain `GroupBy` over posts would silently drop
            postless authors, so iterate the **authors** and look up their post counts.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Author(int Id, string Name);
            public record Post(int Id, int AuthorId);

            public static class Solution
            {
                // TODO: "name:count" per author sorted by name; authors with no posts show 0.
                public static string[] PostCounts(Author[] authors, Post[] posts)
                {
                    return new string[0];
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
                    var authors = new[]
                    {
                        new Author(1, "ann"),
                        new Author(2, "bob"),
                        new Author(3, "cid"),
                    };
                    var posts = new[]
                    {
                        new Post(10, 1), new Post(11, 1), new Post(12, 2),
                    };
                    r.Check("counts with zeros, sorted", () =>
                        Assert.Equal("ann:2,bob:1,cid:0",
                            string.Join(",", Solution.PostCounts(authors, posts))));
                    r.Check("no posts at all -> all zero", () =>
                        Assert.Equal("ann:0,bob:0,cid:0",
                            string.Join(",", Solution.PostCounts(authors, new Post[0]))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Author(int Id, string Name);
            public record Post(int Id, int AuthorId);

            public static class Solution
            {
                public static string[] PostCounts(Author[] authors, Post[] posts)
                {
                    // ToLookup returns an empty group (Count 0) for missing keys — perfect
                    // for a left join that must keep zero-count parents.
                    var byAuthor = posts.ToLookup(p => p.AuthorId);

                    return authors
                        .OrderBy(a => a.Name)
                        .Select(a => $"{a.Name}:{byAuthor[a.Id].Count()}")
                        .ToArray();
                }
            }
            """,
        Hints =
        [
            "Iterate the authors (left side) so postless authors survive.",
            "posts.ToLookup(p => p.AuthorId) gives an empty group for authors with no posts.",
            "byAuthor[a.Id].Count() is 0 when the author has no posts.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "counts with zeros, sorted", IsHidden = false },
            new TestCaseSeed { Name = "no posts at all -> all zero", IsHidden = true },
        ],
    };

    private static ExerciseSeed GroupByHaving => new()
    {
        Slug = "ef-group-by-having",
        Title = "GROUP BY ... HAVING (Filter Groups)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given sales rows, return categories whose **total** amount is strictly greater
            than `threshold`, as `"category=total"` sorted by category. This is `GROUP BY
            category HAVING SUM(amount) > @threshold`. The key distinction: `HAVING` filters
            **after** aggregation, so the `Where` goes **after** the `GroupBy`/`Sum`.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Sale(string Category, decimal Amount);

            public static class Solution
            {
                // TODO: categories with SUM(Amount) > threshold, "category=total" sorted.
                public static string[] BigCategories(Sale[] sales, decimal threshold)
                {
                    return new string[0];
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
                    var sales = new[]
                    {
                        new Sale("books", 40m),
                        new Sale("toys", 5m),
                        new Sale("books", 30m),
                        new Sale("games", 100m),
                    };
                    r.Check("totals over 50", () =>
                        Assert.Equal("books=70,games=100",
                            string.Join(",", Solution.BigCategories(sales, 50m))));
                    r.Check("threshold excludes ties (not strictly greater)", () =>
                        Assert.Equal("games=100",
                            string.Join(",", Solution.BigCategories(sales, 70m))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Sale(string Category, decimal Amount);

            public static class Solution
            {
                public static string[] BigCategories(Sale[] sales, decimal threshold) =>
                    sales.GroupBy(s => s.Category)                 // GROUP BY Category
                         .Select(g => new { g.Key, Total = g.Sum(s => s.Amount) })
                         .Where(x => x.Total > threshold)          // HAVING SUM(Amount) > @t
                         .OrderBy(x => x.Key)
                         .Select(x => $"{x.Key}={x.Total}")
                         .ToArray();
            }
            """,
        Hints =
        [
            "GroupBy the category, then Sum each group's Amount.",
            "HAVING is a Where placed AFTER aggregation — filter on the computed Total.",
            "Use strictly-greater (>) so a total equal to the threshold is excluded.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "totals over 50", IsHidden = false },
            new TestCaseSeed { Name = "threshold excludes ties (not strictly greater)", IsHidden = true },
        ],
    };

    private static ExerciseSeed ConditionalAggregation => new()
    {
        Slug = "ef-conditional-aggregation",
        Title = "Conditional Aggregation (Pivot)",
        Difficulty = "Hard",
        Kind = "Function",
        Prompt =
            """
            Pivot payment rows into one summary line per customer. For each customer output
            `"name:paid/refunded"` where `paid` is the sum of amounts with `Status == "paid"`
            and `refunded` is the sum with `Status == "refunded"` (ignore any other status).
            Sort by customer name. This mirrors SQL `SUM(CASE WHEN Status='paid' THEN Amount
            ELSE 0 END)` — each "column" is a filtered sum inside the group.

            Every customer in the input appears in the output even if one bucket is 0.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Payment(string Customer, string Status, decimal Amount);

            public static class Solution
            {
                // TODO: "name:paid/refunded" per customer, sorted by name.
                public static string[] Pivot(Payment[] payments)
                {
                    return new string[0];
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
                    var payments = new[]
                    {
                        new Payment("ann", "paid", 100m),
                        new Payment("ann", "refunded", 20m),
                        new Payment("ann", "paid", 50m),
                        new Payment("bob", "paid", 10m),
                        new Payment("bob", "pending", 999m),
                    };
                    r.Check("pivot per customer", () =>
                        Assert.Equal("ann:150/20,bob:10/0",
                            string.Join(",", Solution.Pivot(payments))));
                    r.Check("only refunds -> paid is 0", () =>
                    {
                        var p = new[] { new Payment("zoe", "refunded", 7m) };
                        Assert.Equal("zoe:0/7", string.Join(",", Solution.Pivot(p)));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Payment(string Customer, string Status, decimal Amount);

            public static class Solution
            {
                public static string[] Pivot(Payment[] payments) =>
                    payments.GroupBy(p => p.Customer)
                            .OrderBy(g => g.Key)
                            .Select(g =>
                            {
                                // Two filtered sums = two pivot "columns".
                                var paid = g.Where(p => p.Status == "paid").Sum(p => p.Amount);
                                var refunded = g.Where(p => p.Status == "refunded").Sum(p => p.Amount);
                                return $"{g.Key}:{paid}/{refunded}";
                            })
                            .ToArray();
            }
            """,
        Hints =
        [
            "GroupBy the customer, then compute two separate filtered Sums per group.",
            "Sum over an empty filtered sequence is 0 — that handles missing buckets.",
            "Statuses other than paid/refunded fall into neither sum and are ignored.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "pivot per customer", IsHidden = false },
            new TestCaseSeed { Name = "only refunds -> paid is 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed MultiKeySort => new()
    {
        Slug = "ef-multi-key-sort-page",
        Title = "Multi-Key Sort Then Skip/Take",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Sort employees by `Department` ascending, then by `Salary` **descending** to
            break ties, then by `Name` ascending as a final tie-breaker. Then return one
            page via `Skip((page-1)*size).Take(size)`. Output each as `"name"` in the sorted
            page order. This is `ORDER BY Dept, Salary DESC, Name` with `OFFSET/FETCH`.
            """,
        StarterCode =
            """
            using System.Linq;

            // PROVIDED — do not modify:
            public record Employee(string Name, string Department, decimal Salary);

            public static class Solution
            {
                // TODO: sort by Dept asc, Salary desc, Name asc; then page (1-based).
                public static string[] SortedPage(Employee[] employees, int pageNumber, int pageSize)
                {
                    return new string[0];
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
                    var employees = new[]
                    {
                        new Employee("ann", "eng", 100m),
                        new Employee("bob", "eng", 120m),
                        new Employee("cid", "eng", 100m),
                        new Employee("dan", "hr", 90m),
                    };
                    // sorted: bob(eng,120), ann(eng,100), cid(eng,100 name tiebreak), dan(hr,90)
                    r.Check("page 1 size 2", () =>
                        Assert.Equal("bob,ann",
                            string.Join(",", Solution.SortedPage(employees, 1, 2))));
                    r.Check("page 2 size 2", () =>
                        Assert.Equal("cid,dan",
                            string.Join(",", Solution.SortedPage(employees, 2, 2))));
                    r.Check("page past end -> empty", () =>
                        Assert.Equal("",
                            string.Join(",", Solution.SortedPage(employees, 3, 2))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public record Employee(string Name, string Department, decimal Salary);

            public static class Solution
            {
                public static string[] SortedPage(Employee[] employees, int pageNumber, int pageSize) =>
                    employees
                        .OrderBy(e => e.Department)          // ORDER BY Dept
                        .ThenByDescending(e => e.Salary)     //   , Salary DESC
                        .ThenBy(e => e.Name)                 //   , Name  (stable tie-break)
                        .Skip((pageNumber - 1) * pageSize)   // OFFSET
                        .Take(pageSize)                      // FETCH
                        .Select(e => e.Name)
                        .ToArray();
            }
            """,
        Hints =
        [
            "OrderBy sets the primary key; chain ThenBy / ThenByDescending for the rest.",
            "Salary descending uses ThenByDescending; Name ascending uses ThenBy.",
            "Apply Skip/Take AFTER sorting so paging follows the sort order.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "page 1 size 2", IsHidden = false },
            new TestCaseSeed { Name = "page 2 size 2", IsHidden = false },
            new TestCaseSeed { Name = "page past end -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed KeysetPagination => new()
    {
        Slug = "ef-keyset-pagination",
        Title = "Keyset (Seek) Pagination",
        Difficulty = "Hard",
        Kind = "Function",
        Prompt =
            """
            Implement keyset pagination. Given items each with a unique ascending `Id`,
            return the next `pageSize` items whose `Id` is **strictly greater** than
            `afterId`, ordered by `Id`. This is `WHERE Id > @afterId ORDER BY Id LIMIT
            @size` — no `OFFSET`, so it stays fast no matter how deep you page.

            For the very first page, callers pass `afterId = 0` (all Ids are positive).
            Return the selected `Id`s. If nothing is left, return an empty array.
            """,
        StarterCode =
            """
            using System.Linq;

            // PROVIDED — do not modify:
            public record Item(int Id, string Name);

            public static class Solution
            {
                // TODO: return up to pageSize Ids strictly greater than afterId, ordered.
                public static int[] NextPage(Item[] items, int afterId, int pageSize)
                {
                    return new int[0];
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
                    // deliberately unsorted input to prove we order by Id
                    var items = new[]
                    {
                        new Item(3, "c"), new Item(1, "a"), new Item(5, "e"),
                        new Item(2, "b"), new Item(4, "d"),
                    };
                    r.Check("first page after 0", () =>
                        Assert.Equal("1,2", string.Join(",", Solution.NextPage(items, 0, 2))));
                    r.Check("next page after 2", () =>
                        Assert.Equal("3,4", string.Join(",", Solution.NextPage(items, 2, 2))));
                    r.Check("last partial page after 4", () =>
                        Assert.Equal("5", string.Join(",", Solution.NextPage(items, 4, 2))));
                    r.Check("past the end -> empty", () =>
                        Assert.Equal("", string.Join(",", Solution.NextPage(items, 5, 2))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public record Item(int Id, string Name);

            public static class Solution
            {
                public static int[] NextPage(Item[] items, int afterId, int pageSize) =>
                    items.Where(i => i.Id > afterId)  // WHERE Id > @afterId (the "seek")
                         .OrderBy(i => i.Id)          // ORDER BY Id (unique, stable key)
                         .Take(pageSize)              // LIMIT @size — no OFFSET needed
                         .Select(i => i.Id)
                         .ToArray();
            }
            """,
        Hints =
        [
            "The bookmark is afterId: keep only rows with Id strictly greater than it.",
            "Order by the unique key BEFORE Take so the boundary is well-defined.",
            "No Skip is needed — that's the whole point; it stays O(pageSize).",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "first page after 0", IsHidden = false },
            new TestCaseSeed { Name = "next page after 2", IsHidden = false },
            new TestCaseSeed { Name = "last partial page after 4", IsHidden = false },
            new TestCaseSeed { Name = "past the end -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed TopNPerGroup => new()
    {
        Slug = "ef-top-n-per-group",
        Title = "Top-N Per Group",
        Difficulty = "Hard",
        Kind = "Function",
        Prompt =
            """
            Return the top `n` highest-scoring players **in each team**. Within a team, sort
            by `Score` descending, breaking ties by `Name` ascending, and keep at most `n`.
            Emit `"team:name"` for each kept player, with teams in alphabetical order and
            players in their in-team ranked order. This is `ROW_NUMBER() OVER (PARTITION BY
            Team ORDER BY Score DESC) <= n`.

            A team with fewer than `n` players keeps all of them.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Player(string Team, string Name, int Score);

            public static class Solution
            {
                // TODO: top n per team by Score desc (Name asc tiebreak); "team:name".
                public static string[] TopN(Player[] players, int n)
                {
                    return new string[0];
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
                    var players = new[]
                    {
                        new Player("red", "ann", 30),
                        new Player("red", "bob", 50),
                        new Player("red", "cid", 30),
                        new Player("blue", "dan", 10),
                        new Player("blue", "eve", 40),
                    };
                    // red top2: bob(50), ann(30, name tiebreak before cid)
                    // blue top2: eve(40), dan(10)
                    r.Check("top 2 per team", () =>
                        Assert.Equal("blue:eve,blue:dan,red:bob,red:ann",
                            string.Join(",", Solution.TopN(players, 2))));
                    r.Check("top 1 per team", () =>
                        Assert.Equal("blue:eve,red:bob",
                            string.Join(",", Solution.TopN(players, 1))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Player(string Team, string Name, int Score);

            public static class Solution
            {
                public static string[] TopN(Player[] players, int n) =>
                    players.GroupBy(p => p.Team)               // PARTITION BY Team
                           .OrderBy(g => g.Key)                // teams alphabetical
                           .SelectMany(g => g
                               .OrderByDescending(p => p.Score) // ORDER BY Score DESC
                               .ThenBy(p => p.Name)             //   , Name (tie-break)
                               .Take(n))                        // ROW_NUMBER() <= n
                           .Select(p => $"{p.Team}:{p.Name}")
                           .ToArray();
            }
            """,
        Hints =
        [
            "GroupBy the team to form partitions, then order the teams for stable output.",
            "Inside each group, OrderByDescending(Score).ThenBy(Name).Take(n).",
            "SelectMany flattens the per-team top lists back into one sequence.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "top 2 per team", IsHidden = false },
            new TestCaseSeed { Name = "top 1 per team", IsHidden = true },
        ],
    };

    private static ExerciseSeed AntiJoin => new()
    {
        Slug = "ef-anti-join",
        Title = "Anti-Join (Items With No Match)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the names of products that have **never** been sold — products whose `Id`
            appears in no sale. This is an anti-join: SQL `WHERE NOT EXISTS (SELECT 1 FROM
            Sales s WHERE s.ProductId = p.Id)`. Build a `HashSet` of sold product Ids once
            (O(1) lookups), then keep products not in it. Return names sorted alphabetically.

            Doing it as `products.Where(p => !sales.Any(s => s.ProductId == p.Id))` is the
            quadratic trap — the HashSet makes it linear.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public record Product(int Id, string Name);
            public record Sale(int Id, int ProductId);

            public static class Solution
            {
                // TODO: names of products with no matching sale, sorted A->Z.
                public static string[] NeverSold(Product[] products, Sale[] sales)
                {
                    return new string[0];
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
                    var products = new[]
                    {
                        new Product(1, "pen"),
                        new Product(2, "desk"),
                        new Product(3, "chair"),
                        new Product(4, "lamp"),
                    };
                    var sales = new[]
                    {
                        new Sale(10, 1), new Sale(11, 1), new Sale(12, 3),
                    };
                    r.Check("never-sold, sorted", () =>
                        Assert.Equal("desk,lamp",
                            string.Join(",", Solution.NeverSold(products, sales))));
                    r.Check("no sales -> all products", () =>
                        Assert.Equal("chair,desk,lamp,pen",
                            string.Join(",", Solution.NeverSold(products, new Sale[0]))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public record Product(int Id, string Name);
            public record Sale(int Id, int ProductId);

            public static class Solution
            {
                public static string[] NeverSold(Product[] products, Sale[] sales)
                {
                    // One-time set of matched keys => O(1) membership, linear overall.
                    var sold = sales.Select(s => s.ProductId).ToHashSet();

                    return products
                        .Where(p => !sold.Contains(p.Id))  // NOT EXISTS
                        .Select(p => p.Name)
                        .OrderBy(n => n)
                        .ToArray();
                }
            }
            """,
        Hints =
        [
            "Collect the ProductIds that DO appear in sales into a HashSet first.",
            "Keep products whose Id is NOT in that set (the anti-join).",
            "OrderBy the name for a deterministic, alphabetical result.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "never-sold, sorted", IsHidden = false },
            new TestCaseSeed { Name = "no sales -> all products", IsHidden = true },
        ],
    };
}
