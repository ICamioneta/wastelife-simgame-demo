using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public partial class Stairs : Node2D
{

	private Area2D bottomArea, topArea;
	
	[Signal]
	public delegate void LayerChangedEventHandler(int floorNumber);
	
	public override void _Ready()
	{
		bottomArea = GetNode<Area2D>("bottomArea");
		topArea = GetNode<Area2D>("topArea");

		bottomArea.Connect("area_entered", new Callable(this, nameof(OnBottomEnter)));
		bottomArea.Connect("area_exited", new Callable(this, nameof(OnBottomExit)));
		topArea.Connect("area_entered", new Callable(this, nameof(OnTopEnter)));
		topArea.Connect("area_exited", new Callable(this, nameof(OnTopExit)));

	}

	public Area2D BottomArea { get { return bottomArea; } }
	public Area2D TopArea { get { return topArea; } }
	
	// oh no, stairs logic!!
	private bool IsMovingLeft(RigidBody2D body)
	{
		GD.Print(body.GetLinearVelocity().X);
		if (body.GetLinearVelocity().X < 0) return true;
		return false;
	}

	private void SetLayers(uint layer, RigidBody2D body)
	{
		// Layer numbers work as an 8 bit array, so a value of 4 is 00000100, activating only the third layer.
		// This system is brilliant if you're not using layers as floors, like I am, since it makes no sense to be on several floors at once.
		// I say that, this system is quite convenient for A.I. vision on lower floors, i think.

		Area2D hitbox;
		
		hitbox = body.GetNode<Area2D>("Hitbox");
		if (hitbox == null) hitbox = body.GetNode<Area2D>("PlayerHitbox");
		if (hitbox == null)
		{
			GD.Print($"SetLayers tried to get a hit box but no valid hitbox names: {body.Name}");
		}
		
		
		GD.Print($"SetLayers: {layer} on {body.Name} and {hitbox.Name}");
		
		body.SetCollisionLayer(layer);
		hitbox.SetCollisionLayer(layer);
		body.SetCollisionMask(layer);
		hitbox.SetCollisionMask(layer);
		
	}
	
	private void OnBottomEnter(Area2D area)
	{
		GD.Print("OnBottomEnter");
		RigidBody2D body = null;
		try
		{
			body = area.GetParent<RigidBody2D>();

			if ( IsMovingLeft(body) )
			{
				
			}
			else
			{
				SetLayers(2, body); // put on stairs layer
				EmitSignal(SignalName.LayerChanged, 2);
			}
		}
		catch // this is _not_ an error
		{
			GD.Print($"area {area.Name} has entered stairs collider but is not attached to a rigid body");
		}
		
	}

	private void OnBottomExit(Area2D area)
	{
		GD.Print("OnBottomExit");
		RigidBody2D body = null;
		try
		{
			body = area.GetParent<RigidBody2D>();

			if ( IsMovingLeft(body) )
			{
				SetLayers(1, body); // ground layer
				EmitSignal(SignalName.LayerChanged, 1);
			}
			else
			{
				SetLayers(2, body); // put on stairs layer, should already be here from OnBottomEnter ??
				EmitSignal(SignalName.LayerChanged, 2);
			}
		}
		catch // this is _not_ an error
		{
			GD.Print($"area {area.Name} has exited stairs collider but is not attached to a rigid body");
		}
	}

	private void OnTopEnter(Area2D area)
	{
		GD.Print("OnTopEnter");
		RigidBody2D body = null;
		try
		{
			body = area.GetParent<RigidBody2D>();

			if ( IsMovingLeft(body) )
			{
				SetLayers(2, body); // put on stairs layer
				EmitSignal(SignalName.LayerChanged, 2);
			}
			else
			{
			}
		}
		catch // this is _not_ an error
		{
			GD.Print($"area {area.Name} has entered stairs collider but is not attached to a rigid body");
		}
	}

	private void OnTopExit(Area2D area)
	{
		GD.Print("OnTopExit");
		RigidBody2D body = null;
		try
		{
			body = area.GetParent<RigidBody2D>();

			if ( IsMovingLeft(body) )
			{
				SetLayers(2, body); // shuold already be here
				EmitSignal(SignalName.LayerChanged, 2);
			}
			else
			{
				SetLayers(4, body); // put on floor 1 layer
				EmitSignal(SignalName.LayerChanged, 4);
			}
		}
		catch // this is _not_ an error
		{
			GD.Print($"area {area.Name} has exited stairs collider but is not attached to a rigid body");
		}
	}
	
	
	
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
	
	/*
		Stairs

		Beams of rock or wood or glass,
		Stretch across from wall to wall,
		From one to other, floors are passed,
		Up these stairs, our feet will crawl.

		On one floor, a man begins,
		Like two lovers, foot and stair kiss,
		Every three steps are creaks and hymns,
		For he who climbs the premises.

		One will wonder how they rise,
		Every step, the ground gets distant,
		Their body lifted to the skies,
		How it works: a question persistent.

		In the beginning, man had a flat,
		A one-storey house hopes for more,
		He cobbled together a jagged track,
		And walked upon the first first floor.

		Stairs are not made just for towers,
		They are useful for all en-hightening,
		They help climb hills and give us power,
		Many materials create their likening.

		Stairs, they lead us to a throne room,
		They raise our speakers so all can hear,
		They take us down into tombs,
		Stairs are something world leaders fear.

		Across each step, we tread with care,
		For a single slip will end in bruises,
		For those stairs claim, we send a prayer,
		In a stair vs. man fight, we are the losers.

		Stairs represent our predicament,
		Our lives climb up then slip and fall,
		When we see stairs, we must confront
		How we answer our mortal call.
	 */
}
