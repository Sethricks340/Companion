using Godot;
using System;
using System.Collections.Generic;

public partial class MainNode : Node2D
{
	private bool dragging = false;
	private Vector2 offset;
	private Vector2 mousePos ;
	private Vector2 windowPos;
	private Node window_inst;
	private Rect2 globalRect;
	private AnimatedSprite2D cat_animated_sprite;
	private Vector2 top_left;
	private Vector2 bottom_left;
	private Vector2 top_right;
	private Vector2 bottom_right;
	private RandomNumberGenerator rng = new RandomNumberGenerator();
	private List<string> animation_list = new List<string>(){"standing","walking","loafing"};
	private Timer TaskTimer;
	private bool temp_screenshot = false;

	public override void _Ready()
	{
		var window_script = (GDScript)GD.Load("res://window.gd");
		window_inst = (Node)window_script.New();
		AddChild(window_inst);
		
		Vector2 max_pos = window_inst.Call("get_display_size").AsVector2();
		
		top_left = new Vector2(0,0);
		bottom_left = new Vector2(0,max_pos.Y);
		top_right = new Vector2(max_pos.X,0);
		bottom_right = new Vector2(max_pos.X,max_pos.Y);
		
		window_inst.Call("MoveWindowTo", bottom_right); 
		
		cat_animated_sprite = GetNode<AnimatedSprite2D>("cat");
		rng.Randomize();
		TaskTimer = GetNode<Timer>("TaskTimer");
		TaskTimer.Timeout += OnTaskTimerTimeout;
		cat_animated_sprite.Animation = "walking";
		cat_animated_sprite.Play();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				mousePos = DisplayServer.MouseGetPosition();
				windowPos = (Vector2)window_inst.Call("get_window_position");
				globalRect = new Rect2(windowPos, new Vector2(125, 105)); // Current Window Size

				if (globalRect.HasPoint(mousePos))
				{
					dragging = true;
					offset = windowPos - mousePos;
				}
			}
			else
			{
				dragging = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (dragging)
		{
			mousePos = DisplayServer.MouseGetPosition();
			window_inst.Call("MoveWindowTo", mousePos + offset);
		}
		// +(-1,0) -> left // +(1,0) -> right // +(0,1) -> down // +(0,-1) -> up
		window_inst.Call("MoveWindowTo", (Vector2)window_inst.Call("get_window_position") + new Vector2(-1,0)); 
	}	
	
	public void AnimationLogic(){
		//cat_animated_sprite.Animation = "standing";
		//cat_animated_sprite.Animation = "walking";
		//cat_animated_sprite.Animation = animation_list[rng.RandiRange(0, animation_list.Count - 1)];
		//cat_animated_sprite.Play();
	}
	private void OnTaskTimerTimeout()
	{
		AnimationLogic();
		
		if (temp_screenshot) return;
		SaveScreen();
		temp_screenshot = true;
	}
	
	private void SaveScreen()
	{
		if (DisplayServer.GetScreenCount() <= 0)
			return;

		var img = DisplayServer.ScreenGetImage(0);
		if (img == null)
			return;
		img.SavePng(@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame.png");
	}
}
