# Response: "silent cross-attach when multiple builders share a `(Category, SubCategory)`"

Reviewed against the current Hitch source (`HitchBuilder.ProcessCategorizedGroup`, the `$plugin`
routing release). Short version: **the problem is real and the proposed fixes are right, but the
mechanism described in the issue is out of date.** The current default is *silent skip*, not *silent
cross-attach / last-writer-wins*. That changes the symptom, the repro trace, and one of the claims
about the workaround. Details below so the trace doesn't get bounced as non-reproducing.

## What's correct in the issue

- Two `[HitchPlugin]` builders under the same `(Category, SubCategory)` with no routing labels is a
  real footgun, and it is **silent** — no exception at startup.
- It is **latent**: a bucket with one builder works; adding a second is what breaks it.
- The fix direction is right: **fail loud on ambiguity** (#1) and **type-based routing** (#2).
- The "give each provider its own subcategory" alternative works today — grouping is purely
  `(Category, SubCategory)` and nothing else keys off it.

## What's inaccurate (must fix before filing)

### 1. The default is silent SKIP, not silent CROSS-ATTACH

The issue says Hitch "runs **every** builder for **every** config instance" so builders "overwrite
each other's keyed registration — last one wins." **That code path does not exist.**

`ProcessCategorizedGroup` routes each instance to *at most one* builder:

```csharp
var owner = instanceSection["$plugin"];
if (!string.IsNullOrEmpty(owner)) {
    var match = candidates.FirstOrDefault(a => AliasMatches(a, owner));
    if (match is null) { /* log + continue — SKIP */ }
    else AttachPlugin(match.PluginType, ...);          // exactly one builder
}
else if (candidates.Count == 1) AttachPlugin(candidates[0]...);  // lone builder — fine
else { /* log + continue — SKIP, attach NOBODY */ }    // ≥2 builders, no $plugin
```

`AttachPlugin` is only ever called for: the lone candidate, the `$plugin`-matched candidate, or an
uncategorized plugin. There is **no loop that attaches all candidates**. So with two builders and no
discriminator, the instance is **dropped entirely — neither builder's `Attach` runs.**

Evidence: the shipped test `InstanceWithoutOwnerInSharedSubcategoryIsSkipped` asserts **both**
builders' attach counts are `0` for an unrouted instance. (Independently re-confirmed with a probe
test: two builders + unrouted instance → zero attaches.)

**Consequence for the issue:** "last one wins," "order-dependent," and "the wrong builder writes your
key" do not apply — *nothing* is written, so order is irrelevant.

### 2. The Tools trace won't reproduce as written

The issue's trace shows `datetime` resolving to "an (empty) MCP provider" (DateTime clobbered by
MCP). Under the current code, with no aliases, **`datetime` never registers at all** — resolving the
`datetime` key returns nothing/throws. The observable ("DateTime tool disappears") is the same; the
*cause* in the trace is wrong. A Hitch maintainer running the repro will see both tools vanish, not
datetime-becomes-MCP, and may reject the report. Rewrite the trace to:

```
instance "datetime" (no $plugin, 2 builders in bucket): matches 2 plugins, declares no '$plugin' → SKIPPED
instance "mcp"      (no $plugin, 2 builders in bucket): matches 2 plugins, declares no '$plugin' → SKIPPED
→ neither tool registers; both disappear from the Tools page.
```

### 3. `AddKeyedSingleton` is a Covalent detail, not Hitch behavior

The issue frames the collision as Hitch writing keyed services. **Hitch contains no `AddKeyed*`
anywhere** — it only chooses *which builder to invoke* and hands it the instance `name`. The keyed
registration happens entirely in the consumer's `Attach`. Keep the two layers separate in the
writeup: Hitch's bug is "routes an instance to zero or wrong builders," not "clobbers a DI key."

### 4. Version number

The issue says "Hitch version 1.0.1." This repo's CHANGELOG is on `10.0.x` / Unreleased, and the
`$plugin` routing is the Unreleased entry. The behavior described (cross-attach) matches a **pre-`$plugin`**
build, which is likely the source of the mismatch. Confirm the version Covalent is actually running —
if it's genuinely pre-routing, the cross-attach description may be correct *for that build* but is
fixed in current Hitch; if it's current, the skip description applies.

## On the proposed fixes

**#1 Fail loud — endorsed, and it's a small change.** The ambiguity is *already detected*; it just
logs (`Console.Error.WriteLine`) instead of throwing. Two branches should throw:
- ≥2 builders and the instance declares no `$plugin`.
- `$plugin` is set but matches no registered builder.

Guardrails for the Hitch-side implementation:
- Must **not** throw for the lone-builder bucket with no `$plugin` — that's the common, correct path
  and is already distinguished in code.
- Prefer **aggregating** all bucket conflicts and throwing once at the end of `Build()`, so startup
  reports every collision, not just the first.
- This is a behavior change for anyone relying on silent skip; ship it as such (note in CHANGELOG).

**#2 Type-based routing — already partially exists; temper the claim.** Routing by type name works
today: `AliasMatches` accepts the plugin's `Type.FullName`/`Name` as the `$plugin` value (covered by
the `OwnerMatchesOnTypeNameWhenAliasFallbackUsed` test), so you can drop `Alias =` and route by type
now. But "removes the boilerplate entirely; nobody has to remember an alias" is too strong: with two
*distinct* builder types in one bucket, Hitch still needs a **per-instance** discriminator to know
which instance belongs to which builder. Type routing only makes that discriminator **type-safe**
(`plugin: typeof(McpToolsProviderBuilder)` stamping `FullName`) instead of a magic string — it
removes the *string*, not the *per-instance tag*. The real "never silently collide" guarantee comes
from **#1 (fail loud)** or from distinct subcategories, not from #2.

## Recommended path for Covalent

1. **Now, no Hitch change:** either (a) ensure every builder in a shared bucket is routed — each
   `WithPlugin(..., plugin: "<Alias-or-TypeName>")` — and add a Covalent-side startup assertion that
   every configured instance in a multi-builder bucket carries a `$plugin`; or (b) split the shared
   buckets into distinct subcategories (`DateTime → (Covalent, DateTimeTool)`, `Mcp → (Covalent, Mcp)`),
   which drops all aliases. Covalent discovers providers via DI keyed services + `ICovalentProviderCollection`,
   not Hitch category, so the shared bucket buys nothing — (b) is the clean fix.
2. **File upstream:** request #1 (aggregated fail-loud at `Build()`) as the durable fix, with the
   corrected mechanism/trace above. Optionally request a `WithPlugin(..., Type plugin)` overload for
   type-safe routing.

## One-line correction to lead the issue with

> In the current `$plugin`-routing build, an unrouted instance in a multi-builder bucket is **silently
> skipped (registered by no builder)** — not cross-attached with last-writer-wins. The proposed
> fail-loud fix still applies and is arguably more justified, since today the instance vanishes with
> only a `Console.Error` line.
