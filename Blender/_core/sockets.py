"""Attachment/interaction socket conventions using Empty objects."""
SOCKET_PREFIX = "SOCKET_"


def find_sockets(objects):
    return sorted(
        [obj for obj in objects if obj.type == "EMPTY" and obj.name.startswith(SOCKET_PREFIX)],
        key=lambda obj: obj.name
    )


def validate_socket_names(objects):
    names = [obj.name for obj in find_sockets(objects)]
    duplicates = sorted({name for name in names if names.count(name) > 1})
    return [f"Nom de socket dupliqué : {name}" for name in duplicates]
