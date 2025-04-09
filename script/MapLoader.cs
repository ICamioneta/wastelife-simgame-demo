using System.Collections.Generic;
using System.IO;
using System.Net;
using Godot;

namespace WastelifeSimgameDemo.script;

public partial class MapLoader : Node
{
	private readonly Dictionary<string, PackedScene> sectionCache = new();
	//private readonly List<MapSection> mapSections = new();
	public const string MapSectionPath = "res://assets/sections/";
	
	// Spacial hashing stuff ik ik
	private const int CellSize = 128;
	private readonly Dictionary<(int, int), List<MapSection>> cells = new();
	private Dictionary<MapSection, List<MapSection>> neighbourCache = new();

	public MapLoader()
	{
		PreloadAllSections();
	}
	public override void _Ready()
	{
		// PreloadAllSections();
	}

	private void PreloadAllSections()
	{
		GD.Print("Preloading all sections...");
		sectionCache.Clear();
		
		using var dir = DirAccess.Open(MapSectionPath);
		if (dir == null) throw new FileNotFoundException("Map section folder not found");

		dir.ListDirBegin();
		string filename;
		
		while ((filename = dir.GetNext()) != "")
		{
			
			if (filename.EndsWith(".tscn"))
			{
				string scenePath = MapSectionPath + filename;
				var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
				if (packedScene == null) continue;
				
				Node tempInstance = packedScene.Instantiate();
				if (tempInstance is MapSection mapSection)
				{
					string nameID = mapSection.NameID;
					sectionCache[nameID] = packedScene;
					AddSectionToGrid(mapSection);
				}
				tempInstance.QueueFree();
			}
			
		}
	}

	private void AddSectionToGrid(MapSection section)
	{
		int leftX = section.X / CellSize,
			rightX = (section.X + section.Width-1) / CellSize,
			topY = section.Y / CellSize,
			bottomY = (section.Y + section.Height-1) / CellSize;
		var positions = new HashSet<(int, int)>
		{
			(leftX, topY),
			(rightX, topY),
			(leftX, bottomY),
			(rightX, bottomY)
		};

		foreach (var cellkey in positions)
		{
			if (!cells.TryGetValue(cellkey, out var sections))
			{
				sections = new List<MapSection>();
				cells[cellkey] = sections;
			}
			
			if (!sections.Contains(section)) sections.Add(section);
		}
	}

	public Node2D GetSectionNode(string nameID)
	{
		if (sectionCache.TryGetValue(nameID, out PackedScene section))
		{
			return (Node2D)section.Instantiate();
		}
		return null;
	}
	
	// takes grid coords
	public MapSection GetSectionAt(int gridX, int gridY, bool report)
	{
		int cellX = gridX / CellSize,
			cellY = gridY / CellSize;

		if (cells.TryGetValue((cellX, cellY), out var sections))
		{
			foreach (MapSection section in sections)
			{
				int secX = section.X, secY = section.Y;
				int secMaxX = secX + section.Width - 1, secMaxY = secY + section.Height - 1;
			
				if (gridX >= secX && gridX <= secMaxX && gridY >= secY && gridY <= secMaxY)
				{ 
					if (report) GD.Print($"Got section {section.NameID} at ({gridX}, {gridY})");
					return section;
				}
			}
		}
		if (report) GD.Print($"No section found for {gridX},{gridY}");
		return null;
	}
	public MapSection GetSectionAt(int x, int y) => GetSectionAt(x, y, true);
	
	// Function version for unmodified x,y coords.
	public MapSection GetSectionAt(Vector2 v) 
		=> GetSectionAt((int)v.X / MapSection.TileResolution, (int)v.Y / MapSection.TileResolution);
		
	// instance versions of GetSectionAt
	public Node2D GetSectionNodeAt(int gridX, int gridY)
	{
		return sectionCache[GetSectionAt(gridX, gridY).NameID].Instantiate<Node2D>();
	}
	
	public Node2D GetSectionNodeAt(Vector2 v) 
		=> GetSectionNodeAt((int)v.X * MapSection.TileResolution, (int)v.Y * MapSection.TileResolution);

	public List<MapSection> GetNeighbours(MapSection section)
	{
		if (neighbourCache.TryGetValue(section, out var neighboursList))
			return neighboursList;
		
		// if it isn't cached, find it and add it to the cache for next time
		var neighbours = FindNeighbours(section);
		if (neighbourCache.ContainsKey(section)) neighbourCache.Remove(section);
		neighbourCache.Add(section, neighbours);

		return neighbours;
	}

	public List<Node2D> GetNeighbourNodes(MapSection section)
	{
		List<Node2D> neighbourNodes = new();
		List<MapSection> neighbours = GetNeighbours(section);
		foreach (MapSection neighbour in neighbours)
		{
			neighbourNodes.Add(sectionCache[neighbour.NameID].Instantiate<Node2D>());
		}
		return neighbourNodes;
	}

	// scans the edges of the mapsection, adding neighbours to the list whenever they're found.
	private List<MapSection> FindNeighbours(MapSection section)
	{
		List<MapSection> neighbours = new();
		int x = (int)section.X,
			y = (int)section.Y;
		
		GD.Print($"scanning for {section.NameID}, from x={x-1}, y={y-1} to {x + section.Width} {y + section.Height}");
		
		// Scan top edge, includes diagonal adjacency but this shouldn't matter with my tile layout
		for (int i = x - 1; i < x + section.Width + 1; i++)
		{
			var neighbour = GetSectionAt(i, y - 1, false);
			if (neighbour != null && !neighbours.Contains(neighbour))
			{
				GD.Print($"Found neighbour {neighbour.NameID} at {i}, {y-1}");
				neighbours.Add(neighbour);
			}
		}
		
		// Left edge (from top to bottom)
		for (int i = y - 1; i < y + section.Height + 1; i++)
		{
			var neighbour = GetSectionAt(x - 1, i, false);
			if (neighbour != null && !neighbours.Contains(neighbour))
			{
				GD.Print($"Found neighbour {neighbour.NameID} at {x-1}, {i}");
				neighbours.Add(neighbour);
			}
		}
		
		// Right edge (top to bottom)
		for (int i = y - 1; i < y + section.Height + 1; i++)
		{
			var neighbour = GetSectionAt(x + section.Width + 1, i, false);
			if (neighbour != null && !neighbours.Contains(neighbour))
			{
				GD.Print($"Found neighbour {neighbour.NameID} at {x + section.Width + 1}, {i}");
				neighbours.Add(neighbour);
			}
		}
		
		// Bottom edge
		for (int i = x - 1; i < x + section.Width + 1; i++)
		{
			var neighbour = GetSectionAt(i, y + section.Height + 1, false);
			if (neighbour != null && !neighbours.Contains(neighbour))
			{
				GD.Print($"Found neighbour {neighbour.NameID} at {i}, {y + section.Height + 1}");
				neighbours.Add(neighbour);
			}
		}
		
		return neighbours;
	}
	
}
