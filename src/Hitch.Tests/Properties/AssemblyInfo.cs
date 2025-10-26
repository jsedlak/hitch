using Hitch;
using Hitch.Tests.TestPlugins;

// Test plugin without category
[assembly: HitchPlugin(typeof(TestPluginProvider))]

// Test plugin with category and subcategory
[assembly: HitchPlugin("Database", "Postgres", typeof(CategorizedTestPluginProvider))]

