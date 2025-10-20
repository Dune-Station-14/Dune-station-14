using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Server._Dune.Cvars;

[CVarDefs]
public sealed partial class CCVars : CVars
{
    [CVarControl(AdminFlags.Debug)]
    public static readonly CVarDef<int> MaxParticles =
        CVarDef.Create("dune.particles_max_amount", 512, CVar.SERVERONLY);
}
