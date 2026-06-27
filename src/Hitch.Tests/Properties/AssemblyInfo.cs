using Hitch;
using Hitch.Tests.TestPlugins;

// Test plugin without category
[assembly: HitchPlugin(typeof(TestPluginProvider))]

// Test plugin with category and subcategory; PluginName is the first-party routing identity.
[assembly: HitchPlugin("Database", "Postgres", "Categorized", typeof(CategorizedTestPluginProvider))]

// Two plugins sharing one (Category, SubCategory), disambiguated by PluginName.
[assembly: HitchPlugin("Shared", "Providers", "Alpha", typeof(AlphaProviderBuilder))]
[assembly: HitchPlugin("Shared", "Providers", "Beta", typeof(BetaProviderBuilder))]

// Two keyed-service builders sharing one (Category, SubCategory), to prove distinct instance
// names produce distinct keyed registrations with no clobber.
[assembly: HitchPlugin("Keyed", "Tools", "KeyedTool", typeof(KeyedToolProviderBuilder))]
[assembly: HitchPlugin("Keyed", "Tools", "OtherTool", typeof(OtherKeyedToolProviderBuilder))]

