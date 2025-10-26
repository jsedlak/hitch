namespace Hitch.Tests;

/// <summary>
/// Collection definition to ensure tests run sequentially
/// (xUnit runs tests in different collections in parallel, but tests in the same collection sequentially)
/// </summary>
[CollectionDefinition("Hitch Tests")]
public class HitchTestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

