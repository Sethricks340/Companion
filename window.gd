extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	# Get access to the actual OS Window (not just the game node)
	var window = get_window()

	# 1. TRANSPARENCY SETUP
	# We enable transparency for both the Godot Viewport and the OS Window
	get_viewport().transparent_bg = true
	window.transparent = true

	# 2. WINDOW SHAPE
	# We remove the borders so it looks like the character is floating
	window.borderless = true

	# Keep the sprite above your web browser/other apps
	window.always_on_top = true

	# Force Windows to relax and let us be borderless
	window.unresizable = false
