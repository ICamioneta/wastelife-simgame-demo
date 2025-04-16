using Godot;
using System;

public partial class door : StaticBody2D
{
	private Area2D activationInside, activationOutside;
	private AnimatedSprite2D sprite;
	private CollisionShape2D collision;
	[Export]
	public SpriteFrames DoorAnimation; 
	private bool doorOpen = false, outsideInteractable = false, insideInteractable = false;

	[Signal]
	public delegate void EnteredInsideEventHandler();
	[Signal]
	public delegate void ExitedInsideEventHandler();
	[Signal]
	public delegate void EnteredOutsideEventHandler();
	[Signal]
	public delegate void ExitedOutsideEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		activationInside = GetNode<Area2D>("ActivationInside");
		activationOutside = GetNode<Area2D>("ActivationOutside");
		collision = GetNode<CollisionShape2D>("doorCollision");
		sprite = GetNode<AnimatedSprite2D>("Sprite");
		sprite.SpriteFrames = DoorAnimation;
		
		activationInside.Connect("area_entered", new Callable(this, nameof(OnInsideAreaEntered)));
		activationInside.Connect("area_exited", new Callable(this, nameof(OnInsideAreaExited)));
		activationOutside.Connect("area_entered", new Callable(this, nameof(OnOutsideAreaEntered)));
		activationOutside.Connect("area_exited", new Callable(this, nameof(OnOutsideAreaExited)));
		sprite.Connect("animation_finished", new Callable(this, nameof(onAnimationFinished)));
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (insideInteractable)
		{
			if (Input.IsActionJustPressed("p_interact"))
			{
				ToggleDoor();
			}
		}

		if (outsideInteractable)
		{
			if (Input.IsActionJustPressed("p_interact"))
			{
				ToggleDoor();
			}
		}
	}
	
	private void OnInsideAreaEntered(Area2D area)
	{
		if (area.Name == "PlayerHitbox")
		{
			insideInteractable = true;
			EmitSignal(SignalName.EnteredInside);
		}
	}
	private void OnInsideAreaExited(Area2D area)
	{
		if (area.Name == "PlayerHitbox")
		{
			insideInteractable = false;
			EmitSignal(SignalName.ExitedInside);
		}
	}	
	private void OnOutsideAreaEntered(Area2D area)
	{
		if (area.Name == "PlayerHitbox")
		{
			outsideInteractable = true;
			EmitSignal(SignalName.EnteredOutside);
		}
	}
	
	private void OnOutsideAreaExited(Area2D area)
	{
		if (area.Name == "PlayerHitbox")
		{
			outsideInteractable = false;
			EmitSignal(SignalName.ExitedOutside);
		}
	}

	private void ToggleDoor()
	{
		doorOpen = !doorOpen;
		if (doorOpen)
		{
			sprite.Play("open");
		}
		else
		{
			sprite.PlayBackwards("open");
		}
	}
	
	private void onAnimationFinished()
	{
		collision.SetDeferred("disabled", doorOpen);
	}
	
	
}
