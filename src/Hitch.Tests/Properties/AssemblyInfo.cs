using Hitch;
using Hitch.Tests.TestPlugins;

// Test plugin without category
[assembly: HitchPlugin(typeof(TestPluginProvider))]

// Test plugin with category and subcategory
[assembly: HitchPlugin("Database", "Postgres", typeof(CategorizedTestPluginProvider))]

// Two plugins sharing one (Category, SubCategory), disambiguated by alias
[assembly: HitchPlugin("Shared", "Providers", typeof(AlphaProviderBuilder), Alias = "Alpha")]
[assembly: HitchPlugin("Shared", "Providers", typeof(BetaProviderBuilder), Alias = "Beta")]

