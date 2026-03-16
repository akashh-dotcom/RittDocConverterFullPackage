"""
DEPRECATED: id_tracker module has been removed.

All ID tracking functionality has been consolidated into id_authority.py.

MIGRATION GUIDE:
----------------
Old (id_tracker):                    New (id_authority):
-----------------                    -------------------
from id_tracker import IDTracker     from id_authority import get_authority
tracker = IDTracker()                authority = get_authority()
tracker.register_source_id(...)      authority.map_id(...)
tracker.map_id(...)                  authority.map_id(...)
tracker.mark_dropped(...)            authority.mark_dropped(...)
tracker.get_xml_id(...)              authority.resolve_linkend(...)
tracker.save_to_json(path)           authority.export_registry(path)
tracker.get_stats()                  authority.get_stats()

For complete documentation, see: docs/ID_AUTHORITY_ARCHITECTURE.md
"""

raise ImportError(
    "The id_tracker module has been removed. "
    "Use id_authority instead: from id_authority import get_authority. "
    "See docs/ID_AUTHORITY_ARCHITECTURE.md for migration guide."
)
