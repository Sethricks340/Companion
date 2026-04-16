using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

class PixelGroup
{
	public int Count = 0;
	public List<Vector2> Positions = new List<Vector2>();
}

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
	private Godot.Timer TaskTimer;
	private Color[] pixels;
	private Dictionary<int, PixelGroup> groups = new Dictionary<int, PixelGroup>();

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
		TaskTimer = GetNode<Godot.Timer>("TaskTimer");
		TaskTimer.Timeout += OnTaskTimerTimeout;
		cat_animated_sprite.Animation = "walking";
		cat_animated_sprite.Play();
		
		var img = DisplayServer.ScreenGetImage(0);
		pixels = new Color[img.GetWidth() * img.GetHeight()];
		
		// Save Screen is in a new thread so it doesn't interrupt the animations
		Thread thread = new Thread(new ThreadStart(SaveScreen));
		thread.Start();
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
	
	private void OnTaskTimerTimeout()
	{
		AnimationLogic();
	}
	public void AnimationLogic(){
		// TODO: right now it is just set to walking, will need to change animations
		//cat_animated_sprite.Animation = "standing";
		//cat_animated_sprite.Animation = "walking";
		//cat_animated_sprite.Animation = animation_list[rng.RandiRange(0, animation_list.Count - 1)];
		//cat_animated_sprite.Play();
	}
	private void SaveScreen()
	{
		System.Threading.Thread.Sleep(500); //wait till cat is loaded

			// Make sure there is a screen
			if (DisplayServer.GetScreenCount() <= 0)
				return;

			// Make sure image is valid
			var img = DisplayServer.ScreenGetImage(0);
			if (img == null)
				return;
			
			// Temp save original image
			img.SavePng(@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame.png");
			
			// Get image width and height (screenshot of whole screen, NOT just godot window)
			//GD.Print("Image Height x Width: " + img.GetHeight() + " x " + img.GetWidth()); // My Output: Image Height x Width: 1080 x 1920
			int w = img.GetWidth();
			int h = img.GetHeight();

			for (int y = 1; y < h - 1; y++){
				for (int x = 1; x < w - 1; x++){
					// for each pixel in the image, get the raw pixel values. (R,G,B,A) (A = transparency)
					Color c = img.GetPixel(x, y);
					// Store in the one dimensional array
					pixels[y * w + x] = c;

					// This clusters similar colors together. 512 total combos
					int r = (int)(c.R * 8); 
					int g = (int)(c.G * 8);
					int b = (int)(c.B * 8);

					int key = (r << 16) | (g << 8) | b; // Combine the rgb information into one key

					// No duplicate keys
					if (!groups.ContainsKey(key)) groups[key] = new PixelGroup();

					groups[key].Count++; // Increase the number of this color found
					groups[key].Positions.Add(new Vector2(x, y)); // Add the position of this pixel to this dictionary key
				}
			}
			
			// If the pixel count for a color is under the threshold, leave it out
			int pixel_count_threshold = 10000; //(WAS 1000)
			foreach (var key in new List<int>(groups.Keys))
			{
				if (groups[key].Count < pixel_count_threshold)
				{
					groups.Remove(key);
				}
			}
			
			// TODO: this is temp, to visualize the success of this. 
			// Display all the items in the pixel dictionary
			int count = 0;
			foreach (var kv in groups)
			{
				GD.Print("Index: " + count++ + " Key: " + kv.Key + " Count: " + kv.Value.Count);
			}
			GD.Print("Total # of groups: " + groups.Count);

			// TODO: this is temp, to visualize the success of this. 
			// We are reconstructing a new image for each significant color, white on a black background
			for (int image_index = 0; image_index < groups.Count; image_index++){
				int targetKey = new List<int>(groups.Keys)[image_index];
				
				Image newImg = Image.Create(w, h, false, Image.Format.Rgba8);
				newImg.Fill(Colors.Black);

				// Reconstruct a rough image from this color
				foreach (var p in groups[targetKey].Positions)
				{
					newImg.SetPixel((int)p.X, (int)p.Y, Colors.White);
				}
				newImg.SavePng($@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame_filter{image_index}.png");
			}
	}
}
