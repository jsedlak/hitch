# Hitch: silent cross-attach when multiple builders share a `(Category, SubCategory)`

**Hitch version:** 1.0.1 (the `$plugin` / `Alias` routing release)
**Severity:** high — silently disables a previously-working plugin, no error, no log.

## TL;DR

When two `[HitchPlugin]` builders are registered under the **same** `(Category, SubCategory)`, Hitch runs
**every** builder for **every** config instance in that bucket unless each builder declares an explicit
`Alias` (and each instance carries a matching `$plugin` discriminator). Without those labels, each builder
registers its keyed service under the instance's name, so the builders **overwrite each other's keyed
registration** — last one wins, the rest silently vanish.

The dangerous part is the **default**: it is silent and order-dependent. Adding a second plugin to a
category that previously had one silently breaks the first, with no diagnostic.

## How Hitch produces it

The config → builder routing is the layer above DI:

1. A config instance exists for a `(Category, SubCategory, name)` — e.g. created by
   `WithPlugin("Covalent", "Tools", "datetime", …)`.
2. Hitch finds the builders registered for that `(Category, SubCategory)` and invokes each builder's
   `Attach(services, config, name)` **with that instance's name**.
3. With **one** builder in the bucket, that's correct. With **two or more** and no alias routing, Hitch
   invokes **all** of them for the instance.
4. Each builder's `Attach` typically does `services.AddKeyedSingleton<TContract>(name, …)` — i.e. it
   registers a keyed service under the instance `name`.
5. So every builder writes the **same** keyed service key. The last registration wins; the others are
   shadowed.

Keyed DI services do **not** prevent this — the collision happens *before* resolution. The keys are fine
and distinct; the problem is that the **wrong builder writes your key** because Hitch handed it the same
instance name.

## How it applies to Covalent

Covalent groups providers by `(Category, SubCategory)`:

| Bucket | Builders | Status |
| --- | --- | --- |
| `(Covalent, Status)` | SampleStatus | 1 builder — safe |
| `(Covalent, Documents)` | AzureBlobDocuments | 1 builder — safe **today** |
| `(Covalent, Responses)` | OpenAi, **Foundry** | 2 builders — hit the bug |
| `(Covalent, Tools)` | DateTime, **Mcp** | 2 builders — hit the bug |

Concrete trace for `Tools` (with a `datetime` and an `mcp` instance, no aliases):

```
instance "datetime":
    DateTimeBuilder.Attach("datetime") → AddKeyedSingleton<IToolProvider>("datetime", DateTimeProvider)
    McpBuilder.Attach("datetime")      → AddKeyedSingleton<IToolProvider>("datetime", McpProvider)   ← clobbers
instance "mcp":
    DateTimeBuilder.Attach("mcp")      → AddKeyedSingleton<IToolProvider>("mcp", DateTimeProvider)
    McpBuilder.Attach("mcp")           → AddKeyedSingleton<IToolProvider>("mcp", McpProvider)          ← clobbers
```

Result: the `datetime` key resolves to an (empty) MCP provider, and the **DateTime tool disappears from
the Tools page** — even though nothing about DateTime changed. The only trigger was *adding* MCP to the
same bucket.

This happened twice:
- **Responses** — OpenAI's services landed under the `azure-foundry` key (OpenAI + Foundry share
  `(Covalent, Responses)`).
- **Tools** — the DateTime tool vanished once MCP was configured.

It is **latent, not loud**: each pairing worked until a *second* provider was added to the bucket, and the
break surfaced far from the cause (a missing tool in the UI, not an error at startup).

### It will happen again

`(Covalent, Documents)` has exactly one builder today (AzureBlobs, no alias). The moment a second document
provider is added (SharePoint, #21; Dataverse, #70), AzureBlobs will silently break unless **both** are
given aliases. The trap is set for the next contributor.

## Current workaround (and why it's fragile)

Each builder in a shared bucket declares an `Alias`, and each `WithPlugin` / config carries a matching
`plugin:` discriminator:

```csharp
[assembly: HitchPlugin("Covalent", "Tools", typeof(McpToolsProviderBuilder), Alias = "Mcp")]
// …and the host call:
builder.HitchBuilder.WithPlugin("Covalent", "Tools", name, configs, plugin: "Mcp");
```

Why it's fragile: it is **opt-in per builder**. Forgetting the alias on **one** of the two builders
silently re-introduces the bug — which is exactly what happened. The `Responses` pair was migrated to
aliases when 1.0.1 landed; the `Tools` pair was missed, so it kept colliding until it was noticed in the
UI. The safe behavior depends on every author of every same-bucket builder remembering an opt-in.

## Proposed fixes (in Hitch)

**1. Fail loud on ambiguity (minimum).** When Hitch finds ≥2 builders for the same `(Category,
SubCategory)` and cannot unambiguously route an instance to exactly one of them, **throw at startup** with
a message naming the colliding builders and the bucket. This turns a silently-disabled plugin into an
immediate, obvious error. It would have caught both Covalent collisions at the door.

**2. Auto-disambiguate by builder identity (ergonomics).** When no explicit `Alias` is given, derive a
stable identity from the builder type so two **distinct** builder types in the same bucket never collide —
ideally letting the host reference the builder by type (`plugin: typeof(McpToolsProviderBuilder)`) rather
than a hand-written string. This removes the boilerplate entirely; nobody has to remember an alias.

(1) is the safety net; (2) removes the footgun. Both together is best: distinct types never collide, and a
genuine ambiguity still fails loud instead of silent.

## Alternative without a Hitch change

Covalent can sidestep it by **not sharing a subcategory** — give each provider its own
`(Category, SubCategory)`, so each bucket has exactly one builder and there is nothing to disambiguate:

```
DateTime → (Covalent, DateTimeTool)
Mcp      → (Covalent, Mcp)
```

This works today and drops every alias. The trade-off is losing Hitch-level grouping of "all Tools
plugins" — which Covalent does not use: providers are discovered at runtime via DI keyed services +
`ICovalentProviderCollection` (filtered by the .NET contract interface), never by Hitch category. So in
Covalent the shared bucket buys nothing and distinct subcategories is a clean fix.

The reason to prefer a Hitch-side fix anyway: the silent-cross-attach default will keep catching anyone
who *does* put two builders in one bucket — so making it loud (or auto-safe) protects every consumer, not
just this codebase.
