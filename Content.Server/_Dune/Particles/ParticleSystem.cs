using System.Linq;
using System.Numerics;
using Content.Server._Dune.Cvars;
using Content.Shared._Dune.Particles;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Random;

namespace Content.Server._Dune.Particles;

// bootleg particle system, delete this when rt would have its own
// it will for sure fuck with the server perfomance, so cvar dune.particles_max_amount exists
public sealed class ParticleSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly Dictionary<EntityUid, float> _spawnTimers = new();

    private int ActiveParticles => EntityQuery<ParticleLifetimeComponent>().Count();

    private void SpawnParticles(EntityUid owner, ParticleCreatorComponent comp)
    {
        var xform = Transform(owner);

        if (!comp.WorkOnGrids && xform.GridUid != null)
            return;

        var baseCoords = xform.Coordinates;

        for (var i = 0; i < comp.Amount; i++)
        {
            var angle = _random.NextFloat(0, MathF.Tau);
            var distance = _random.NextFloat(0, comp.Radius);

            var offset = comp.Offset + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
            var spawnCoords = baseCoords.Offset(offset);

            var particle = Spawn(comp.ParticlePrototype, spawnCoords);
            _tagSystem.AddTag(particle, "HideContextMenu");

            var life = _entMan.EnsureComponent<ParticleLifetimeComponent>(particle);
            life.TimeRemaining = comp.Lifetime;

            var vel = _entMan.EnsureComponent<ParticleVelocityComponent>(particle);
            vel.Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * comp.Speed;
        }
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ParticleCreatorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var particle, out var _))
        {
            if (!_spawnTimers.ContainsKey(uid))
                _spawnTimers[uid] = 0f;

            _spawnTimers[uid] += frameTime;
            if (_spawnTimers[uid] >= particle.SpawnInterval)
            {
                SpawnParticles(uid, particle);
                _spawnTimers[uid] = 0f;
            }
        }

        var particleQuery = EntityQueryEnumerator<ParticleLifetimeComponent, TransformComponent>();
        while (particleQuery.MoveNext(out var uid, out var life, out var xform))
        {
            life.TimeRemaining -= frameTime;
            if (life.TimeRemaining <= 0f)
            {
                QueueDel(uid);
                continue;
            }

            if (TryComp<ParticleVelocityComponent>(uid, out var vel))
                xform.WorldPosition += vel.Velocity * frameTime;
        }
    }
}
