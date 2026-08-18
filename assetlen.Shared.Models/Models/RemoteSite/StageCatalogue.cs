namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// One major phase of a build. The unit a colour is assigned to, and the unit
/// stages are grouped under.
/// <para>
/// The group is what makes an accent worth having: plinth walling, ground-floor
/// walling and parapet walling are all "walling", and the only thing that tells
/// them apart at a glance is which phase they belong to (assetlen.md §3 — the
/// stage is the funded unit, and Peter funds them one at a time).
/// </para>
/// </summary>
public enum StageGroup
{
    /// <summary>
    /// Anything the reader named themselves that fits no phase below — and,
    /// deliberately, zero. A column added to a table of existing stages defaults
    /// to 0, and an unset phase must read as "nobody has said" rather than
    /// silently claiming that every stage on the project is preliminaries.
    /// </summary>
    Custom = 0,

    Preliminaries = 1,
    Substructure = 2,
    Superstructure = 3,
    Roofing = 4,
    Envelope = 5,
    Services = 6,
    Finishes = 7,
    ExternalWorks = 8,
    Handover = 9
}

/// <summary>One known stage of construction, and enough detail to recognise it.</summary>
/// <param name="Key">Stable identifier. Stored on the stage so the catalogue can grey out what is already used.</param>
/// <param name="Name">What it is called on site.</param>
/// <param name="Group">The major phase it belongs to — decides its accent.</param>
/// <param name="Detail">What the stage actually covers, in the words a foreman would use.</param>
/// <param name="Aliases">Other names for the same work, so search finds it however the reader thinks of it.</param>
public readonly record struct StageCatalogueItem(
    string Key,
    string Name,
    StageGroup Group,
    string Detail,
    string[] Aliases)
{
    /// <summary>True when the reader's query matches the name, the detail or any alias.</summary>
    public bool Matches(string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;

        var q = query.Trim();

        return Name.Contains(q, StringComparison.OrdinalIgnoreCase)
            || Detail.Contains(q, StringComparison.OrdinalIgnoreCase)
            || StageCatalogue.GroupName(Group).Contains(q, StringComparison.OrdinalIgnoreCase)
            || Aliases.Any(a => a.Contains(q, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// The known stages of construction, so a project is not stuck with whatever was
/// typed on the first day.
/// <para>
/// Held in code rather than a table on purpose: it is a reference list, the same
/// for every tenant, and a stage a reader actually uses is copied onto their
/// project as an ordinary row. Nothing here is per-project state.
/// </para>
/// </summary>
public static class StageCatalogue
{
    public static string GroupName(StageGroup group) => group switch
    {
        StageGroup.Preliminaries => "Preliminaries & site setup",
        StageGroup.Substructure => "Substructure",
        StageGroup.Superstructure => "Superstructure",
        StageGroup.Roofing => "Roofing",
        StageGroup.Envelope => "Envelope & openings",
        StageGroup.Services => "Services",
        StageGroup.Finishes => "Finishes",
        StageGroup.ExternalWorks => "External works",
        StageGroup.Handover => "Handover",
        _ => "Custom"
    };

    public static string GroupDetail(StageGroup group) => group switch
    {
        StageGroup.Preliminaries => "Getting onto the site and being able to work on it.",
        StageGroup.Substructure => "Everything below the damp-proof course.",
        StageGroup.Superstructure => "The frame and the walls, floor by floor.",
        StageGroup.Roofing => "Getting it watertight overhead.",
        StageGroup.Envelope => "Closing the building in — doors, windows, external finishes.",
        StageGroup.Services => "Everything that runs inside the walls.",
        StageGroup.Finishes => "What the client actually sees and touches.",
        StageGroup.ExternalWorks => "The site around the building.",
        StageGroup.Handover => "Proving it works and handing it over.",
        _ => "Named on this project."
    };

    /// <summary>Every group in build order. Drives the catalogue's own ordering.</summary>
    public static readonly StageGroup[] Groups =
    [
        StageGroup.Preliminaries, StageGroup.Substructure, StageGroup.Superstructure,
        StageGroup.Roofing, StageGroup.Envelope, StageGroup.Services,
        StageGroup.Finishes, StageGroup.ExternalWorks, StageGroup.Handover
    ];

    public static readonly StageCatalogueItem[] Items =
    [
        // ── Preliminaries ────────────────────────────────────────────────
        new("prelim.mobilisation", "Site mobilisation", StageGroup.Preliminaries,
            "Hoarding, site office, store, water and power connections, access road.",
            ["setup", "site establishment", "hoarding"]),
        new("prelim.clearance", "Site clearance", StageGroup.Preliminaries,
            "Removing vegetation, topsoil and anything standing on the plot.",
            ["clearing", "grubbing", "demolition"]),
        new("prelim.setting-out", "Setting out", StageGroup.Preliminaries,
            "Profiles, pegs and levels — the building located on the ground against the drawing.",
            ["pegging", "profiles", "levels"]),

        // ── Substructure ─────────────────────────────────────────────────
        new("sub.excavation", "Excavation", StageGroup.Substructure,
            "Digging trenches and pits to formation level, and carting away spoil.",
            ["digging", "trenches", "earthworks"]),
        new("sub.foundation", "Foundation", StageGroup.Substructure,
            "Blinding, reinforcement and concrete to the strip, pad or raft foundation.",
            ["footing", "strip foundation", "raft", "pad"]),
        new("sub.foundation-walling", "Foundation walling", StageGroup.Substructure,
            "Blockwork or masonry from the footing up to plinth level.",
            ["below-ground walling", "footing walls"]),
        new("sub.plinth-walling", "Plinth walling", StageGroup.Substructure,
            "Walling from ground level to the damp-proof course, with the plinth beam.",
            ["plinth", "dpc walling", "plinth beam"]),
        new("sub.hardcore", "Hardcore and blinding", StageGroup.Substructure,
            "Filling, compacting and blinding the oversite ready for the slab.",
            ["filling", "murram", "compaction", "oversite"]),
        new("sub.ground-slab", "Ground floor slab", StageGroup.Substructure,
            "Damp-proof membrane, mesh and concrete to the ground floor.",
            ["oversite concrete", "slab", "dpm"]),

        // ── Superstructure ───────────────────────────────────────────────
        new("super.columns", "Columns and stanchions", StageGroup.Superstructure,
            "Reinforcement, formwork and concrete to the vertical frame.",
            ["stanchions", "posts", "frame"]),
        new("super.ground-walling", "Ground floor walling", StageGroup.Superstructure,
            "Walling from the slab to the ground-floor ring beam, with openings formed.",
            ["walling", "blockwork", "brickwork", "first floor walls"]),
        new("super.ring-beam", "Ring beam", StageGroup.Superstructure,
            "The beam tying the walling together at each floor level.",
            ["lintel beam", "tie beam", "band beam"]),
        new("super.suspended-slab", "Suspended slab", StageGroup.Superstructure,
            "Formwork, reinforcement and concrete to an upper floor, and striking it.",
            ["first floor slab", "upper slab", "decking"]),
        new("super.upper-walling", "Upper floor walling", StageGroup.Superstructure,
            "Walling to the floors above ground.",
            ["first floor walling", "upper walls"]),
        new("super.staircase", "Staircase", StageGroup.Superstructure,
            "Structural flights and landings between floors.",
            ["stairs", "flight", "landing"]),
        new("super.parapet-walling", "Parapet walling", StageGroup.Superstructure,
            "Walling carried above roof level, with its coping.",
            ["parapet", "coping"]),

        // ── Roofing ──────────────────────────────────────────────────────
        new("roof.structure", "Roof structure", StageGroup.Roofing,
            "Trusses, purlins and wall plates set out and fixed.",
            ["trusses", "purlins", "carcass", "timber"]),
        new("roof.covering", "Roof covering", StageGroup.Roofing,
            "Sheeting or tiling, ridges and flashings — the point it stops raining inside.",
            ["iron sheets", "tiles", "sheeting", "watertight"]),
        new("roof.rainwater", "Rainwater goods", StageGroup.Roofing,
            "Gutters, fascia, downpipes and the discharge from them.",
            ["gutters", "downpipes", "fascia"]),

        // ── Envelope ─────────────────────────────────────────────────────
        new("env.doors-windows", "Doors and windows", StageGroup.Envelope,
            "Frames, shutters and glazing to every opening, and their ironmongery.",
            ["glazing", "aluminium", "shutters", "frames", "fenestration"]),
        new("env.burglar-proofing", "Burglar proofing", StageGroup.Envelope,
            "Bars, grilles and security screens to openings.",
            ["grilles", "bars", "security"]),
        new("env.external-render", "External render", StageGroup.Envelope,
            "Plaster, screed or cladding to the outside face of the walls.",
            ["plaster", "cladding", "external plaster"]),

        // ── Services ─────────────────────────────────────────────────────
        new("svc.electrical-first", "Electrical first fix", StageGroup.Services,
            "Conduits, back boxes and cabling chased in before plastering.",
            ["conduiting", "wiring", "first fix"]),
        new("svc.electrical-second", "Electrical second fix", StageGroup.Services,
            "Switches, sockets, fittings and the board energised and tested.",
            ["fittings", "sockets", "second fix", "distribution board"]),
        new("svc.plumbing-first", "Plumbing first fix", StageGroup.Services,
            "Supply and waste pipework run and pressure-tested before closing up.",
            ["pipework", "soil", "waste", "first fix"]),
        new("svc.plumbing-second", "Plumbing second fix", StageGroup.Services,
            "Sanitary ware, taps and appliances fitted and commissioned.",
            ["sanitary ware", "taps", "second fix", "fittings"]),
        new("svc.drainage", "Drainage", StageGroup.Services,
            "Inspection chambers, septic tank or sewer connection and the runs to them.",
            ["septic", "soakpit", "manholes", "sewer"]),
        new("svc.water-storage", "Water storage", StageGroup.Services,
            "Tanks, stands, pumps and the distribution from them.",
            ["tank", "reservoir", "pump", "booster"]),

        // ── Finishes ─────────────────────────────────────────────────────
        new("fin.internal-plaster", "Internal plaster", StageGroup.Finishes,
            "Rendering and skimming the internal walls and soffits.",
            ["rendering", "skimming", "wall finish"]),
        new("fin.screed", "Floor screed", StageGroup.Finishes,
            "Levelling screed laid ready to receive the floor finish.",
            ["levelling", "sand cement screed"]),
        new("fin.floor-finish", "Floor finishes", StageGroup.Finishes,
            "Tiling, timber, vinyl or terrazzo to the finished floors.",
            ["tiles", "tiling", "terrazzo", "timber floor"]),
        new("fin.wall-tiling", "Wall tiling", StageGroup.Finishes,
            "Tiling to wet areas and splashbacks.",
            ["tiles", "bathroom tiling", "splashback"]),
        new("fin.ceiling", "Ceilings", StageGroup.Finishes,
            "Suspended, gypsum or timber ceilings, with cornices and access panels.",
            ["gypsum", "soffit", "cornice", "suspended ceiling"]),
        new("fin.painting", "Painting and decoration", StageGroup.Finishes,
            "Preparation, undercoat and finish coats inside and out.",
            ["paint", "decoration", "emulsion"]),
        new("fin.joinery", "Fitted joinery", StageGroup.Finishes,
            "Kitchen units, wardrobes, vanities and built-in furniture.",
            ["kitchen", "wardrobes", "cabinets", "carpentry"]),

        // ── External works ───────────────────────────────────────────────
        new("ext.retaining-wall", "Retaining wall", StageGroup.ExternalWorks,
            "Walls holding back ground, with their drainage behind.",
            ["retaining", "gabion", "earth retention"]),
        new("ext.perimeter-wall", "Perimeter wall and gate", StageGroup.ExternalWorks,
            "The boundary wall, gate and gatehouse.",
            ["fence", "boundary", "gate", "compound wall"]),
        new("ext.paving", "Paving and driveway", StageGroup.ExternalWorks,
            "Driveway, parking, walkways and kerbs.",
            ["driveway", "hardstanding", "walkway", "kerbs"]),
        new("ext.landscaping", "Landscaping", StageGroup.ExternalWorks,
            "Soft landscaping, planting and site drainage falls.",
            ["garden", "planting", "lawn"]),
        new("ext.septic", "Septic and soak pit", StageGroup.ExternalWorks,
            "The external treatment and disposal installation.",
            ["soakaway", "septic tank"]),

        // ── Handover ─────────────────────────────────────────────────────
        new("hand.snagging", "Snagging", StageGroup.Handover,
            "The defects list walked, agreed and worked off.",
            ["defects", "punch list", "making good"]),
        new("hand.commissioning", "Testing and commissioning", StageGroup.Handover,
            "Services proved working, with the certificates to show it.",
            ["testing", "certificates", "sign-off"]),
        new("hand.handover", "Handover", StageGroup.Handover,
            "Keys, as-built drawings, warranties and the final account.",
            ["completion", "as-built", "final account"])
    ];

    public static StageCatalogueItem? Find(string? key) =>
        string.IsNullOrEmpty(key) ? null : Items.FirstOrDefault(i => i.Key == key) is { Key.Length: > 0 } hit ? hit : null;

    /// <summary>Items matching a query, grouped in build order. Empty groups are dropped.</summary>
    public static IEnumerable<IGrouping<StageGroup, StageCatalogueItem>> Search(string? query) =>
        Items.Where(i => i.Matches(query))
             .GroupBy(i => i.Group)
             .OrderBy(g => Array.IndexOf(Groups, g.Key));
}
