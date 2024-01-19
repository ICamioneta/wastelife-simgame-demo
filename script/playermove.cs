using Godot;
using System;

public partial class playermove : RigidBody2D
{
	public int speed = 100;
	private AnimatedSprite2D sprite;
	// Called when the node enters the scene tree for the first time.	
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 velocity = new Vector2();
		if (Input.IsActionPressed("p_right"))
		{
			velocity.X += 1;
		}
		if (Input.IsActionPressed("p_left"))
		{
			velocity.X -= 1;
		}
		if (Input.IsActionPressed("p_down"))
		{
			velocity.Y += 1;
		}
		if (Input.IsActionPressed("p_up"))
		{
			velocity.Y -= 1;
		}
		
		if (Input.IsActionPressed("p_run"))
		{
			velocity = velocity.Normalized() * (speed * 2);
		} else
		{
			velocity = velocity.Normalized() * speed;
		}
		
		this.LinearVelocity = velocity;
		
		if (velocity.IsZeroApprox())
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
