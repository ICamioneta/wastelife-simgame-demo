namespace WastelifeSimgameDemo.script;
using Godot;

public partial class MapSection : Node2D
{
	public const int TileResolution = 32;
	// very important parameters
	[Export]
	public string NameID { get; private set; }

	[Export]
	public int Width { get; private set; }

	[Export]
	public int Height { get; private set; }

	[Export]
	public int X { get; private set; }

	[Export]
	public int Y { get; private set; }

	
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{
		
	}

	public bool Contains(Vector2 point)
	{
		int px = (int)((long)point.X / TileResolution),
			py = (int)((long)point.Y / TileResolution);
		return px >= X && px <= X + Width && py >= Y && py <= Y + Height;
	}
}
