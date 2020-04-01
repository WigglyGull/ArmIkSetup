using Godot;
using System;

public class Joint : Position2D{
    [Export] float MaxDistance;
    
    public override void _Ready(){
        MaxDistance = Position.x;
        Position = Vector2.Zero;
    }

    public override void _Process(float delta){
        Vector2 newPosition = Position;
        newPosition.x = Input.IsMouseButtonPressed(2) ? Mathf.Lerp(newPosition.x, MaxDistance, 0.1f) : Mathf.Lerp(newPosition.x, 0, 0.08f);
        Position = newPosition;
    }
}