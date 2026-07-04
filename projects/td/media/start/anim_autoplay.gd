@tool extends AnimationPlayer
func _enter_tree() -> void:
	var animList: PackedStringArray = get_animation_list()
	var first := animList[0]
	var anim: Animation = get_animation(first)
	
	anim.loop_mode = Animation.LOOP_LINEAR
	current_animation = first
	if(autoplay == &""):
		autoplay = first
		pass
	return
