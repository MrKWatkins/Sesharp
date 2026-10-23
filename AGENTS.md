# Test assertions

Use `MrKWatkins.Assertions` for all unit and integration test assertions, following the existing `value.Should().Equal(...)`, `.Contain(...)` and `.BeTrue()` patterns.
For exception assertions, use `AssertThat.Invoking(...).Should().Throw<TException>()` or `.NotThrow()`.
Use NUnit for test attributes and lifecycle hooks; do not introduce NUnit `Assert.*`, `ClassicAssert`, `CollectionAssert` or `StringAssert` assertions.
