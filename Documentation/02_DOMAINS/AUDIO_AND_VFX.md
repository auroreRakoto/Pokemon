# Audio and VFX

## Scope

Owns presentation standards and asset organization for sounds and visual effects. Gameplay domains own the meaning and timing of events.

## VFX categories

- Pokémon ambient effects;
- movement effects (dust, water splash, leaf movement);
- attack/projectile effects;
- impact/hit effects;
- status effects;
- environmental effects (rain, fog, wind, fireflies);
- resource feedback (berry picked, plant regrowth).

## Audio categories

- Pokémon vocalizations;
- movement/foley;
- environment ambience;
- weather;
- combat/ability sounds;
- UI feedback;
- music.

## Event-driven presentation

Gameplay emits meaningful events or presentation cues. Presentation systems map these to animation/VFX/audio. Do not make gameplay depend on a particle system successfully playing.

## Attachments/sockets

Potential socket names include Head, Mouth, Back, AttackOrigin, HitCenter, Ground, PerchPoint, RestPoint, NestPoint, ResourceSlot. Define per asset and only where needed. Socket contracts must be shared with consuming domains.

## Performance

Use pooling for frequently spawned effects, distance/visibility relevance, limits on concurrent audio/VFX and graceful fallback when assets are missing.
