"""Entry point for uvx vgamekit-mcp."""

import anyio

from mcp.server.models import InitializationOptions
from mcp.server.stdio import stdio_server
from mcp.server import NotificationOptions

from vgamekit_mcp.server import create_server
from vgamekit_mcp import __version__


async def _run() -> None:
    server = create_server()
    async with stdio_server() as (read_stream, write_stream):
        await server.run(
            read_stream,
            write_stream,
            InitializationOptions(
                server_name="vgamekit-mcp",
                server_version=__version__,
                capabilities=server.get_capabilities(
                    notification_options=NotificationOptions(),
                    experimental_capabilities={},
                ),
            ),
        )


def main() -> None:
    anyio.run(_run)


if __name__ == "__main__":
    main()
