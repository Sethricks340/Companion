using Godot;
using System;

public partial class MainNode : Node2D
{
	private bool dragging = false;
	private Vector2 offset;
	private Vector2 mousePos ;
	private Node window_inst;
	private Rect2 globalRect;

	public override void _Ready()
	{
		var window_script = (GDScript)GD.Load("res://window.gd");
		window_inst = (Node)window_script.New();
		AddChild(window_inst);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				mousePos = DisplayServer.MouseGetPosition();
				globalRect = new Rect2((Vector2)window_inst.Call("get_window_position"), new Vector2(130, 130));

				if (globalRect.HasPoint(mousePos))
				{
					dragging = true;
					offset = (Vector2)window_inst.Call("get_window_position") - mousePos;
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
	}	
}
