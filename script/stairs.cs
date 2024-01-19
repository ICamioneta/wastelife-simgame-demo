using Godot;
using System;
using System.Collections.Generic;

public partial class stairs : Node
{
	//updetect shifts the player's layer up if they are not on the stairs. 
	//If they are on the stairs, it shifts the player's layer down.
	//downdetect does the reverse
	//physics bodies are on the stairs if they're in layer 2
	//this code will need an overhaul when wanting to represent any number of floors
	
	private Area2D indetect;
	
	List<RigidBody2D> inStairsList = new List<RigidBody2D>();
	
	public override void _Ready()
	{
		indetect = GetNode<Area2D>("indetect");
		indetect.Connect("area_entered", new Callable(this, nameof(onInEntered)));
		indetect.Connect("area_exited", new Callable(this, nameof(onInExited)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		inStairsList.ForEach(delegate(RigidBody2D body)
		{
			if (body.LinearVelocity.X != 0) //moving right
			{
				body.ApplyCentralForce(new Vector2(0, (-1 * body.LinearVelocity.X)));
				
			} 
		});
	}
	
	
	private void onInEntered(Area2D area)
	{
		GD.Print($"{area.Name} entered stairs");
		if (area.Name == "playerHitbox")
		{
			RigidBody2D player = area.GetParent() as RigidBody2D;
			Area2D playerHitbox = player.GetNode<Area2D>("playerHitbox");
			setCollisions(player, 2);
			setCollisions(playerHitbox, 2);
			inStairsList.Add(player);
		}
	}
	
	private void onInExited(Area2D area)
	{
		GD.Print($"{area.Name} exited stairs");
		if (area.Name == "playerHitbox")
		{
			RigidBody2D player = area.GetParent() as RigidBody2D;
			Area2D playerHitbox = player.GetNode<Area2D>("playerHitbox");
			
			if (player.LinearVelocity.X > 0) //moving right
			{
				setCollisions(player, 3);
				setCollisions(playerHitbox, 3);
			} else if (player.LinearVelocity.X < 0) //moving left
			{
				setCollisions(player, 1);
				setCollisions(playerHitbox, 1);
			}
			inStairsList.Remove(player);
		}
	}
	
	private uint setCollisions(CollisionObject2D thing, uint layer)
	{
		thing.CollisionLayer = layer;
		thing.CollisionMask = layer;
		GD.Print($"{layer} is laer");
		return 1;
	}
}
