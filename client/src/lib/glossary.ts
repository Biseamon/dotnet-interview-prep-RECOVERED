// A searchable glossary of C# keywords and terminology — plain-English definitions
// with tiny examples. Pure reference content (no backend needed). Grouped by category.
export interface Term {
  term: string
  category: string
  definition: string
  example?: string
}

export const GLOSSARY: Term[] = [
  // ---- Access modifiers (who can see this?) ----
  { term: 'public', category: 'Access modifiers', definition: 'Visible to everyone, anywhere.', example: 'public int Age;' },
  { term: 'private', category: 'Access modifiers', definition: 'Visible only inside the same class. The default for class members.', example: 'private int _count;' },
  { term: 'protected', category: 'Access modifiers', definition: 'Visible inside the class and its subclasses.', example: 'protected void Init() {}' },
  { term: 'internal', category: 'Access modifiers', definition: 'Visible anywhere in the same project (assembly), but not outside it.', example: 'internal class Helper {}' },
  { term: 'protected internal', category: 'Access modifiers', definition: 'Visible in the same assembly OR to subclasses anywhere.' },
  { term: 'private protected', category: 'Access modifiers', definition: 'Visible only to subclasses within the same assembly.' },
  { term: 'file', category: 'Access modifiers', definition: 'Type is visible only within the same source file (C# 11+).', example: 'file class Internal {}' },

  // ---- Type kinds ----
  { term: 'class', category: 'Types', definition: 'A reference type: a blueprint for objects with data and behavior. Copies share the same object.', example: 'class Car { public int Wheels; }' },
  { term: 'struct', category: 'Types', definition: 'A value type: copied when assigned or passed. Best for small, immutable data.', example: 'struct Point { public int X, Y; }' },
  { term: 'interface', category: 'Types', definition: 'A contract of members a type must implement — no data, just the shape.', example: 'interface IShape { double Area(); }' },
  { term: 'enum', category: 'Types', definition: 'A named set of constant values.', example: 'enum Day { Mon, Tue, Wed }' },
  { term: 'record', category: 'Types', definition: 'A class (or struct) built for data: value-based equality and easy copying with `with`.', example: 'record Person(string Name);' },
  { term: 'delegate', category: 'Types', definition: 'A type that holds a reference to a method — a "function pointer".', example: 'delegate int Op(int a, int b);' },

  // ---- Modifiers ----
  { term: 'static', category: 'Modifiers', definition: 'Belongs to the type itself, not to any instance. One shared copy.', example: 'static int Count;' },
  { term: 'sealed', category: 'Modifiers', definition: 'Prevents a class from being inherited (or an override from being overridden further).', example: 'sealed class Final {}' },
  { term: 'abstract', category: 'Modifiers', definition: 'Incomplete on purpose: cannot be instantiated; subclasses must fill in the missing parts.', example: 'abstract class Shape { abstract double Area(); }' },
  { term: 'virtual', category: 'Modifiers', definition: 'Marks a member that subclasses are allowed to override.', example: 'virtual void Speak() {}' },
  { term: 'override', category: 'Modifiers', definition: 'Replaces a virtual/abstract member from the base class.', example: 'override void Speak() {}' },
  { term: 'partial', category: 'Modifiers', definition: 'Splits a class/method across multiple files that the compiler merges into one.', example: 'partial class Big {}' },
  { term: 'readonly', category: 'Modifiers', definition: 'A field that can only be set at declaration or in the constructor — never changed after.', example: 'readonly int _id;' },
  { term: 'const', category: 'Modifiers', definition: 'A compile-time constant that never changes.', example: 'const double Pi = 3.14159;' },
  { term: 'new (modifier)', category: 'Modifiers', definition: 'Hides an inherited member with a new one (different from overriding).', example: 'new void Draw() {}' },
  { term: 'volatile', category: 'Modifiers', definition: 'Tells the compiler a field may change across threads; reads/writes are not reordered.', example: 'volatile bool _stop;' },
  { term: 'unsafe', category: 'Modifiers', definition: 'Allows pointer operations that skip safety checks.', example: 'unsafe { int* p = &x; }' },
  { term: 'extern', category: 'Modifiers', definition: 'The method is implemented externally (e.g., native code via P/Invoke).' },

  // ---- Members & parameters ----
  { term: 'void', category: 'Members & parameters', definition: 'The method returns nothing.', example: 'void Log(string m) {}' },
  { term: 'return', category: 'Members & parameters', definition: 'Sends a value back from a method and exits it.', example: 'return a + b;' },
  { term: 'params', category: 'Members & parameters', definition: 'Accepts a variable number of arguments as an array.', example: 'void Sum(params int[] nums) {}' },
  { term: 'ref', category: 'Members & parameters', definition: 'Passes a variable by reference, so the method can change the caller’s value.', example: 'void Add(ref int x) { x++; }' },
  { term: 'out', category: 'Members & parameters', definition: 'Like ref, but the method must assign it; used to return extra values.', example: 'int.TryParse(s, out int n);' },
  { term: 'in (parameter)', category: 'Members & parameters', definition: 'Passes by reference but read-only (an optimization for big structs).', example: 'void Use(in BigStruct s) {}' },
  { term: 'this', category: 'Members & parameters', definition: 'Refers to the current instance. Also marks an extension method’s first parameter.', example: 'this.Name = name;' },
  { term: 'base', category: 'Members & parameters', definition: 'Refers to the parent class — call its constructor or members.', example: 'base.Speak();' },
  { term: 'required', category: 'Members & parameters', definition: 'A property that MUST be set when the object is created (C# 11+).', example: 'required string Name { get; init; }' },

  // ---- Properties & accessors ----
  { term: 'get', category: 'Properties', definition: 'The read part of a property.', example: 'public int X { get; }' },
  { term: 'set', category: 'Properties', definition: 'The write part of a property.', example: 'public int X { get; set; }' },
  { term: 'init', category: 'Properties', definition: 'A setter usable only during object creation — then it’s locked.', example: 'public int X { get; init; }' },
  { term: 'value', category: 'Properties', definition: 'The implicit parameter inside a setter holding the assigned value.', example: 'set { _x = value; }' },

  // ---- Control flow ----
  { term: 'if / else', category: 'Control flow', definition: 'Run code when a condition is true (or otherwise).', example: 'if (x > 0) {...} else {...}' },
  { term: 'switch', category: 'Control flow', definition: 'Branch on a value across many cases; also a powerful expression form.', example: 'x switch { 0 => "zero", _ => "other" }' },
  { term: 'case / default', category: 'Control flow', definition: 'A branch of a switch; `default` is the fallback.' },
  { term: 'for', category: 'Control flow', definition: 'Loop with an index/counter.', example: 'for (int i = 0; i < n; i++) {}' },
  { term: 'foreach', category: 'Control flow', definition: 'Loop over each item in a collection.', example: 'foreach (var x in list) {}' },
  { term: 'while / do', category: 'Control flow', definition: 'Loop while a condition holds (`do` checks after the first run).', example: 'while (cond) {}' },
  { term: 'break', category: 'Control flow', definition: 'Exit the nearest loop or switch immediately.' },
  { term: 'continue', category: 'Control flow', definition: 'Skip to the next iteration of the loop.' },
  { term: 'yield', category: 'Control flow', definition: 'Produces items one at a time to build an iterator lazily.', example: 'yield return i;' },

  // ---- Exceptions ----
  { term: 'try / catch / finally', category: 'Exceptions', definition: 'Run risky code; catch errors; `finally` always runs (cleanup).', example: 'try {...} catch (Ex e) {...} finally {...}' },
  { term: 'throw', category: 'Exceptions', definition: 'Raise an exception to signal an error.', example: 'throw new ArgumentException();' },
  { term: 'when (filter)', category: 'Exceptions', definition: 'A condition on a catch — only catch if it’s true.', example: 'catch (Ex e) when (e.Code == 5)' },

  // ---- Values & operators ----
  { term: 'var', category: 'Values & operators', definition: 'Let the compiler infer the variable’s type from the right-hand side.', example: 'var list = new List<int>();' },
  { term: 'null', category: 'Values & operators', definition: 'The absence of a value for a reference type.', example: 'string? s = null;' },
  { term: 'true / false', category: 'Values & operators', definition: 'The two boolean values.' },
  { term: 'new', category: 'Values & operators', definition: 'Creates an instance of a type.', example: 'new Car();' },
  { term: 'is', category: 'Values & operators', definition: 'Tests a value’s type, often with pattern matching.', example: 'if (o is int n) {...}' },
  { term: 'as', category: 'Values & operators', definition: 'Casts to a type, returning null instead of throwing on failure.', example: 'var c = o as Car;' },
  { term: 'typeof', category: 'Values & operators', definition: 'Gets the `Type` object for a type.', example: 'typeof(int)' },
  { term: 'nameof', category: 'Values & operators', definition: 'Gets the name of a symbol as a string (refactor-safe).', example: 'nameof(Age) // "Age"' },
  { term: 'default', category: 'Values & operators', definition: 'The default value of a type (0, null, etc.).', example: 'default(int) // 0' },
  { term: 'checked / unchecked', category: 'Values & operators', definition: 'Turn arithmetic overflow checking on or off.' },
  { term: 'and / or / not (patterns)', category: 'Values & operators', definition: 'Combine patterns in switch/if.', example: 'x is > 0 and < 10' },
  { term: 'with', category: 'Values & operators', definition: 'Creates a copy of a record with some properties changed.', example: 'p with { Age = 30 }' },

  // ---- Async & concurrency ----
  { term: 'async', category: 'Async & concurrency', definition: 'Marks a method that can `await` — it may pause without blocking a thread.', example: 'async Task DoAsync() {}' },
  { term: 'await', category: 'Async & concurrency', definition: 'Pauses until an awaited task finishes, then resumes.', example: 'await SaveAsync();' },
  { term: 'lock', category: 'Async & concurrency', definition: 'Ensures only one thread runs a section at a time.', example: 'lock (_gate) { ... }' },
  { term: 'volatile', category: 'Async & concurrency', definition: 'Field reads/writes are always fresh across threads (no caching/reordering).' },

  // ---- Namespaces, files & references ----
  { term: 'namespace', category: 'Namespaces & files', definition: 'A named container that groups related types.', example: 'namespace MyApp.Data;' },
  { term: 'using', category: 'Namespaces & files', definition: 'Imports a namespace, OR auto-disposes a resource at block end.', example: 'using var f = File.Open(path);' },
  { term: 'global using', category: 'Namespaces & files', definition: 'A `using` that applies to the whole project.', example: 'global using System.Linq;' },

  // ---- Built-in types ----
  { term: 'int / long', category: 'Built-in types', definition: 'Whole numbers (32-bit / 64-bit).', example: 'int n = 42;' },
  { term: 'double / float / decimal', category: 'Built-in types', definition: 'Decimal numbers. `decimal` is precise for money; `double`/`float` are faster but approximate.' },
  { term: 'bool', category: 'Built-in types', definition: 'true or false.' },
  { term: 'char', category: 'Built-in types', definition: 'A single character.', example: "char c = 'A';" },
  { term: 'string', category: 'Built-in types', definition: 'Text — a sequence of characters. Immutable.', example: 'string s = "hi";' },
  { term: 'object', category: 'Built-in types', definition: 'The base type of everything in .NET.' },
  { term: 'byte', category: 'Built-in types', definition: 'An 8-bit unsigned number (0–255).' },

  // ---- Generics & constraints ----
  { term: 'where (constraint)', category: 'Generics', definition: 'Restricts what a generic type parameter can be.', example: 'T Max<T>(T a, T b) where T : IComparable<T>' },
  { term: 'T (type parameter)', category: 'Generics', definition: 'A placeholder for a type chosen when the generic is used.', example: 'List<T>' },

  // ---- Other ----
  { term: 'event', category: 'Other', definition: 'A notification mechanism built on delegates that others can subscribe to.', example: 'event Action Clicked;' },
  { term: 'operator', category: 'Other', definition: 'Defines how an operator (like +) works for your type.', example: 'public static Vec operator +(Vec a, Vec b)' },
  { term: 'implicit / explicit', category: 'Other', definition: 'Define automatic or manual type conversions.', example: 'public static implicit operator int(Money m)' },
  { term: 'stackalloc', category: 'Other', definition: 'Allocates a small buffer on the stack (no garbage collection).', example: 'Span<int> b = stackalloc int[4];' },
]
