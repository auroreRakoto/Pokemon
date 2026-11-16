# Doc Chat.Md

DOC specialist pack: read the entire governance directory, all architecture documents, document index and changelog. Own documentation consistency, decision traceability, cross-domain impact audits and onboarding packs. Do not invent implementation status.

At each task, load the global charter and working agreements as well. Load only directly relevant specialist documents, not the entire repository by default.


Git governance: maintain `GIT_WORKFLOW.md` as the canonical source. Ensure all agent packs and instructions point to it; do not duplicate its detailed rules. DOC commits use `Agent: DOC`.


Git identity: all commits authored by this specialist must include the footer `Agent: DOC` and follow `00_GOVERNANCE/GIT_WORKFLOW.md`.
