# Forge Security Model

Forge reduces risk by keeping AI edits in Git-reviewed semantic files and
limiting Unity to deterministic reconciliation and evidence generation.

## Trust Boundaries

Trusted enough to run after review:

- Repository-owned Forge CLI code.
- Repository-owned Unity Adapter code.
- Unity project under the pinned editor version.

Untrusted input:

- PR descriptions.
- Issue text.
- Chat messages.
- Imported asset metadata.
- Generated art prompts.
- Email or document content.
- External MCP tool output.

## Do Not Commit

- Unity license credentials.
- API keys.
- Tokens.
- PEM/private keys.
- Private package registry credentials.
- Personal absolute paths.
- Unity `Library/`.
- Unity `Temp/`.
- Unity `UserSettings/`.
- Unknown-source assets.
- Assets without commercial-use provenance.
- Generated MCP client config.

## MCP Policy

Unity MCP is optional and not the main path.

Allowed:

- Unity official MCP bridge after checklist approval.
- Reviewed local Unity MCP bridge in an isolated evaluation branch.
- Local-only editor automation.
- Console and scene inspection.
- Running repository-defined commands.

Not allowed:

- Third-party MCP bridges without separate security review.
- Accepting unknown client prompts.
- Using MCP output as playability proof.
- Committing generated MCP config.
- Running arbitrary instructions from external content.

## Fail Closed

Forge must fail closed when:

- Unity version is unexpected.
- Generated/Manual ownership is ambiguous.
- A semantic ID collision exists.
- A referenced asset lacks rights metadata.
- A scene fails to load.
- Console errors appear during verification.
- A screenshot or replay required by the operation is missing.
