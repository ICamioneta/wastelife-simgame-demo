using Godot;
using System;

public partial class playermove : RigidBody2D
{
	public int speed = 50;
	int accel = 5;
	private AnimatedSprite2D sprite;
	// Called when the node enters the scene tree for the first time.	
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		Vector2 targetVelocity = new Vector2();

		if (Input.IsActionPressed("p_right"))
		{
			targetVelocity.X += 1;
		}
		if (Input.IsActionPressed("p_left"))
		{
			targetVelocity.X -= 1;
		}
		if (Input.IsActionPressed("p_down"))
		{
			targetVelocity.Y += 1;
		}
		if (Input.IsActionPressed("p_up"))
		{
			targetVelocity.Y -= 1;
		}

		// Normalize the target velocity and scale it by speed
		if (Input.IsActionPressed("p_run"))
		{
			targetVelocity = targetVelocity.Normalized() * (speed * 2);
		}
		else
		{
			targetVelocity = targetVelocity.Normalized() * speed;
		}

		// Calculate the force needed to reach the target velocity
		Vector2 currentVelocity = state.LinearVelocity;
		Vector2 force = (targetVelocity - currentVelocity) * accel;

		// Apply the force to the body
		state.ApplyForce(force);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var velocity = this.LinearVelocity;
		
		if (Math.Abs(velocity.X) < 25 && Math.Abs(velocity.Y) < 25)
		{
			sprite.Pause();
		} else 
		{
			float direction = velocity.Angle();
			if (direction >= -Math.PI / 8 && direction < Math.PI / 8)
			{
				sprite.Play("walkright");
			} else if (direction >= Math.PI / 8 && direction < 3 * Math.PI / 8)
			{
				sprite.Play("walkdownright");
			} else if (direction >= 3 * Math.PI / 8 && direction < 5 * Math.PI / 8)
			{
				sprite.Play("walkdown");
			} else if (direction >= 5 * Math.PI / 8 && direction < 7 * Math.PI / 8)
			{
				sprite.Play("walkdownleft");
			} else if (direction >= 7 * Math.PI / 8 || direction < -7 * Math.PI / 8)
			{
				sprite.Play("walkleft");
			} else if (direction >= -7 * Math.PI / 8 && direction < -5 * Math.PI / 8)
			{
			sprite.Play("walkupleft");
			} else if (direction >= -5 * Math.PI / 8 && direction < -3 * Math.PI / 8)
			{
				sprite.Play("walkup");
			} else
			{
				sprite.Play("walkupright");
			}
		}
	}
}
