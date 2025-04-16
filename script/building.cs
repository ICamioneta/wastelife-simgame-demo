using Godot;
using System;

public partial class building : Node2D
{
	private Node2D buildingExterior; // tilemaplayer
	private Node2D floor0, floor1, floor2; // tilemaplayer, need to add more by hand
	private Stairs stairs0, stairs1; // n-floors would be a nightmare. Probably won't ever happen... 
	private door door; // I'll handle n doors later...
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//connect children
		buildingExterior = GetNode<Node2D>("BuildingExterior");
		floor0 = GetNode<Node2D>("Floor0");
		if (HasNode("Floor1")) floor1 = GetNode<Node2D>("Floor1");

		door = GetNode<door>("Door");
		if (HasNode("Floor0/Stairs")) stairs0 = GetNode<Stairs>("Floor0/Stairs");
		
		// function connects
		door.EnteredInside += ShowFloor0;
		door.EnteredOutside += ShowExterior;

		if (stairs0 != null) stairs0.PlayerLayerChanged += LayerChange;
		
		// establish visibility
		buildingExterior.Visible = true;
		floor0.Visible = false;
		if (floor1 != null) floor1.Visible = false;
	}

	private void LayerChange(int layer)
	{
		if (layer == 1) ShowFloor0();
		if (layer == 2) ShowFloor0();
		if (layer == 4) ShowFloor1();
	}
	private void ShowFloor0()
	{
		buildingExterior.Visible = false;
		floor0.Visible = true;
		if (floor1 != null) floor1.Visible = false;
		if (floor2 != null) floor2.Visible = false;
	}

	private void ShowFloor1()
	{
		buildingExterior.Visible = false;
		floor0.Visible = true;
		floor1.Visible = true;
		if (floor2 != null) floor2.Visible = false;
	}

	private void ShowExterior()
	{
		buildingExterior.Visible = true;
		floor0.Visible = false;
		if (floor1 != null) floor1.Visible = false;
		if (floor2 != null) floor2.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
