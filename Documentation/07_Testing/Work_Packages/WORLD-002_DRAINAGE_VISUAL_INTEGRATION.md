# WORLD-002 — Drainage visual integration in Planet Studio

## Scope
This increment integrates the existing spherical drainage prototype into the Editor-first Planet Generation Studio. It adds a configurable drainage resolution (8–512), provisional sea-level cutoff, river accumulation threshold and three overlays: river candidates, flow accumulation and sampled flow direction. The same drainage graph is generated independently of the display mesh resolution.

## How to use
1. Open `Tools > World > Open Planet Generation Studio`.
2. Generate the ordinary terrain preview if one is not already present.
3. In `WORLD-002 — Hydrology / Drainage`, set drainage resolution (512 is the benchmarked target), sea level and accumulation threshold.
4. Choose `RiverCandidates`, `FlowAccumulation` or `FlowDirection`.
5. Click `Generate Drainage Preview`.
6. Switch `View` between Spherical and Planisphere and regenerate to inspect the same field in each projection.
7. Use `Clear Drainage` to remove only the overlay; `Clear Preview` removes the complete preview.

## Field definitions
- Elevation is sampled from the current Planet Studio preview stage and settings.
- Water outlets are provisionally classified by `height <= drainageSeaLevelMeters`. This is an editable preview cutoff, not the final water/ocean/lake classification.
- Flow accumulation is the prototype's contributing-node count proxy (not area-weighted).
- River candidates are the prototype `RiverMask`; they are not approved final river polylines.
- Flow direction is shown as sampled downstream segments, controlled by `flowDirectionStride`.

## Acceptance / limitations
- [ ] Unity compiles the new Editor and settings scripts without warnings/errors.
- [ ] Generate drainage preview at 512 and inspect the globe.
- [ ] Inspect the same generated drainage data on the planisphere, including the longitude wrap.
- [ ] Confirm overlay can be cleared/rebuilt without destroying the terrain preview.
- [ ] Confirm threshold and sea-level changes alter the overlay predictably.
- [ ] Check that no overlay mesh is silently left behind after regeneration.
- [ ] Visual continuity, channel quality, lake/outlet semantics and area-weighted accumulation remain unvalidated until manual review.

This integration is an inspection tool, not the final hydrography acceptance gate.
