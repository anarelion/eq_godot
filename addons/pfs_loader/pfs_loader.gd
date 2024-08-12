@tool
extends EditorPlugin


func _enter_tree() -> void:
	# Initialization of the plugin goes here.
	print
	ResourceLoader.add_resource_format_loader(PackFileLoader.new())
	


func _exit_tree() -> void:
	# Clean-up of the plugin goes here.
	pass
