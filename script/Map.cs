using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WastelifeSimgameDemo.script;


public partial class Map : Node2D
{
	private const string PlayerSceneLocation = "res://interactables/Player.tscn";
	[Export] private Vector2 intiialPlayerPosition = new Vector2(320, 320);
	[Export] private Vector2 cameraZoom = new Vector2(3, 3);
	
	private MapLoader _mapLoader;
	private Dictionary<MapSection, Node2D> _loadedGrounds = new(), _loadedObjects = new();
	private List<MapSection> _loadedSections = new();
	private Area2D currentArea;
	private Node2D objects, ground;
	private RigidBody2D player;

	public override void _Ready()
	{
		GD.Print("Map Loaded, preparing...");
		_mapLoader = new MapLoader();
		this.AddChild(_mapLoader);

		this.ground = new();
		this.objects = new();
		this.AddChild(ground);
		this.AddChild(objects);
		objects.YSortEnabled = true;

		currentArea = new();
		currentArea.Visible = false; // remove for debug
		this.AddChild(currentArea);
		CollisionShape2D currentAreaShape = new(); // the rect is to be set in a function called in NextMapSection
		currentAreaShape.Name = "currentAreaShape";
		currentArea.AddChild(currentAreaShape);
		currentArea.AreaExited += CurrentAreaExited;
		
		// player
		PackedScene playerResource = GD.Load<PackedScene>(PlayerSceneLocation);
		player = playerResource.Instantiate<RigidBody2D>();
		
		Camera2D camera  = new();
		camera.Zoom = cameraZoom;
		player.AddChild(camera);
		
		player.Position = intiialPlayerPosition;
		GD.Print($"player position is {player.Position}");
		objects.AddChild(player);
		
		camera.MakeCurrent();
		
		// initial map section
		NextMapSection(player.GlobalPosition);
		
	}

	// I did spend like an hour giving every map section node an Area2D that perfectly aligned with its shape,
	// but I couldn't write a version of this function that seemed efficient enough.
	// So instead of duplicating that Area2D node, I'm just going to use the data in the MapSection class.
	private void SetCurrentArea(MapSection mapSection)
	{
		// safety to ensure the dynamic loading doesn't break
		var hitbox = player.GetNode<Area2D>("PlayerHitbox").GetNode<Node2D>("hitboxshape");
		if (!mapSection.Contains(hitbox.GlobalPosition))
		{
			GD.Print($"map section {mapSection.NameID} does not contain player. " +
					 $"Trying to get the right map section for player position {player.GlobalPosition}");

			mapSection = _mapLoader.GetSectionAt(player.GlobalPosition);
			if (!mapSection.Contains(player.GlobalPosition))
			{
				throw new Exception($"Got section {mapSection.NameID} but it does not contain the player in SetCurrentArea.");
			}
		}

		var currentAreaShape = currentArea.GetNode<CollisionShape2D>("currentAreaShape");
		RectangleShape2D rect = new();
		GD.Print($"setting new area {mapSection.NameID}");
		
		float tr = MapSection.TileResolution;
		float width = mapSection.Width * tr, height = mapSection.Height * tr;
		Vector2 size = new(width, height),
			position = new(mapSection.X * tr, mapSection.Y * tr);
		
		rect.Size = size;
		
		currentAreaShape.CallDeferred("set", "shape", rect);
		currentAreaShape.CallDeferred("set", "position", new Vector2(width / 2, height / 2));
		currentArea.CallDeferred("set", "position", position);

	}

	private void SetCurrentArea()
	{
		MapSection section = _mapLoader.GetSectionAt(player.GlobalPosition);
		SetCurrentArea(section);
	}


	private void PlaceMapSection(MapSection mapSection)
	{
		if (_loadedSections.Contains(mapSection)) return;
		
		Node2D node = _mapLoader.GetSectionNode(mapSection.NameID);
		Vector2 position = new Vector2(mapSection.X * MapSection.TileResolution, mapSection.Y * MapSection.TileResolution);
		
		GD.Print($"Placing map section {mapSection.NameID} at {position.X}, {position.Y}");
		
		Node2D sectionGround = node.GetNode<Node2D>("ground");
		Node2D sectionObjects = node.GetNode<Node2D>("objects");
		
		this.AddChild(node);
		
		node.RemoveChild(sectionGround);
		node.RemoveChild(sectionObjects);

		sectionGround.Owner = null;
		sectionObjects.Owner = null;
		
		sectionGround.Position = position;
		sectionObjects.Position = position;
		
		ground.CallDeferred("add_child", sectionGround);
		objects.CallDeferred("add_child", sectionObjects);
		
		this.RemoveChild(node);
		
		_loadedGrounds.Add(mapSection, sectionGround);
		_loadedObjects.Add(mapSection, sectionObjects);
		_loadedSections.Add(mapSection);

		node.CallDeferred("queue_free");
	}

	private void RemoveMapSection(MapSection mapSection)
	{
		if (!_loadedSections.Contains(mapSection)) return;
		
		GD.Print($"Removing map section {mapSection.NameID}");
			
		ground.CallDeferred("remove_child", _loadedGrounds[mapSection]);
		objects.CallDeferred("remove_child", _loadedObjects[mapSection]);
		
		_loadedGrounds[mapSection].CallDeferred("queue_free");
		_loadedObjects[mapSection].CallDeferred("queue_free");
		
		_loadedGrounds.Remove(mapSection);
		_loadedObjects.Remove(mapSection);
		_loadedSections.Remove(mapSection);
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
		if (!_loadedSections.Contains(currentMapSection)) PlaceMapSection(currentMapSection);
		
		SetCurrentArea(currentMapSection);
		
		// check loaded sections for not neighbours
		foreach (MapSection section in _loadedSections)
		{
			if (section == currentMapSection) continue;
			if (!neighbours.Contains(section)) removeQueue.Add(section);
		}
		
		// check neighbours for sections that need to be loaded
		foreach (MapSection section in neighbours)
		{
			if (!_loadedSections.Contains(section)) addQueue.Add(section);
		}
		
		foreach(MapSection addable in addQueue) PlaceMapSection(addable);
		foreach(MapSection removable in removeQueue) RemoveMapSection(removable);
		
	}

	private void CurrentAreaExited(Area2D area)
	{
		if (area.Name != "PlayerHitbox") return;
		Vector2 globPos = area.GetNode<Node2D>("hitboxshape").GlobalPosition;
		
		GD.Print($"CurrentAreaExited at {globPos}");

		CallDeferred(nameof(NextMapSection), globPos);
	}
}
