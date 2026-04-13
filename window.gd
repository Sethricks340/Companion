extends Node2D

var window
var top_left = Vector2(0, 0)
var bottom_left = Vector2(0, 945)
var top_right = Vector2(1775, 0)
var bottom_right = Vector2(1775, 945)

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
	position.x = clamp(position.x, top_left.x, top_right.x)
	position.y = clamp(position.y, top_left.y, bottom_left.y)

	window.position = position
	
func get_window_position() -> Vector2:
	return window.position
