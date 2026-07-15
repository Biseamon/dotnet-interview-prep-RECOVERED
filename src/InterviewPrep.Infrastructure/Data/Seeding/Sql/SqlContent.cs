using System.Text.Json;

namespace InterviewPrep.Infrastructure.Data.Seeding.Sql;

// The "SQL" topic — real, runnable exercises. The learner writes SQL; the SqlRunner
// executes it against a fresh in-memory SQLite database (seeded per exercise) and
// compares the result set to the reference query's. Each exercise's harness is a JSON
// spec { setup, solution, ordered } built by the SqlHarness helper (no hand-escaping).
internal static class SqlContent
{
    // Shared sample data used by the querying-basics lesson.
    private const string Employees =
        """
        CREATE TABLE employees (id INTEGER, name TEXT, department TEXT, salary INTEGER);
        INSERT INTO employees VALUES
          (1,'Ada','Engineering',95000),
          (2,'Bea','Engineering',60000),
          (3,'Cid','Sales',45000),
          (4,'Dee','Sales',52000),
          (5,'Eli','Marketing',48000);
        """;

    // Shared customers/orders used by the joins lesson.
    private const string Shop =
        """
        CREATE TABLE customers (id INTEGER, name TEXT);
        CREATE TABLE orders (id INTEGER, customer_id INTEGER, amount INTEGER);
        INSERT INTO customers VALUES (1,'Ann'),(2,'Bob'),(3,'Cara');
        INSERT INTO orders VALUES (10,1,50),(11,1,80),(12,2,120),(13,2,30);
        """;

    private static string SqlHarness(string setup, string solution, bool ordered = false) =>
        JsonSerializer.Serialize(new { setup, solution, ordered });

    // Builds a SQL exercise from its parts (keeps each definition short & readable).
    private static ExerciseSeed Sql(string slug, string title, string difficulty, string prompt,
        string starter, string setup, string solution, bool ordered, string[] hints, string[] visibleTests) => new()
    {
        Slug = slug, Title = title, Difficulty = difficulty, Kind = "Function", Language = "Sql",
        TimeoutSeconds = 5, Prompt = prompt, StarterCode = starter,
        HarnessCode = SqlHarness(setup, solution, ordered),
        ReferenceSolution = solution,
        Hints = hints.ToList(),
        TestCases = visibleTests.Select(t => new TestCaseSeed { Name = t, IsHidden = false }).ToList(),
    };

