extends Button

func _on_button_pressed():
	get_tree().current_scene._resources.InstantiateCharacter("it149")
	# get_tree().current_scene.Resources.InstantiateZone()
