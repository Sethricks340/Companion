extends Node2D

var window

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	# Get access to the actual OS Window (not just the game node)
	window = get_window()

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
	
func MoveWindowTo(position: Vector2) -> void:

	var max_pos = get_display_size()

	position = position.clamp(Vector2.ZERO, max_pos)

	window.position = position
	
func get_window_position() -> Vector2:
	return window.position
	
func get_display_size() -> Vector2:
	var screen_size = Vector2(DisplayServer.screen_get_size(DisplayServer.get_primary_screen()))
	var window_size = Vector2(window.size)

	var max_pos = screen_size - window_size
	return max_pos
