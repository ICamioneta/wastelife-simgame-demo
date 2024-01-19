using Godot;
using System;

public partial class buildingEnter : Node2D
{
	private TileMap buildingExterior;
	private TileMap floor1, floor2;
	private Area2D inside, inside2, inside3;
	
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		buildingExterior = GetNode<TileMap>("BuildingExterior");
		floor1 = GetNode<TileMap>("Floor1");
		if (HasNode("Floor2")) floor2 = GetNode<TileMap>("Floor2");
		inside = GetNode<Area2D>("Inside");
		if (HasNode("Inside2")) inside2 = GetNode<Area2D>("Inside2");
		if (HasNode("Inside3")) inside3 = GetNode<Area2D>("Inside3");
		
		inside.Connect("area_entered", new Callable(this, nameof(onInsideEntered)));
		inside.Connect("area_exited", new Callable(this, nameof(onInsideExited)));
		if (inside2 != null) inside2.Connect("area_entered", new Callable(this, nameof(onInside2Entered)));
		if (inside2 != null) inside2.Connect("area_exited", new Callable(this, nameof(onInside2Exited)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void onInsideEntered(Area2D area)
	{
		GD.Print($"building floor1 entered by {area.Name}");
		if (area.Name == "playerHitbox")
		{
			buildingExterior.Visible = false;
			floor1.SetLayerEnabled(1, true);
			if (floor2 != null) floor2.Visible = false;
		}
	}
	
	private void onInsideExited(Area2D area)
	{
		GD.Print($"building floor1 exited by {area.Name}");
		if (area.Name == "playerHitbox")
		{
			buildingExterior.Visible = true;
			floor1.SetLayerEnabled(1, false);
			if (floor2 != null) floor2.Visible = true;
		}
	}
	
	private void onInside2Entered(Area2D area)
	{
		GD.Print($"building floor2 exited by {area.Name}");
		if (area.Name == "playerHitbox")
		{
			buildingExterior.Visible = false;
			floor2.SetLayerEnabled(1, true);
		}
	}
	
	private void onInside2Exited(Area2D area)
	{
		GD.Print($"building floor2 exited by {area.Name}");
		if (area.Name == "playerHitbox")
		{
			buildingExterior.Visible = true;
			floor2.SetLayerEnabled(1, false);
		}
	}
}
