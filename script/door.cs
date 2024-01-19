using Godot;
using System;

public partial class door : StaticBody2D
{
	private Area2D activationArea;
	private AnimatedSprite2D sprite;
	private CollisionShape2D collision;
	[Export]
	public SpriteFrames DoorAnimation; 
	private bool doorOpen = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		activationArea = GetNode<Area2D>("ActivationArea");
		collision = GetNode<CollisionShape2D>("doorCollision");
		sprite = GetNode<AnimatedSprite2D>("Sprite");
		sprite.SpriteFrames = DoorAnimation;
		
		activationArea.Connect("area_entered", new Callable(this, nameof(onAreaEntered)));
		activationArea.Connect("area_exited", new Callable(this, nameof(onAreaExited)));
		sprite.Connect("animation_finished", new Callable(this, nameof(onAnimationFinished)));
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void onAreaEntered(Area2D area)
	{
		if (area.Name == "Inside") return;
		doorOpen = true;
		sprite.Play("open");
	}
	
	private void onAreaExited(Area2D area)
	{
		if (area.Name == "Inside") return;
		doorOpen = false;
		sprite.PlayBackwards("open");
	}
	
	private void onAnimationFinished()
	{
		collision.SetDeferred("disabled", doorOpen);
	}
	
	
}
