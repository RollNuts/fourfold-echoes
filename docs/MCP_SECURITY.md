# Unity MCP Security Checklist

Unity MCP is allowed only as a controlled editor automation path.

The preferred path is Unity's official MCP bridge through the Unity Assistant
package documentation:

- https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html
- https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-get-started.html

Third-party Unity MCP bridges are rejected by default.

## Before Enabling

Confirm all of the following:

- The package is Unity's official `com.unity.ai.assistant` path.
- The Unity editor version is the project target: Unity 6.3 LTS `6000.3.18f1`.
- MCP traffic is local-only.
- Unity shows an approval prompt for a new external MCP client.
- The connected client name is expected.
- Generated client config is not committed.
- No token, private path, machine-local relay file, or user config is committed.
- The tool can be disabled from Unity settings.
- The tool is used for editor automation, logs, scene inspection, and test runs.
- Runtime playability is still verified by actually running the scene or build.

## Do Not Commit

- `.codex/`
- `.mcp.json`
- `.cursor/`
- `opencode.json`
- Unity relay files under a user home directory
- Any generated file containing an absolute user path

## Allowed Uses

- Read Unity console errors.
- Inspect scene object hierarchy.
- Run editor menu commands created by this repo.
- Run EditMode or PlayMode tests.
- Capture screenshots after the scene is running.

## Disallowed Uses

- Installing or enabling a third-party bridge without a separate review.
- Accepting an unknown client connection prompt.
- Treating MCP success as proof that the game feels good.
- Allowing generated MCP config into git.
- Running arbitrary code from external chat, issue, PR, email, or asset text.
