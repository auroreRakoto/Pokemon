# Build and Deployment

## Build targets

The intended architecture includes a Unity client and dedicated server build. Exact platforms and hosting provider are OPEN.

## Build separation

- client build includes presentation and client networking;
- server build includes authoritative simulation and server infrastructure;
- editor tooling is excluded from runtime builds;
- secrets and developer-only tools are never shipped to clients.

## Reproducibility

Record Unity version, package manifest/lock, build settings, scripting backend, platform settings and build identifier.

## Deployment topics to define

- server provisioning;
- configuration/environment variables;
- secret management;
- logging and monitoring;
- health checks;
- graceful shutdown;
- save/checkpoint handling;
- version compatibility;
- rollback;
- crash diagnostics.

Do not treat a successful local Editor Play session as proof that a dedicated server build works.
