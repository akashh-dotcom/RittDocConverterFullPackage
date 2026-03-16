"""
DEPRECATED: id_mapper module has been removed.

All ID mapping functionality has been consolidated into id_authority.py.

MIGRATION GUIDE:
----------------
Old (id_mapper):                     New (id_authority):
----------------                     -------------------
from id_mapper import prescan_file   from id_authority import prescan_file
from id_mapper import get_mapper     from id_authority import get_authority
mapper = get_mapper()                authority = get_authority()
mapper.prescan_epub_file(...)        authority.prescan_epub_file(...)
mapper.register_prescanned_file(...) authority.register_prescanned_file(...)
mapper.map_id(...)                   authority.map_id(...)
mapper.resolve_link(...)             authority.resolve_linkend(...)
mapper.is_dropped(...)               authority.is_dropped(...)
mapper.export_json(path)             authority.export_registry(path)

For complete documentation, see: docs/ID_AUTHORITY_ARCHITECTURE.md
"""

raise ImportError(
    "The id_mapper module has been removed. "
    "Use id_authority instead: from id_authority import get_authority, prescan_file. "
    "See docs/ID_AUTHORITY_ARCHITECTURE.md for migration guide."
)