    public static TopicSeed Topic => new()
    {
        Slug = "sql",
        Name = "SQL",
        Description = "Real SQL you write and run: SELECT/WHERE, ORDER BY, JOINs, GROUP BY, and subqueries — graded against a live database.",
        Order = 13,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "sql-querying-basics", Title = "Querying Basics", Order = 1,
                MarkdownContent =
                    """
                    ## Querying Basics

                    `SELECT columns FROM table WHERE condition`. `WHERE` filters rows,
                    `ORDER BY` sorts, `DISTINCT` removes duplicates, and aggregate functions
                    (`COUNT`, `SUM`, `AVG`) summarize. These exercises run against a small
                    `employees` table — write the query, hit Run, and your result set is
                    compared to the correct one.
                    """,
                Exercises =
                [
                    Sql("sql-select-where", "SELECT with WHERE", "Easy",
                        "Return the **names** of employees earning more than 50000.",
                        "-- Return names of employees with salary > 50000\nSELECT name FROM employees WHERE ...;",
                        Employees, "SELECT name FROM employees WHERE salary > 50000;", false,
                        ["Use `SELECT name FROM employees`.", "Add `WHERE salary > 50000`."],
                        ["names of high earners"]),

                    Sql("sql-order-by", "ORDER BY", "Easy",
                        "Return all employee **names**, sorted by **salary, highest first**.",
                        "-- All names, highest salary first\nSELECT name FROM employees ORDER BY ...;",
                        Employees, "SELECT name FROM employees ORDER BY salary DESC;", true,
                        ["`ORDER BY salary` sorts ascending by default.", "Add `DESC` for highest first."],
                        ["names ordered by salary desc"]),

                    Sql("sql-distinct", "DISTINCT", "Easy",
                        "List each **department** exactly once (no duplicates).",
                        "-- Each department once\nSELECT DISTINCT ... FROM employees;",
                        Employees, "SELECT DISTINCT department FROM employees;", false,
                        ["`DISTINCT` removes duplicate rows.", "`SELECT DISTINCT department FROM employees`."],
                        ["distinct departments"]),

                    Sql("sql-aggregate", "COUNT (aggregate)", "Medium",
                        "Return how many employees are in the **Engineering** department, as a single value in a column named `count`.",
                        "-- Count Engineering employees, column named count\nSELECT COUNT(*) AS count FROM employees WHERE ...;",
                        Employees, "SELECT COUNT(*) AS count FROM employees WHERE department = 'Engineering';", false,
                        ["`COUNT(*)` counts rows.", "Filter with `WHERE department = 'Engineering'`.", "Alias it: `AS count`."],
                        ["count of Engineering employees"]),
                ],
            },
            new LessonSeed
            {
                Slug = "sql-joins-grouping", Title = "Joins & Grouping", Order = 2,
                MarkdownContent =
                    """
                    ## Joins & Grouping

                    A `JOIN` combines rows from two tables on a matching key. `INNER JOIN`
                    keeps only matches; `LEFT JOIN` keeps all left rows (NULLs where none
                    match). `GROUP BY` collapses rows into groups you summarize with
                    aggregates, and `HAVING` filters those groups. Tables: `customers` and
                    `orders` (with a `customer_id`).
                    """,
                Exercises =
                [
                    Sql("sql-inner-join", "INNER JOIN", "Medium",
                        "For every order, return the customer **name** and the order **amount** (columns `name`, `amount`).",
                        "-- Join orders to customers\nSELECT c.name, o.amount\nFROM orders o\nJOIN customers c ON ...;",
                        Shop, "SELECT c.name, o.amount FROM orders o JOIN customers c ON c.id = o.customer_id;", false,
                        ["Join on `customers.id = orders.customer_id`.", "Select `c.name, o.amount`."],
                        ["each order's customer + amount"]),

                    Sql("sql-left-join", "LEFT JOIN", "Medium",
                        "Return **every** customer's `name` and their number of orders as `orders` (0 for customers with none). Hint: LEFT JOIN + GROUP BY, and COUNT the order id.",
                        "-- Every customer + their order count (0 if none)\nSELECT c.name, COUNT(o.id) AS orders\nFROM customers c\nLEFT JOIN orders o ON ...\nGROUP BY c.id, c.name;",
                        Shop, "SELECT c.name, COUNT(o.id) AS orders FROM customers c LEFT JOIN orders o ON o.customer_id = c.id GROUP BY c.id, c.name;", false,
                        ["LEFT JOIN keeps customers with no orders.", "COUNT(o.id) counts only real orders (NULLs don't count).", "GROUP BY the customer."],
                        ["every customer + order count"]),

                    Sql("sql-group-by", "GROUP BY + HAVING", "Medium",
                        "Return each customer's `name` and their **total** order amount as `total`, but only customers whose total is **greater than 100**.",
                        "-- Customers whose total spend > 100\nSELECT c.name, SUM(o.amount) AS total\nFROM customers c\nJOIN orders o ON o.customer_id = c.id\nGROUP BY c.id, c.name\nHAVING ...;",
                        Shop, "SELECT c.name, SUM(o.amount) AS total FROM customers c JOIN orders o ON o.customer_id = c.id GROUP BY c.id, c.name HAVING SUM(o.amount) > 100;", false,
                        ["SUM(o.amount) per customer needs GROUP BY.", "Filter groups with HAVING, not WHERE.", "`HAVING SUM(o.amount) > 100`."],
                        ["big spenders (> 100 total)"]),

                    Sql("sql-subquery", "Subquery", "Medium",
                        "Return the **names** of employees who earn **more than the average** salary. Use a subquery for the average.",
                        "-- Above-average earners\nSELECT name FROM employees WHERE salary > (SELECT ... FROM employees);",
                        Employees, "SELECT name FROM employees WHERE salary > (SELECT AVG(salary) FROM employees);", false,
                        ["`(SELECT AVG(salary) FROM employees)` computes the average.", "Compare each salary against it in the WHERE clause."],
                        ["above-average earners"]),
                ],
            },
        ],
    };
}
