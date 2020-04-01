using Godot;
using System;

public class Player : KinematicBody2D{
    public static Vector2 publicPostion;
    public bool up, down, left, right; 
    AnimatedSprite sprite;
    Position2D arm;

    int maxSpeed = 100;
    int accleration = 1000;

    Vector2 motion = Vector2.Zero;

    public override void _Ready(){
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        arm = GetNode<Position2D>("Arm");
    }

    public override void _PhysicsProcess(float delta){
        publicPostion = GlobalPosition;
        GetInput();
        SetSprite();
        
        Vector2 axis = GetAxis();
        if(axis == Vector2.Zero) ApplyFriction(accleration * delta);
        else ApplyMovement(axis.x * accleration * delta, axis.y * accleration * delta);
        MoveAndSlide(motion);
    }

    Vector2 GetAxis(){
        Vector2 axis = new Vector2();
        axis.x = Convert.ToInt32(left) - Convert.ToInt32(right);
        axis.y = Convert.ToInt32(down) - Convert.ToInt32(up);
        return axis.Normalized();
    }

    void ApplyFriction(float amount){
        if(motion.Length() > amount) motion -= motion.Normalized() * amount;
        else motion = Vector2.Zero;
    }

    void ApplyMovement(float amountX, float amountY){
        Vector2 newMotion = new Vector2(amountX, amountY);
        motion += newMotion;
        motion = motion.Clamped(maxSpeed);
    }

    void SetSprite(){
        if(up) sprite.Play("Backward");
        else if(down) sprite.Play("Forward");
        else if(right) sprite.Play("Left");
        else if(left) sprite.Play("Right");

        if(sprite.Animation == "Backward") arm.ZIndex = 10;
        else arm.ZIndex = -10;
    }

    void GetInput(){
        up = Input.IsActionPressed("ui_up") ? true : false;
        down = Input.IsActionPressed("ui_down") ? true : false;
        right = Input.IsActionPressed("ui_right") ? true : false;
        left = Input.IsActionPressed("ui_left") ? true : false;
    }
}