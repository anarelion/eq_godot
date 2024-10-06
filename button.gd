extends Button

func _on_button_pressed():
	get_tree().current_scene.Resources.InstantiateActor("bam")
	print("Instantiating bam")
