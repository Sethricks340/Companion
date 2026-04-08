using Godot;
using System;




public partial class MainNode : Node2D
{
	//GD.Print("Hello, world!");
	private bool dragging = false;
	private Vector2 offset;
	private Node2D parentScript;
	private Sprite2D _cat;
	
	public override void _Ready()
	{
		//if ((GetParent() as Node2D) == null) GD.Print("parent is null");
		_cat = GetNode<Sprite2D>("Cat");
			
		var script = (GDScript)GD.Load("res://window.gd");
		var instance = (Node)script.New();
		AddChild(instance);
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
					Rect2 localRect = _cat.GetRect();            
					Rect2 globalRect = new Rect2(_cat.GlobalPosition + localRect.Position, localRect.Size);

					if (globalRect.HasPoint(mousePos))
					{
						dragging = true;
						offset = _cat.GlobalPosition - mousePos;
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
			_cat.GlobalPosition = GetGlobalMousePosition() + offset;
		}
	}
}
