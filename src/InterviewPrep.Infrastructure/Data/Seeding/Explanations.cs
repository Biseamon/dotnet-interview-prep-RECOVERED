namespace InterviewPrep.Infrastructure.Data.Seeding;

// Central "explain like I'm 5" learning material, one concise blurb per exercise slug.
// Kept in one place (rather than sprinkled through the content files) so the plain-English
// teaching can be written and tuned independently of the code/harness. The seeder attaches
// these by slug; the workspace shows them as a friendly "The idea" card above the problem.
public static class Explanations
{
    public static string? For(string slug) => Map.GetValueOrDefault(slug);

    private static readonly Dictionary<string, string> Map = new()
    {
        // ---- Async ----
        ["sum-async"] = "Some jobs take time, like waiting for water to boil. `async` lets your program start the wait and go do other things instead of standing there. Here you wait a tiny moment, then add two numbers.",
        ["concurrent-fetch"] = "If you have three kettles, boil them all at once instead of one after another. Start every task first, then wait for them together with `Task.WhenAll` — much faster than one at a time.",
        ["retry-async"] = "If a phone call drops, you try again a few times before giving up. Retrying wraps a flaky task so a failure gets another chance.",
        ["timeout-async"] = "You give someone 5 minutes to answer; if they don't, you move on. A timeout races the real work against a clock and stops waiting if the clock wins.",
        ["first-to-complete"] = "Ask three friends the same question and go with whoever answers first. `Task.WhenAny` gives you the fastest result and ignores the slower ones.",

        // ---- Arrays & Hashing ----
        ["two-sum"] = "You want two toys that together cost exactly $10. Instead of checking every pair, remember each price you've seen; when you see one, check if its partner is already in your memory.",
        ["contains-duplicate"] = "\"Did I already see this sticker?\" Keep a bag of what you've seen; if a new one is already in the bag, it's a duplicate.",
        ["valid-anagram"] = "Two words are anagrams if they use the exact same letters, just shuffled — like 'cat' and 'act'. Count each letter and check the counts match.",
        ["group-anagrams"] = "Sort each word's letters; words that become the same belong in one pile. Count the piles.",
        ["top-k-frequent"] = "Tally how many times each thing appears (like counting votes), then pick the few with the most.",

        // ---- Two Pointers & Sliding Window ----
        ["valid-palindrome"] = "A palindrome reads the same forwards and backwards, like 'racecar'. Put one finger on each end and walk inward, checking the letters match.",
        ["two-sum-sorted"] = "The list is already sorted small-to-big. Put a finger at each end: if their sum is too big, move the right finger left; too small, move the left finger right.",
        ["longest-substring-no-repeat"] = "Slide a window along the text, growing it while all letters differ; when a repeat sneaks in, shrink from the left until it's gone. Track the biggest window seen.",
        ["best-time-buy-sell-stock"] = "Buy low, sell high — but you must buy first. Walk through the prices, remember the cheapest day so far, and check today's profit against it.",

        // ---- Stacks ----
        ["valid-parentheses"] = "Every open bracket needs a matching close, like nesting boxes. Stack up the opens; each close must match the box on top.",
        ["min-stack"] = "A stack of plates where you can also instantly ask 'what's the smallest plate?'. Keep a second stack tracking the smallest-so-far.",
        ["eval-rpn"] = "A calculator where numbers come first and the operator comes last ('2 3 +'). Push numbers on a stack; on an operator, pop two and combine.",

        // ---- Linked Lists ----
        ["reverse-linked-list"] = "A train of cars each pointing to the next. To reverse it, flip each arrow to point backward as you walk down the train.",
        ["merge-two-sorted-lists"] = "Two sorted lines of people merging into one line — always let the smaller one step in next.",
        ["linked-list-cycle"] = "Two runners on a track, one fast, one slow. If the track secretly loops, the fast runner eventually laps and bumps into the slow one.",

        // ---- Trees ----
        ["max-depth-binary-tree"] = "How tall is the family tree? A branch's height is 1 plus the taller of its two children.",
        ["invert-binary-tree"] = "Hold the tree up to a mirror — swap every left and right child.",
        ["same-tree"] = "Are two trees identical twins? Check the tops match, then compare their left sides and right sides the same way.",
        ["binary-tree-level-order"] = "Read the tree floor by floor, left to right, like rows of seats. A queue visits each floor in order.",

        // ---- Binary Search ----
        ["binary-search"] = "Guess a number 1–100: always guess the middle, then throw away the half that can't hold it. Sorted lists let you halve the search each step.",
        ["search-rotated-sorted-array"] = "A sorted list got spun around at some point. One half is still sorted — figure out which, then search the right half.",
        ["kth-largest-element"] = "Find the 3rd biggest without fully sorting. Keep a small pile of the biggest few (a heap) and toss the rest.",

        // ---- Dynamic Programming ----
        ["climbing-stairs"] = "You hop 1 or 2 steps. Ways to reach a step = ways to the step below + ways to the one before that. It's the Fibonacci pattern.",
        ["house-robber"] = "Grab money from houses but never two next-door. At each house pick the better of: skip it, or take it plus what you saved two houses back.",
        ["coin-change"] = "Make an amount with the fewest coins. Build answers for every amount from 0 up, each time trying one more coin on top of a smaller answer.",
        ["longest-increasing-subsequence"] = "Find the longest run of numbers that keep going up (not necessarily next to each other). For each number, extend the best smaller run that ended before it.",

        // ---- Backtracking ----
        ["subsets"] = "List every possible team from a group — include or skip each person. Make a choice, go deeper, then undo it and try the other way.",
        ["permutations"] = "Every possible order to line people up. Pick who's next, recurse, then put them back and try someone else.",
        ["generate-parentheses"] = "Build all valid bracket arrangements. Add '(' while some remain, and ')' only when it won't go unbalanced.",

        // ---- Intervals ----
        ["merge-intervals"] = "Overlapping calendar events become one big block. Sort by start time, then stretch the current block whenever the next one overlaps.",
        ["meeting-rooms"] = "How many rooms so no two overlapping meetings clash? Count the most meetings happening at the same moment.",

        // ---- Graphs ----
        ["number-of-islands"] = "Land tiles touching each other form an island in a sea of water. Find a land tile, flood-fill its whole island so you don't count it twice, and tally the islands.",
        ["count-connected-components"] = "Groups of friends linked by friendship. Start at someone, visit everyone reachable, and each fresh start is a new group.",

        // ---- Design Patterns: Creational ----
        ["singleton"] = "Some things should exist only once — like a single TV remote for the house. A private door (constructor) plus one shared instance everyone uses.",
        ["factory-method"] = "A vending machine: you ask for 'circle' and it hands you the right shape — you don't build it yourself. A method decides which type to create.",
        ["builder"] = "Build a burger step by step — add cheese, add bacon — then serve. Each step returns the builder so you can chain them; Build() gives the finished thing.",
        ["abstract-factory"] = "A themed toy set: the 'dark' factory makes a matching dark button AND dark checkbox. One factory makes a whole family of related things.",
        ["prototype"] = "Photocopy an object instead of building a new one from scratch — but a real copy, so changing the copy doesn't touch the original.",

        // ---- Design Patterns: Structural ----
        ["adapter"] = "A travel plug adapter lets a European plug fit a US socket. Wrap an object so its shape fits what your code expects.",
        ["decorator"] = "Add toppings to coffee: milk, then sugar. Each wrapper adds a little without changing the coffee itself, and they stack.",
        ["facade"] = "One 'Start' button that quietly does all the messy startup steps behind the scenes. A simple front over a complicated system.",
        ["proxy"] = "A stand-in that answers for the real thing — here it remembers the answer so the slow real work only happens once.",
        ["composite"] = "A folder can hold files AND other folders. Treat one item and a group of items the same way, so trees just work.",
        ["bridge"] = "A TV and its remote are separate: swap either without breaking the other. Split what something IS from HOW it's done.",
        ["flyweight"] = "Instead of making a brand-new letter 'a' every time, share one and reuse it — saves lots of memory when things repeat.",

        // ---- Design Patterns: Behavioral ----
        ["strategy"] = "Pick how to travel — walk, bike, or drive — and swap it anytime. Interchangeable methods behind one common interface.",
        ["observer"] = "Subscribe to a channel: when it posts, all subscribers get notified. The subject tells everyone who signed up when something changes.",
        ["command"] = "Write each action on a card ('turn on light'). You can run cards, queue them, or undo the last — actions become objects.",
        ["state"] = "A traffic light behaves differently in each state and knows what comes next. The object changes its behavior as its state changes.",
        ["template-method"] = "A recipe fixes the steps but lets you choose the filling. The parent sets the skeleton; children fill in specific steps.",
        ["chain-of-responsibility"] = "A refund goes to the team lead; if too big, up to the manager, then the director. Each handler deals with it or passes it along.",
        ["mediator"] = "Instead of everyone texting everyone, they talk through one group chat. A middleman routes messages so the parts don't wire directly together.",
        ["iterator"] = "A remote's 'next' button steps through channels without you knowing how they're stored. Walk items one at a time.",
        ["memento"] = "A save point in a game: capture the current state so you can load it back later (undo).",
        ["visitor"] = "A guest who knows how to handle each type of room it enters. Add new operations to a set of types without changing those types.",
        ["interpreter"] = "Read a tiny language and work out what it means, like solving 'x AND (NOT y)'. Each grammar rule is a small piece that evaluates itself.",

        // ---- Multithreading ----
        ["thread-safe-counter"] = "Many hands adding to one jar can lose count if they bump. A lock or atomic bump makes each +1 happen cleanly, one at a time.",
        ["parallel-sum"] = "Split a big pile of numbers among helpers; each adds their part, then you combine the few subtotals — instead of everyone fighting over one total.",
        ["lazy-once-init"] = "Build the thing only when first needed, and make sure that even in a crowd it's built exactly once. `Lazy<T>` handles that.",
        ["semaphore-throttle"] = "A room with 3 keys: only 3 people inside at once, the rest wait for a key. A semaphore caps how many run at the same time.",
        ["concurrent-count"] = "Many workers tallying votes onto one shared sheet safely, without overwriting each other — that's `ConcurrentDictionary`.",

        // ---- Memory & GC ----
        ["idisposable-basic"] = "When you're done with something borrowed (like a library book), give it back. `Dispose` releases what you held; `using` returns it automatically.",
        ["dispose-guard"] = "Once you've returned the book you can't read it — asking should politely error. Guard against using something after it's disposed.",
        ["object-pool"] = "Reuse paper cups from a stack instead of making a new one each time — less garbage. Rent one, return it, rent it again.",
        ["value-semantics"] = "A struct is like a photocopy: change the copy and the original stays. A class is like a shared document: change it and everyone sees it.",
        ["stackalloc-sum"] = "Grab a tiny scratchpad on the stack for a moment's work — no leftover garbage to clean up afterward.",

        // ---- C# Language & Runtime ----
        ["linq-flatten"] = "Empty several small boxes into one big box, keeping the order. `SelectMany` merges lists-of-lists into one list.",
        ["linq-word-frequency"] = "Count how many times a word shows up, the tidy declarative way.",
        ["linq-distinct-sorted"] = "Remove duplicates and put things in order — like tidying a messy shelf.",
        ["classify-number"] = "Sort a number into 'negative', 'zero', or 'positive' with a neat `switch` instead of a pile of ifs.",
        ["shape-area-pattern"] = "Look at a shape and, depending on what kind it is, use the right area formula — pattern matching picks the branch for you.",
        ["generic-max"] = "One 'bigger of two' helper that works for numbers, words, anything comparable — write it once, use it everywhere.",
        ["generic-frequency"] = "Find the most common item in any kind of list using one reusable counter.",
        ["sum-span"] = "Add up numbers by looking at them in place, without copying them into a new box first — fast and allocation-free.",
        ["reverse-span"] = "Flip a list end-for-end right where it sits: swap the outer pair, then work inward.",

        // ---- ASP.NET Core Internals ----
        ["middleware-pipeline"] = "Like airport security lanes stacked in order: each step does something, passes you to the next, then can act again on your way out. That's how web requests flow.",
        ["di-lifetimes"] = "Who hands you your tools? A 'singleton' gives everyone the same one; 'transient' gives a brand-new one each time. This is dependency injection.",
        ["route-matching"] = "Match a web address like 'users/{id}' to the right page and pull out the id — like reading a labeled envelope.",
        ["model-validation"] = "Check a form before doing anything: name filled in? age sensible? Collect the problems and reject bad input early.",

        // ---- EF Core & Data ----
        ["ef-projection"] = "Grab only the columns you need (just names), not the whole row — like taking one item off a shelf instead of the whole shelf.",
        ["n-plus-one"] = "Don't run to the shop once per item on your list — make one trip. Fix the sneaky 'one query per row' habit by fetching together.",
        ["ef-pagination"] = "Show results one page at a time: skip the earlier pages, take this page's worth.",
        ["ef-group-aggregate"] = "Total up sales per category — group things by a key, then sum each group (SQL's GROUP BY).",

        // ---- SOLID ----
        ["srp-tax-calculator"] = "One tool, one job. A tax calculator only calculates tax — printing and saving live elsewhere.",
        ["ocp-total-area"] = "Add new shapes without touching the old code. Ask each shape its area; new shapes just work.",
        ["lsp-substitution"] = "A child type must be safe to use wherever the parent is expected — no nasty surprises when you swap it in.",
        ["isp-segregation"] = "Don't force a simple printer to carry 'scan' and 'fax' buttons it can't use. Prefer small, focused interfaces.",
        ["dip-inversion"] = "Plug into a socket (an interface), not one specific appliance. Depend on abstractions so you can swap the real thing — or a fake for testing.",

        // ---- Enterprise Patterns ----
        ["repository"] = "A tidy toy box with 'add', 'find by id', and 'get all' — it hides where the toys are really stored.",
        ["unit-of-work"] = "Fill your shopping cart, then check out once — all the changes save together, or not at all.",
        ["specification"] = "Write a rule as a reusable object ('even AND positive') and snap little rules together into bigger ones.",
        ["cqrs"] = "Separate the 'change it' buttons from the 'just show me' buttons. Commands write; queries read.",
        ["result-pattern"] = "Instead of throwing a tantrum (exception), hand back a neat 'success with value' or 'failure with reason' you can pass along.",
        ["circuit-breaker"] = "If a shop keeps failing you, stop knocking for a while so it can recover — fail fast instead of waiting every time.",
        ["retry-policy"] = "Try again a few times before giving up, for little hiccups that fix themselves.",

        // ---- System Design Building Blocks ----
        ["lru-cache"] = "A small shelf that fits only a few things; when full, toss whatever you used longest ago. Super-fast add and get.",
        ["ttl-cache"] = "Sticky notes that expire — after their time is up, they count as gone.",
        ["trie"] = "A word tree for autocomplete: share common beginnings so 'app' and 'apple' walk the same path.",
        ["min-heap"] = "A magic bag that always lets you pull out the smallest item quickly — the engine behind priority queues.",
        ["union-find"] = "Quickly answer 'are these two in the same group?' as groups keep merging — great for connections.",
        ["token-bucket"] = "A bucket refills with coins over time; each request spends one. No coins, no entry — that's rate limiting.",
        ["sliding-window-rate-limiter"] = "Count how many requests happened in the last few seconds; if it's under the limit, allow one more. Old requests slide out of the window over time.",
        ["ring-buffer"] = "A fixed row of slots that loops: when it's full, the newest entry writes over the oldest — like a security camera that only keeps the last hour.",
        ["consistent-hashing"] = "Place servers around a clock face; each key goes to the next server clockwise. Adding or removing one server only shuffles a few keys, not all of them.",
        ["leaderboard-topn"] = "Keep everyone's score, then show the highest few — like the top scores on an arcade machine.",

        // ---- Bit Manipulation ----
        ["single-number"] = "Everyone came with a twin except one person. XOR is a magic handshake where twins cancel out — whoever's left is the loner.",
        ["number-of-one-bits"] = "Count the 1s in a number's binary. The trick `n & (n-1)` erases the last 1 each time — count how many erases it takes.",
        ["counting-bits"] = "For every number up to n, count its 1-bits — reusing the answer for half the number plus its last bit.",
        ["missing-number"] = "You should have 0,1,2,…,n. Add up what you SHOULD have and subtract what you DO have — the gap is the missing one.",

        // ---- Arrays, Matrix & 2-D DP ----
        ["maximum-subarray"] = "Find the best winning streak in a row of gains and losses: keep a running total, and start over whenever it drops below zero.",
        ["product-except-self"] = "For each spot, multiply everything to its left by everything to its right — no division needed.",
        ["rotate-image"] = "Spin a grid a quarter-turn: first flip it along the diagonal, then mirror each row.",
        ["unique-paths"] = "A robot only moves right or down. The ways to reach a square = ways from above + ways from the left; fill the grid to the end.",

        // ---- Unit Testing ----
        ["fizzbuzz"] = "Say 'Fizz' for multiples of 3, 'Buzz' for 5, 'FizzBuzz' for both — the classic warm-up that shows you can handle simple rules and edge cases.",
        ["testable-clock"] = "Don't ask the real clock 'what time is it?' — let someone hand you the time. Then a test can pretend it's any moment.",
        ["boundary-validation"] = "Bugs love the edges. A valid age is 0 to 120, so test exactly -1, 0, 120, and 121.",
        ["fake-repository"] = "For tests, swap the real database for a pretend one that just uses a dictionary — fast and predictable.",
        ["spy-logger"] = "A spy quietly writes down every call it gets, so afterward a test can check 'was this logged?'.",

        // ---- Microservices ----
        ["saga-orchestrator"] = "A multi-step plan where, if a later step fails, you carefully undo the earlier steps in reverse — like unpacking a bag you just packed.",
        ["outbox-pattern"] = "Write the 'to-send' note in the same drawer as your data, then a helper mails the notes — so you never save the data but lose the message.",
        ["api-gateway-router"] = "One front desk that reads the address on each request and sends it to the right department (the longest-matching one wins).",
        ["service-registry"] = "A phone book of running services; when you ask for one, it hands out copies in turns so no single copy gets swamped.",

        // ---- Clean Code ----
        ["guard-clauses"] = "Deal with the bad/edge cases up front and return early, so the main logic isn't buried inside nested ifs.",
        ["return-boolean-directly"] = "If the answer is already true/false, just return the comparison — no need for `if (x) return true; else return false;`.",
        ["no-magic-numbers"] = "Give mystery numbers a name. `FreeShippingThreshold` tells the next reader what `50` actually means.",

        // ---- SQL ----
        ["sql-select-where"] = "SELECT picks columns; WHERE keeps only the rows you want — like asking a spreadsheet to show just the tall people.",
        ["sql-order-by"] = "ORDER BY sorts the results — smallest-first by default, or biggest-first with DESC.",
        ["sql-distinct"] = "DISTINCT throws away duplicate rows, leaving one of each — like a guest list with no repeats.",
        ["sql-aggregate"] = "Aggregate functions squish many rows into one number: COUNT (how many), SUM (total), AVG (average).",
        ["sql-inner-join"] = "A JOIN glues two tables together where a key matches — here, each order to the customer who placed it.",
        ["sql-left-join"] = "A LEFT JOIN keeps every row from the left table even if there's no match on the right (you get NULLs) — so customers with zero orders still show up.",
        ["sql-group-by"] = "GROUP BY makes one bucket per key and lets you total each bucket; HAVING then filters the buckets (WHERE filters rows, HAVING filters groups).",
        ["sql-subquery"] = "A subquery is a query inside a query — compute the average first, then compare each row against it.",

        // ---- DevOps & Containers ----
        ["dockerfile-dotnet"] = "A Dockerfile is a recipe for your app's box (image). Build it with the big SDK toolbox, then copy just the finished app into a small, light box to run — that keeps it fast and tidy.",
        ["github-actions-ci"] = "CI is a robot that, every time you push code, checks out your project, builds it, and runs the tests — so broken code is caught right away.",
        ["docker-compose"] = "docker-compose starts several boxes together with one command — here your web app and its database — and tells them how to find each other.",
        ["k8s-deployment"] = "A Deployment tells Kubernetes 'always keep 3 copies of this app running' and restarts any that die — like a manager keeping shifts staffed.",
        ["k8s-service"] = "A Service is a stable front desk with one address that spreads visitors across all the running copies of your app.",

        // ---- AI & LLM Engineering ----
        ["cosine-similarity"] = "Turn two things into arrows of numbers; cosine similarity measures how much they point the same way — 1 = twins, 0 = unrelated. It's how AI finds 'similar' text.",
        ["top-k-logits"] = "The model gives every possible next word a score. Top-K keeps just the few highest-scoring ones to pick from — like a shortlist.",
        ["text-chunking"] = "Long documents are cut into bite-size overlapping pieces so the AI can find and read the relevant bit. Overlap keeps ideas from being sliced in half.",
        ["prompt-template"] = "A fill-in-the-blanks letter: swap {{name}} and {{question}} for real values to build the prompt you send the model.",
        ["estimate-tokens"] = "Models read 'tokens', roughly 4 characters each. Estimating tokens tells you if your text fits in the model's window.",
        ["trim-history"] = "A chat can get too long to fit. Keep the newest messages that fit the token budget and drop the oldest — like a rolling notepad.",
        ["softmax"] = "Turn a bunch of raw scores into percentages that add up to 100% — bigger score, bigger slice of the pie.",
        ["greedy-decode"] = "At each step the model picks its single favorite next word, over and over, until it decides to stop.",
        ["keyword-search"] = "Pick the document that shares the most words with your question — the simplest way to 'search'.",
        ["whitespace-tokenizer"] = "Chop a sentence into words and swap each for a number the model understands; unknown words get a special 'I don't know this' number.",
        ["jaccard-similarity"] = "Compare two bags of words: shared words divided by total words. 1 = same, 0 = nothing in common — handy for spotting duplicates.",
        ["bloom-filter"] = "A tiny memory trick that can say 'definitely never seen it' or 'maybe seen it' using just a row of on/off switches — saves checking a giant list.",
        ["exponential-backoff"] = "If something fails, wait a bit; fail again, wait twice as long — so you don't pester a busy server. Cap the wait so it doesn't grow forever.",
        ["weighted-round-robin"] = "Share out work like dealing cards, but give bigger, stronger servers more cards each round.",
        ["event-sourcing-fold"] = "Instead of storing today's balance, store every deposit and withdrawal; replay them in order to get the balance — like adding up a receipt.",

        // ---- Architecture & Distributed Systems ----
        ["cache-aside"] = "Check your pocket before going to the store. If it's cached, use it; if not, fetch once and keep it for next time.",
        ["round-robin-balancer"] = "Deal work like cards around a table — server A, B, C, then back to A — so everyone shares evenly.",
        ["idempotent-consumer"] = "If the same letter arrives twice, act on it only once. Remember what you've handled so duplicates do nothing.",
        ["shard-router"] = "Split data across drawers by a rule (hash the key) so the same key always lands in the same drawer.",

        // ---- SQL (window functions & advanced) ----
        ["sql-self-join-hierarchy"] = "Join a table to itself so each person's row can look up their boss's name — one copy plays the employee, the other plays the manager.",
        ["sql-correlated-count"] = "For each person, run a mini count of how many others point to them as boss — the inner query peeks at the outer row, like asking 'how many people report to this exact person?'",
        ["sql-running-total"] = "Imagine a bank balance that updates with each transaction — a running total adds each row to the sum of all rows before it.",
        ["sql-running-total-partition"] = "Like a running total, but each region keeps its own separate tally that starts fresh, as if every region had its own bank account.",
        ["sql-top-n-per-group"] = "Rank the sales inside each region from biggest to smallest, then keep only the #1 in each group — the champion of every region.",
        ["sql-dedupe-latest"] = "When someone appears many times, sort their rows newest-first, number them, and keep just #1 so you're left with each person's most recent record.",
        ["sql-pivot-conditional"] = "Turn rows into side-by-side columns by summing amounts only when they belong to East (one column) or West (another) — reshaping tall data into wide.",
        ["sql-gaps-islands"] = "Consecutive numbers stay in step with their row count, so subtracting the row number gives them a shared label — that label groups each unbroken run into one island.",

        // ---- DevOps (production hardening) ----
        ["dockerfile-hardened"] = "A hardened image is a locked house, not just a built one — it runs as a non-root user, announces its port, and has a HEALTHCHECK so Docker can tell when it's sick.",
        ["dockerfile-buildargs"] = "Build args are dials you turn at build time (like the version), and LABELs are the sticker on the box — metadata that lets you trace a running image back to its source.",
        ["github-actions-matrix"] = "A matrix build is a for-loop over your CI: the same steps run across several OSes at once, while caching skips re-downloading packages and artifacts save the results.",
        ["k8s-deployment-probes"] = "Probes are Kubernetes taking your pod's pulse — liveness restarts a wedged container, readiness holds traffic until it's warmed up, and resource requests/limits keep one pod from eating the whole node.",
        ["k8s-config-secret"] = "ConfigMaps hold plain settings and Secrets hold credentials; both inject into the container as env vars so the image stays the same across every environment.",
        ["k8s-hpa"] = "An HPA is a thermostat for replicas — it watches CPU and adds or removes pods between a min and max so you scale with load instead of guessing.",
        ["dockerignore"] = "A .dockerignore is a bouncer for your build context — it keeps bin/obj, .git, and .env files out of the image so builds stay small and secrets don't leak.",
        ["compose-full-stack"] = "A full compose stack is a tiny production rehearsal — named volumes persist your data, healthchecks report readiness, and a shared network lets the services find each other by name.",

        // ---- EF Core & Data (more) ----
        ["ef-distinct-projection"] = "Pick just the column you care about first, then throw out repeats — so you get a clean list of unique cities instead of the same one over and over.",
        ["ef-n-plus-one-dto"] = "Instead of looking up each order's customer one-by-one (a trip to the database every time), you grab all customers once into a phone book and just glance at it — then stitch order + name into one tidy card.",
        ["ef-left-join-counts"] = "You want EVERY author listed, even the ones who wrote nothing. So you walk the author list and count their posts — folks with zero posts still show up with a 0 instead of vanishing.",
        ["ef-group-by-having"] = "First add up each category's total, THEN keep only the big ones. HAVING is a filter you apply after the adding-up, not before — that's the whole trick.",
        ["ef-conditional-aggregation"] = "For each customer you make two piggy banks — one for 'paid', one for 'refunded' — and drop each payment into the right one. One tidy row per person showing both totals.",
        ["ef-multi-key-sort-page"] = "Sort by department, then by highest pay, then by name to settle ties — like sorting a deck by suit, then rank, then color — and hand back just one page of the result.",
        ["ef-keyset-pagination"] = "Instead of counting 'skip 10000 rows' every time (slow), you remember the last item you saw and ask for 'the next ones after this' — like a bookmark.",
        ["ef-top-n-per-group"] = "Within each team, line players up by score and keep only the best few — like taking the top 2 medalists from every event, not overall.",
        ["ef-anti-join"] = "Make a guest list of every product that WAS sold, then hand back everyone NOT on that list — the products nobody ever bought.",

        // ---- ASP.NET Core Internals (more) ----
        ["aspnet-middleware-short-circuit"] = "Some middleware is a bouncer: if it handles the request itself, it doesn't call next() and everyone behind it in line just never gets to run.",
        ["aspnet-exception-middleware"] = "It's the top-of-pipeline safety net — it runs everything else inside a try/catch and turns any crash into a tidy status code like 400 or 404 instead of a splat.",
        ["aspnet-di-scopes"] = "Singletons are the one family car everyone shares forever, scoped things are a rental you keep for one trip (request), and transients are a fresh paper cup every single time you ask.",
        ["aspnet-route-precedence"] = "When two URL patterns both fit, the pickier one wins: an exact word beats a {blank} to fill in, which beats a catch-all that grabs whatever's left.",
        ["aspnet-model-validation-rules"] = "Instead of stopping at the first mistake, it checks every field and hands back the whole list of what's wrong so you can fix it all in one go.",
        ["aspnet-endpoint-filter-chain"] = "Filters are gift-wrap around your handler: each one can peek at the input, say no early, or add a bow to the answer on the way back out.",
        ["aspnet-content-negotiation"] = "The client says 'I'd like JSON, or XML if not' and the server hands over the first one on that wish-list it actually knows how to make.",

        // ---- Async (more) ----
        ["cancellable-delay"] = "Learn to politely stop your work the moment someone hits the cancel button.",
        ["retry-with-backoff"] = "When something keeps failing, try again a few times, waiting a little longer each time.",
        ["whenall-first-error"] = "Do lots of jobs at once, add up the answers, and speak up if any job blows up.",
        ["bounded-parallelism"] = "Run many chores together, but only let a handful happen at the same moment.",
        ["channel-producer-consumer"] = "One helper fills a conveyor belt with numbers while another adds them all up.",

        // ---- Multithreading (more) ----
        ["double-checked-lazy"] = "Peek first without locking, and only lock-and-build if nobody built it yet, so it's made just once.",
        ["thread-local-aggregate"] = "Each helper keeps its own running total and they add their piles together at the very end.",
        ["concurrent-memoize"] = "Remember each answer the first time you work it out so you never do the same math twice.",
        ["reader-writer-cache"] = "Lots of people can read the shelf at once, but writing requires the whole shelf to yourself.",
        ["bounded-resource-pool"] = "Only a fixed number of tickets exist, so only that many people can use the ride at the same time.",
        ["deadlock-free-transfer"] = "Always grab the two piggy banks in the same fixed order so two kids never freeze waiting on each other.",

        // ---- Memory & GC (more) ----
        ["object-pool-reset"] = "Wipe a borrowed toy clean before the next kid takes it, so nobody gets leftover mess.",
        ["dispose-chain"] = "One box that, when you close it, closes every box inside it exactly once.",
        ["array-pool-buffer"] = "Borrow a scratch pad from a shared pile, use it, and always give it back.",
        ["weakref-cache"] = "A sticky note you can find while you hold the thing, but that vanishes once you let go.",
        ["boxing-trap"] = "Putting a number in a gift box costs you a box; using it plain costs nothing.",
        ["span-csv-parser"] = "Read numbers out of a sentence by pointing at each part instead of copying it.",
        ["ref-struct-enumerator"] = "Walk along a row of numbers without ever picking the whole row up.",

        // ---- Testing (more) ----
        ["parameterized-cases"] = "One clamp function, a whole table of tricky inputs — check all the edges at once, like a spelling test with every hard word.",
        ["token-expiry-clock"] = "Give your code a pretend clock so you can jump to any moment and check if the ticket is still good — no waiting an hour.",
        ["stub-rate-provider"] = "Instead of asking the internet for today's rate, hand your code a pretend rate so the answer is always the same.",
        ["characterization-test"] = "Copy exactly what the old messy code already does, quirks and all, so you can safely change it later without breaking anything.",
        ["mock-verify-call"] = "Build a fake helper that writes down every time it's poked, then check your code poked it the right number of times with the right message.",
        ["state-vs-interaction"] = "Prove a signup worked two ways — the user is now in the list (state) AND the welcome email got sent exactly once (interaction).",

        // ---- SOLID (more) ----
        ["srp-split-report"] = "One class was doing two chores, so we cut it into two little helpers that each do just one thing.",
        ["ocp-strategy-discount"] = "Instead of editing the price machine for every new deal, you hand it deal-cards it just plays in order.",
        ["ocp-handler-pipeline"] = "A conveyor belt of little workers; adding a new worker never means rebuilding the belt.",
        ["lsp-rectangle-square"] = "A square shouldn't pretend to be a stretchy rectangle, so we let both just be shapes that can tell you their area.",
        ["dip-order-policy"] = "The order boss doesn't own a real database or phone — you hand it toy ones, so it's easy to test.",
        ["composition-over-inheritance"] = "Instead of a new duck breed for every sound, one duck holds a sound-gadget you can swap anytime.",

        // ---- Architecture & Distributed Systems (more) ----
        ["anti-corruption-layer"] = "A translator at the border: it turns the foreign country's money and words into yours, so your side never has to learn their language.",
        ["hexagonal-ports-adapters"] = "Your game works the same whether you plug in a real controller or a keyboard — because it only talks to the 'plug' shape, not the device.",
        ["arch-event-sourcing-fold"] = "Instead of keeping your bank balance, keep the list of every deposit and withdrawal; add them all up any time to get today's number.",
        ["cqrs-projection"] = "You write things down as they happen, and separately keep a tidy scoreboard so anyone can read the answer instantly.",
        ["idempotency-key-store"] = "Stamp each order with a ticket number; if the same ticket comes back, hand over the receipt you already made instead of doing it twice.",
        ["bounded-queue-backpressure"] = "A bucket that only holds so much — when it's full it says 'no more' instead of overflowing all over the floor.",
        ["outbox-relay"] = "Write the letter and drop it in your outbox in one move; the mail carrier grabs unsent letters later and checks them off, so none get lost or sent twice.",
        ["saga-compensation"] = "A recipe where, if a step burns, you carefully undo the earlier steps in reverse so the kitchen ends up clean.",

        // ---- Microservices (more) ----
        ["correlation-id-propagation"] = "Give every request a sticker so you can follow it through every service it visits.",
        ["api-composition-aggregator"] = "Ask several services one question each, then staple their answers into one reply.",
        ["bulkhead-isolation"] = "Give each dependency its own small bucket of slots so one clog can't drain the whole sink.",
        ["retry-budget-token-bucket"] = "You get a jar of retry coins; when the coins run out, you stop trying again.",
        ["ms-idempotent-consumer"] = "Remember which messages you've handled so doing the same one twice only counts once.",
        ["ms-circuit-breaker"] = "After a friend keeps failing, stop calling for a while, then test once before trusting them again.",
        ["outbox-relay-effectively-once"] = "Even if the mailman drops the same letter twice, the reader only acts on it once.",

        // ---- Enterprise Patterns (more) ----
        ["specification-composition"] = "Snap yes/no rule blocks together with AND, OR, and NOT to sift a list.",
        ["unit-of-work-rollback"] = "Save all your changes at once, or none — one bad item cancels the whole batch.",
        ["mediator-dispatch"] = "Drop a request in a slot and the mailroom finds its one matching worker.",
        ["caching-decorator"] = "Remember an answer the first time so you never ask the slow guy twice.",
        ["pipeline-behaviors"] = "Pass a request through a line of guards, each wrapping the next, before it reaches the worker.",
        ["outbox-dispatch"] = "Only mail the letters after the deal is truly signed — cancel and nothing gets sent.",

        // ---- Clean Code (more) ----
        ["simplify-boolean-de-morgan"] = "Untangle a double-negative condition using De Morgan's law so it reads as the plain rule: allowed only when not banned and paid.",
        ["rename-and-flatten"] = "Take a deeply nested if/else pyramid and flatten it with guard clauses that return early, so the main path reads straight down.",
        ["decompose-god-method"] = "Chop one giant do-everything method into small named steps (validate, subtotal, discount, tax) that each do one job.",
        ["command-query-separation"] = "A method should either change something or answer something, not both — split the mutate-and-return into Increment() and a read-only Value.",
        ["encapsulate-data-clump"] = "Two values that always travel together become a small type that does its own math, instead of every caller reaching in.",
        ["extract-pricing-policy"] = "Pull magic-number ticket pricing into a clean policy with named boundaries and tier prices, careful about the age edges.",
        ["replace-flag-argument"] = "Format(text, true) tells you nothing — split the boolean-switch method into two clearly named methods, Shout and Whisper.",
        ["replace-switch-with-lookup"] = "Turn a long switch on direction words into a dictionary lookup, so adding a case is adding an entry, not editing a branch.",
        ["remove-duplication-dry"] = "The same fee formula was copy-pasted three times — write it once and look up the only thing that differs, the rate.",
        ["tell-dont-ask"] = "Instead of yanking fields out of an order to judge it, put the rules on the order itself so it knows if it's settled or needs a reminder.",

        // ---- C# Language & Runtime (more) ----
        ["record-with-expression"] = "`with` makes a fresh copy of a record with one field changed, leaving the original alone.",
        ["pattern-state-machine"] = "A switch over (state, event) is a clean transition table for a little state machine.",
        ["comparable-semver-struct"] = "Teach a struct to compare itself (CompareTo + <, >) so Array.Sort and Max just work.",
        ["span-csv-sum"] = "Add up comma-separated numbers by sliding a view over the text, never allocating a substring.",
        ["linq-lazy-batch"] = "`yield return` hands out chunks one at a time and only does work when the consumer asks.",
    };
}
