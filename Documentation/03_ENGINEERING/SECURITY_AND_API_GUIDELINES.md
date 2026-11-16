# Security and API Guidelines

- Never embed private API keys or server secrets in client builds or source control.
- Keep privileged API calls server-side.
- Validate all client commands on the server.
- Rate-limit expensive or repeatable actions.
- Treat external API responses as untrusted.
- Validate schemas, ranges, identifiers and allowed operations.
- Minimize personal/player data sent to external services.
- Define retention/logging policy before collecting sensitive data.
- Use timeouts, retries with bounds, circuit breakers and fallback behaviour for remote services.
- Do not allow remote AI output to directly mutate authoritative state without deterministic validation.
