extends Button

func _on_button_pressed():
	get_tree().current_scene.Resources.InstantiateActor("bam")
	get_tree().current_scene.Resources.InstantiateZone()
	print("Instantiating bam")
