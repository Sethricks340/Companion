using Godot;
using System;

public partial class MainNode : Node2D
{
	//GD.Print("Hello, world!");
	private bool dragging = false;
	private Vector2 offset;
	private Node2D parentScript;
	private Sprite2D _cat;
	private Node window_inst;
	
	public override void _Ready()
	{
		//if ((GetParent() as Node2D) == null) GD.Print("parent is null");
		_cat = GetNode<Sprite2D>("Cat");
			
		var window_script = (GDScript)GD.Load("res://window.gd");
		window_inst = (Node)window_script.New();
		AddChild(window_inst);

		//window_inst.Call("MoveWindowTo", new Vector2(0, 0));
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.Pressed)
				{
					// Check if mouse is over the sprite
					Vector2 mousePos = GetGlobalMousePosition();
					GD.Print("mousePos: " + mousePos);
					Rect2 localRect = _cat.GetRect();            
					Rect2 globalRect = new Rect2(_cat.GlobalPosition + localRect.Position, localRect.Size);
					GD.Print("globalRect: " + globalRect);
					
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
	}

	public override void _Process(double delta)
	{		
		if (dragging)
		{
			window_inst.Call("MoveWindowTo", GetGlobalMousePosition() + offset);
		}
	}
}
