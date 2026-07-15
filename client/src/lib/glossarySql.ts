import type { Term } from './glossary'

// SQL keywords and concepts, plain-English, with tiny examples.
export const GLOSSARY_SQL: Term[] = [
  // Statements
  { term: 'SELECT', category: 'Statements', definition: 'Reads (queries) rows from tables.', example: 'SELECT name FROM users;' },
  { term: 'FROM', category: 'Statements', definition: 'Names the table(s) a query reads from.', example: 'FROM orders' },
  { term: 'WHERE', category: 'Statements', definition: 'Filters individual rows by a condition.', example: "WHERE status = 'paid'" },
  { term: 'ORDER BY', category: 'Statements', definition: 'Sorts results; ASC (default) or DESC.', example: 'ORDER BY created DESC' },
  { term: 'GROUP BY', category: 'Statements', definition: 'Collapses rows into groups to aggregate.', example: 'GROUP BY category' },
  { term: 'HAVING', category: 'Statements', definition: 'Filters GROUP BY groups (WHERE filters rows).', example: 'HAVING COUNT(*) > 5' },
  { term: 'DISTINCT', category: 'Statements', definition: 'Removes duplicate rows from the result.', example: 'SELECT DISTINCT city' },
  { term: 'LIMIT / OFFSET', category: 'Statements', definition: 'Return only some rows (paging).', example: 'LIMIT 10 OFFSET 20' },
  { term: 'INSERT', category: 'Statements', definition: 'Adds new rows.', example: 'INSERT INTO t (a) VALUES (1);' },
  { term: 'UPDATE', category: 'Statements', definition: 'Changes existing rows (use WHERE!).', example: 'UPDATE t SET a = 2 WHERE id = 1;' },
  { term: 'DELETE', category: 'Statements', definition: 'Removes rows (use WHERE!).', example: 'DELETE FROM t WHERE id = 1;' },

  // Joins
  { term: 'INNER JOIN', category: 'Joins', definition: 'Keeps only rows matching in both tables.', example: 'a JOIN b ON a.id = b.a_id' },
  { term: 'LEFT JOIN', category: 'Joins', definition: 'Keeps all left rows; NULLs where no right match.', example: 'a LEFT JOIN b ON …' },
  { term: 'RIGHT / FULL JOIN', category: 'Joins', definition: 'Mirror of LEFT / keep rows from both sides.' },
  { term: 'CROSS JOIN', category: 'Joins', definition: 'Every combination of both tables (Cartesian product).' },
  { term: 'ON', category: 'Joins', definition: 'The condition two tables join on.', example: 'ON a.id = b.a_id' },

  // Aggregates
  { term: 'COUNT / SUM / AVG', category: 'Aggregates', definition: 'Summarize many rows into one value.', example: 'SELECT AVG(price)' },
  { term: 'MIN / MAX', category: 'Aggregates', definition: 'Smallest / largest value in a group.' },

  // Schema & constraints
  { term: 'PRIMARY KEY', category: 'Schema', definition: 'Uniquely identifies each row (unique + not null).' },
  { term: 'FOREIGN KEY', category: 'Schema', definition: 'A column referencing another table\'s key (a relationship).' },
  { term: 'UNIQUE', category: 'Schema', definition: 'No two rows may share this value.' },
  { term: 'INDEX', category: 'Schema', definition: 'A lookup structure that speeds reads (costs write time + space).' },
  { term: 'Clustered vs non-clustered index', category: 'Schema', definition: 'Clustered = the physical row order (one per table). Non-clustered = a separate structure pointing to rows (many allowed).' },

  // Concepts
  { term: 'Transaction', category: 'Concepts', definition: 'A unit of work that fully commits or fully rolls back.', example: 'BEGIN; …; COMMIT;' },
  { term: 'ACID', category: 'Concepts', definition: 'Atomicity, Consistency, Isolation, Durability — the guarantees of a reliable transaction.' },
  { term: 'Normalization', category: 'Concepts', definition: 'Structuring tables to remove redundancy (1NF/2NF/3NF).' },
  { term: 'Subquery', category: 'Concepts', definition: 'A query nested inside another query.', example: 'WHERE x > (SELECT AVG(x) …)' },
  { term: 'CTE (WITH)', category: 'Concepts', definition: 'A named temporary result you can reference, improving readability.', example: 'WITH t AS (…) SELECT …' },
  { term: 'View', category: 'Concepts', definition: 'A saved query you can query like a table.' },
  { term: 'Window function', category: 'Concepts', definition: 'Computes across related rows without collapsing them.', example: 'ROW_NUMBER() OVER (…)' },
  { term: 'Isolation level', category: 'Concepts', definition: 'How much concurrent transactions can see of each other (Read Committed, Serializable, …).' },
]
