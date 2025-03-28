using System.Collections.Generic;
using Godot;

namespace WastelifeSimgameDemo.script;


public partial class Map : Node2D
{
	private const string PlayerSceneLocation = "res://Interactables/Player.tscn";
	[Export] private Vector2 playerPosition = new Vector2(320, 320);
	
	private MapLoader _mapLoader;
	private Dictionary<Vector2, Node2D> _loadedScene = new();

	public override void _Ready()
	{
		GD.Print("Map Loaded, preparing...");
		_mapLoader = new MapLoader();
		this.AddChild(_mapLoader);
		RigidBody2D player = InstantiatePlayer(PlayerSceneLocation);
		player.Position = playerPosition;
		GD.Print($"player position is {player.Position}");
		this.AddChild(player);
		
		List<MapSection> mapSections = new();
		MapSection initialTile = _mapLoader.GetSectionAt(playerPosition);
		mapSections.Add(initialTile);
		var initialNeighbours = _mapLoader.GetNeighbours(initialTile);
		mapSections.AddRange(initialNeighbours);
			
		foreach (MapSection mapSection in mapSections)
		{
			PlaceMapSection(mapSection);
		}
	}

	private RigidBody2D InstantiatePlayer(string playerPath)
	{
		PackedScene playerResource = GD.Load<PackedScene>(playerPath);

		RigidBody2D player = playerResource.Instantiate<RigidBody2D>();
		Camera2D camera  = new();
		camera.Zoom = new Vector2(3, 3);
		player.AddChild(camera);
		
		return player;
	}

	private void PlaceMapSection(MapSection mapSection)
	{
		GD.Print($"Placing map section {mapSection.NameID}");
		Node2D node = _mapLoader.GetSectionNode(mapSection.NameID);
		node.Position = new Vector2(mapSection.X * MapSection.TileResolution, mapSection.Y * MapSection.TileResolution);
		this.AddChild(node);
	}
	
	
}
