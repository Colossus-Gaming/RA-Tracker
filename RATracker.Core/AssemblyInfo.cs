using System.Runtime.CompilerServices;

// The V1 DTOs and mappers are internal implementation detail, but the test suite asserts on them
// directly. They moved here from the WPF assembly, so the grant has to move with them.
[assembly: InternalsVisibleTo("RATracker.Tests")]
