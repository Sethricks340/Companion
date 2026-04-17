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
		//cat_animated_sprite.Animation = "walking";
		cat_animated_sprite.Animation = "loafing";
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
		//window_inst.Call("MoveWindowTo", (Vector2)window_inst.Call("get_window_position") + new Vector2(-1,0)); 
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
	private void MoveWindowSafe(Vector2 position)
	{
		window_inst.Call("MoveWindowTo", position);
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
			List<Vector2> lineCenters = FindHorizontalLineCenters(w, h, img);
			// Print line centers debug
			//for (int i = 0; i < lineCenters.Count; i++){
				//GD.Print("line center #" + i + ": " + lineCenters[i]);
			//}
			
			// Can't be called in this thread for some reason
			int random = rng.RandiRange(0, lineCenters.Count - 1);
			GD.Print("random: " + random);
			Vector2 window_offset = new Vector2(-63, -115);
			Vector2 randomLine = lineCenters[random];
			float catW = 150;
			float catH = 135;

			while (
				randomLine.X < catW / 2 ||
				randomLine.Y < catH / 2 ||
				randomLine.X > w - catW / 2 ||
				randomLine.Y > h - catH / 2
			)
			{
				random = rng.RandiRange(0, lineCenters.Count - 1);
				randomLine = lineCenters[random];
			}
			GD.Print("Random Line: " + randomLine);
			CallDeferred(nameof(MoveWindowSafe), randomLine + window_offset);
	}
	
	private List<Vector2> FindHorizontalLineCenters(int w, int h, Image img){
		List<Vector2> lineCenters = new List<Vector2>();
		
			for (int y = 1; y < h; y++){
				for (int x = 1; x < w; x++){
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
			
			// Remove all but the largest color in dictionary
			int largestKey = -1;
			int largestCount = 0;

			foreach (var kv in groups){
				if (kv.Value.Count > largestCount)
				{
					largestCount = kv.Value.Count;
					largestKey = kv.Key;
				}
			}

			var keysToRemove = new List<int>();

			foreach (var kv in groups){
				if (kv.Key != largestKey)
					keysToRemove.Add(kv.Key);
			}

			foreach (var k in keysToRemove){
				groups.Remove(k);
			}
			
			// TODO: this is temp, to visualize the success of this. 
			// TODO: getting weird curves with split screens
			// TODO: maybe only use the largest group for speed
			// If the pixel count for a color is under the threshold, leave it out
			int count = 0;
			int pixel_count_threshold = 100000;
			foreach (var kv in groups){
				if (kv.Value.Count < pixel_count_threshold){
					groups.Remove(kv.Key);
					// return // What if there are none over 100,000
				}
				else{
					//GD.Print("Index: " + count + " Key: " + kv.Key + " Count: " + kv.Value.Count);
					
			 		//We are reconstructing a new image for each significant color, white on a black background
					int targetKey = new List<int>(groups.Keys)[count];
					
					Image newImg = Image.Create(w, h, false, Image.Format.Rgba8);
					newImg.Fill(Colors.Black);

					// Reconstruct a rough image from this color
					foreach (var p in groups[targetKey].Positions)
					{
						newImg.SetPixel((int)p.X, (int)p.Y, Colors.White);
					}
					// This new image is still 1080 x 1920 pixels
					newImg.SavePng($@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame_filter{count}.png");
					
					// Take out any white pixels that are completely surrounded by white (top, bottom, right, left)
					// Take out any white pixels that are completely surrounded by black (top, bottom, right, left)
					// TODO: use dictionary instead of new image?
					Image edgeImg = Image.Create(w, h, false, Image.Format.Rgba8);
					edgeImg.Fill(Colors.Black);

					for (int y2 = 1; y2 < h - 1; y2++)
					{
						for (int x2 = 1; x2 < w - 1; x2++)
						{
							if (newImg.GetPixel(x2, y2) == Colors.White)
							{
								bool up    = newImg.GetPixel(x2, y2 - 1) == Colors.White;
								bool down  = newImg.GetPixel(x2, y2 + 1) == Colors.White;
								bool left  = newImg.GetPixel(x2 - 1, y2) == Colors.White;
								bool right = newImg.GetPixel(x2 + 1, y2) == Colors.White;

								int whiteNeighbors = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

								// Keep only edge pixels:
								// - NOT fully surrounded by white (removes interior)
								// - NOT isolated (removes noise)
								// - DONT have a horizontal neighboor
								if (whiteNeighbors > 0 && whiteNeighbors < 4 && (left || right))
								{
									edgeImg.SetPixel(x2, y2, Colors.White);
								}
							}
						}
					}
					
					// Make new image and save	
					edgeImg.SavePng($@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame_edges{count}.png");
					
					Image filteredImg = Image.Create(w, h, false, Image.Format.Rgba8);
					filteredImg.Fill(Colors.Black);

					for (int y2 = 1; y2 < h - 1; y2++)
					{
						for (int x2 = 1; x2 < w - 1; x2++)
						{
							if (edgeImg.GetPixel(x2, y2) == Colors.White)
							{
								bool up    = edgeImg.GetPixel(x2, y2 - 1) == Colors.White;
								bool down  = edgeImg.GetPixel(x2, y2 + 1) == Colors.White;
								bool left  = edgeImg.GetPixel(x2 - 1, y2) == Colors.White;
								bool right = edgeImg.GetPixel(x2 + 1, y2) == Colors.White;

								// keep only horizontal edges that have black on top and bottom, and white on left or right
								if ((left || right) && (!up && !down))
								{
									filteredImg.SetPixel(x2, y2, Colors.White);
								}
							}
						}
					}

					// Now we have all the one-pixel width horizontal edges.	
					filteredImg.SavePng($@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame_edges_filtered{count}.png");
						
						
					// Now remove any horizontal edges that are less than 200 pixels long
					bool[,] visited = new bool[w, h];

					for (int y2 = 0; y2 < h; y2++)
					{
						for (int x2 = 0; x2 < w; x2++)
						{
							if (filteredImg.GetPixel(x2, y2) != Colors.White || visited[x2, y2])
								continue;

							Queue<Vector2> q = new Queue<Vector2>();
							List<Vector2> segment = new List<Vector2>();

							q.Enqueue(new Vector2(x2, y2));
							visited[x2, y2] = true;

							while (q.Count > 0)
							{
								var p = q.Dequeue();
								segment.Add(p);

								int px = (int)p.X;
								int py = (int)p.Y;

								// horizontal-only connectivity
								int[,] dirs = { { -1, 0 }, { 1, 0 } };

								for (int i = 0; i < 2; i++)
								{
									int nx = px + dirs[i, 0];
									int ny = py + dirs[i, 1];

									if (nx < 0 || ny < 0 || nx >= w || ny >= h)
										continue;

									if (visited[nx, ny])
										continue;

									if (filteredImg.GetPixel(nx, ny) == Colors.White)
									{
										visited[nx, ny] = true;
										q.Enqueue(new Vector2(nx, ny));
									}
								}
							}

							// delete short segments
							if (segment.Count >= 200)
							{
								// pick midpoint of the segment
								Vector2 sum = Vector2.Zero;

								foreach (var p in segment)
									sum += p;

								Vector2 center = sum / segment.Count;

								//GD.Print("Line center: " + center);
								lineCenters.Add(center);
							}
							else
							{
								foreach (var p in segment)
									filteredImg.SetPixel((int)p.X, (int)p.Y, Colors.Black);
							}
						}
					}
					// Add small red dot in the center of each line, this is the reference loafpoint
					foreach (var c in lineCenters)
					{
						int x = (int)c.X;
						int y = (int)c.Y;

						if (x >= 0 && x < w && y >= 0 && y < h)
						{
							filteredImg.SetPixel(x, y, Colors.Red);
						}
					}
					filteredImg.SavePng($@"C:\Users\sethr\backup\Desktop\Companion\companion\temp_screenshot\frame_edges_filtered_final{count}.png");
					
					count++;
				}
			}

			return lineCenters;
	}
}
