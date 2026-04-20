@tool extends EditorPlugin

func _ready() -> void:
	prints('addons/forward_input ready')
	pass

func _input(ev) -> void:
	EditorInterface.get_edited_scene_root().propagate_call("_input", [ev])
	return
