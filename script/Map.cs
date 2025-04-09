using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WastelifeSimgameDemo.script;


public partial class Map : Node2D
{
	private const string PlayerSceneLocation = "res://Interactables/Player.tscn";
	[Export] private Vector2 intiialPlayerPosition = new Vector2(320, 320);
	[Export] private Vector2 cameraZoom = new Vector2(3, 3);
	
	private MapLoader _mapLoader;
	private Dictionary<MapSection, Node2D> _loadedSections = new();
	private Area2D currentArea;

	public override void _Ready()
	{
		GD.Print("Map Loaded, preparing...");
		_mapLoader = new MapLoader();
		this.AddChild(_mapLoader);
		RigidBody2D player = InstantiatePlayer(PlayerSceneLocation);
		player.Position = intiialPlayerPosition;
		GD.Print($"player position is {player.Position}");
		this.AddChild(player);
		
		List<MapSection> mapSections = new();
		MapSection initialTile = _mapLoader.GetSectionAt(intiialPlayerPosition);
		PlaceMapSection(initialTile);
		
		currentArea = _loadedSections[initialTile].GetNode<Area2D>("Area");
		currentArea.AreaExited += CurrentAreaExited;
		
		var initialNeighbours = _mapLoader.GetNeighbours(initialTile);
		mapSections.AddRange(initialNeighbours);	
			
		foreach (MapSection mapSection in mapSections)
		{
			PlaceMapSection(mapSection);
		}
	}

	private void SetCurrentArea(MapSection mapSection)
	{
		if (!_loadedSections.ContainsKey(mapSection)) return;
		if (currentArea != null) currentArea.AreaExited -= CurrentAreaExited;

		currentArea = _loadedSections[mapSection].GetNode<Area2D>("Area");
		currentArea.AreaExited += CurrentAreaExited;
	}
	
	private RigidBody2D InstantiatePlayer(string playerPath)
	{
		PackedScene playerResource = GD.Load<PackedScene>(playerPath);

		RigidBody2D player = playerResource.Instantiate<RigidBody2D>();
		Camera2D camera  = new();
		camera.Zoom = cameraZoom;
		player.AddChild(camera);
		
		return player;
	}

	private void PlaceMapSection(MapSection mapSection)
	{
		if (_loadedSections.ContainsKey(mapSection)) return;
		
		GD.Print($"Placing map section {mapSection.NameID}");
		Node2D node = _mapLoader.GetSectionNode(mapSection.NameID);
		node.Position = new Vector2(mapSection.X * MapSection.TileResolution, mapSection.Y * MapSection.TileResolution);
		
		CallDeferred("add_child", node);
		_loadedSections.Add(mapSection, node);
	}

	private void RemoveMapSection(MapSection mapSection)
	{
		if (_loadedSections.TryGetValue(mapSection, out var node))
		{
			GD.Print($"Removing map section {mapSection.NameID}");
			CallDeferred("remove_child", node);
			node.CallDeferred("queue_free");
			_loadedSections.Remove(mapSection);
		}
		
	}

	// dynamic loading function: loads sections if they need to be loaded and removes them if they're no longer neighbours.
	private void NextMapSection(Vector2 pos)
	{
		MapSection currentMapSection = _mapLoader.GetSectionAt(pos);
		GD.Print(currentMapSection.NameID);
		List<MapSection> neighbours = _mapLoader.GetNeighbours(currentMapSection);
		List<MapSection> addQueue = new();
		List<MapSection> removeQueue = new();
		
		// catch in case the next section isn't already loaded.
		if (!_loadedSections.ContainsKey(currentMapSection)) PlaceMapSection(currentMapSection);
		
		SetCurrentArea(currentMapSection);
		
		// check loaded sections for not neighbours
		foreach (MapSection section in _loadedSections.Keys)
		{
			if (section == currentMapSection) continue;
			if (!neighbours.Contains(section)) removeQueue.Add(section);
		}
		
		// check neighbours for sections that need to be loaded
		foreach (MapSection section in neighbours)
		{
			if (!_loadedSections.Keys.Contains(section)) addQueue.Add(section);
		}
		
		
		foreach(MapSection addable in addQueue) PlaceMapSection(addable);
		foreach(MapSection removable in removeQueue) RemoveMapSection(removable);
		
	}

	private void CurrentAreaExited(Area2D area)
	{
		if (area.Name != "PlayerHitbox") return;
		Vector2 globPos = area.GetNode<Node2D>("hitboxshape").GlobalPosition;
		
		RigidBody2D player = area.GetParent<RigidBody2D>();
		GD.Print($"CurrentAreaExited at {globPos}");
		
		NextMapSection(globPos);
	}
	
	
}
