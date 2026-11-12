# Test assertions

- For C# changes, load and follow the `csharp-conventions` skill.

Use `MrKWatkins.Assertions` for all unit and integration test assertions, following the existing `value.Should().Equal(...)`, `.Contain(...)` and `.BeTrue()` patterns.
For exception assertions, use `AssertThat.Invoking(...).Should().Throw<TException>()` or `.NotThrow()`.
Use NUnit for test attributes and lifecycle hooks; do not introduce NUnit `Assert.*`, `ClassicAssert`, `CollectionAssert` or `StringAssert` assertions.

# Licence

Preserve the GPL-3.0-only licence for project code and package metadata.

# Build conventions

- Shared C# rules, test defaults and packaging assets come from MrKWatkins.CSharpConventions.
- Only the command-line tool is published. Core implementation and test-support assemblies remain non-packable.
- Missing XML comments remain allowed for internal tooling and test fixtures.
- Hard wrap C# and Markdown at 180 characters. Verify whitespace with `dotnet format whitespace Sesharp.sln --no-restore --verify-no-changes`.
