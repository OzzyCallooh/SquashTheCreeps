using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public int Speed { get; set; } = 14;

	[Export]
	public int FallAcceleration { get; set; } = 75;

	[Export]
	public int JumpImpulse {get; set; } = 20;

	[Export]
	public int BounceImpulse { get; set; } = 16;

	[Signal]
	public delegate void HitEventHandler();

	private Vector3 _targetVelocity = Vector3.Zero;


    public override void _PhysicsProcess(double delta)
    {
		var direction = Vector3.Zero;
		if (Input.IsActionPressed("move_forward")) direction.Z -= 1.0f;
		if (Input.IsActionPressed("move_back")) direction.Z += 1.0f;
		if (Input.IsActionPressed("move_left")) direction.X -= 1.0f;
		if (Input.IsActionPressed("move_right")) direction.X += 1.0f;

		if (direction != Vector3.Zero) {
			direction = direction.Normalized();
			// Setting the basis property will affect the rotation of the node.
			GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);

			GetNode<AnimationPlayer>("AnimationPlayer").SpeedScale = 4;
		} else {
			GetNode<AnimationPlayer>("AnimationPlayer").SpeedScale = 1;
		}

		// Ground velocity
		_targetVelocity.X = direction.X * Speed;
		_targetVelocity.Z = direction.Z * Speed;

		if (IsOnFloor() && Input.IsActionJustPressed("jump")) {
			_targetVelocity.Y = JumpImpulse;
		}

		// Vertical velocity
		if (!IsOnFloor()) {
			_targetVelocity.Y -= FallAcceleration * (float)delta;
		}

		Velocity = _targetVelocity;
		MoveAndSlide();

		for (int index = 0; index < GetSlideCollisionCount(); index++) {
			KinematicCollision3D collision = GetSlideCollision(index);
			if (collision.GetCollider() is Mob mob) {
				if (Vector3.Up.Dot(collision.GetNormal()) > 0.1f) {
					mob.Squash();
					_targetVelocity.Y = BounceImpulse;
					break;
				}
			}
		}

		var pivot = GetNode<Node3D>("Pivot");
		pivot.Rotation = new Vector3(Mathf.Pi / 6.0f * Velocity.Y / JumpImpulse, pivot.Rotation.Y, pivot.Rotation.Z);
    }

	public void Die() {
		EmitSignal(SignalName.Hit);
		QueueFree();
	}

	private void OnMobDetectorBodyEntered(Node3D body) {
		Die();
	}
}
