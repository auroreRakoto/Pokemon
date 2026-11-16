# Combat and Abilities

## Scope

Owns real-time combat rules, attack validation, damage, cooldowns, ability execution and combat outcomes.

## Separation

- Combat domain decides whether an attack is valid and what it does.
- Pokémon domain owns general individual health/physiology state through an agreed contract.
- Art owns animation/VFX/audio assets and sockets.
- Networking/server validates and replicates authoritative results.
- UI presents information but does not determine outcomes.

## Attack lifecycle

Conceptual flow:
1. player or Pokémon requests an action;
2. server checks actor state, ability, range, target, cooldown and resource/cost conditions;
3. combat starts an action and emits presentation cues;
4. hit detection resolves according to the chosen combat model;
5. damage/status/knockback/effects are applied authoritatively;
6. result event is replicated;
7. animation, VFX, audio and UI present the result.

## Animation/VFX separation

Animation can signal timing or pose, but must not be the sole authority for damage. Use explicit attack timing/data or server-side resolution. VFX are presentation and must not determine hit results.

## Ability data

Potential fields:
- ability ID;
- targeting mode;
- range/area;
- cooldown;
- windup/active/recovery timing;
- damage/effect parameters;
- costs;
- status effects;
- animation/VFX/audio references;
- validation rules.

Exact combat mechanics remain OPEN.

## Integration requirements

Every ability must define:
- actor/target requirements;
- server validation;
- authoritative effect;
- cancellation/interruption behaviour;
- animation event or timing contract;
- VFX/audio cues;
- replication requirements;
- tests for range, cooldown, invalid targets and race conditions.
