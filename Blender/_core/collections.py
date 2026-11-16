"""Collection creation and deterministic object membership."""
import bpy


def get_or_create_collection(name, parent=None):
    parent = parent or bpy.context.scene.collection
    collection = bpy.data.collections.get(name)
    if collection is None:
        collection = bpy.data.collections.new(name)
    if collection.name not in {child.name for child in parent.children}:
        parent.children.link(collection)
    return collection


def move_objects_to_collection(objects, collection, unlink_others=True):
    for obj in objects:
        if collection.objects.get(obj.name) is None:
            collection.objects.link(obj)
        if unlink_others:
            for old in list(obj.users_collection):
                if old != collection:
                    old.objects.unlink(obj)


def collection_objects_recursive(collection):
    result = list(collection.objects)
    for child in collection.children:
        result.extend(collection_objects_recursive(child))
    # Preserve order while de-duplicating by Blender object identity.
    seen, unique = set(), []
    for obj in result:
        pointer = obj.as_pointer()
        if pointer not in seen:
            seen.add(pointer)
            unique.append(obj)
    return unique
