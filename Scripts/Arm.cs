using Godot;
using System;

public class Arm : Position2D{
    [Export]int minDist = 5;
    float currentX;

    public static Position2D joint1, joint2, hand;
    public static float lenUpper = 0, lenMiddle = 0, lenLower = 0;

    public enum States{Move, Grab};
    public static States state = States.Move;
    public static bool holding;

    bool fliped = true;
    AnimatedSprite handSprite;
    CollisionShape2D handCollision;
    public static Vector2 targetPos;

    public override void _Ready(){
        joint1 = GetNode<Position2D>("Joint1");
        joint2 = joint1.GetNode<Position2D>("Joint2");
        hand = joint2.GetNode<Position2D>("Hand");
        
        handSprite = hand.GetNode<AnimatedSprite>("Sprite");
        handCollision = hand.GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D");
    }

    public override void _Process(float delta){
        lenUpper = joint1.Position.x;
        lenMiddle = joint2.Position.x;
        lenLower = hand.Position.x;

        targetPos = targetPos.LinearInterpolate(GetGlobalMousePosition(), 0.2f);
        targetPos.y = Mathf.Max(59, targetPos.y);

        SetStates();
        Visuals();
        UpdateIk();
        Update();
    }

    void SetStates(){
        switch(state){
            case States.Move:
                handSprite.Play("Idle");
                handCollision.Disabled = true;
                currentX = targetPos.x;
            break;
            case States.Grab:
                handSprite.Play("Grab");
                handCollision.Disabled = false;
                if(holding){
                    targetPos.x = currentX;
                    Leg.targetPos.y = Mathf.Clamp(Leg.targetPos.y, 90, 105);
                }
            break;
        }
    }

    void Visuals(){
        if(targetPos.x >= Player.publicPostion.x){
            handSprite.FlipV = false;
            fliped = true;
        }else{
            handSprite.FlipV = true;
            fliped = false;
        }

        if(Input.IsMouseButtonPressed(1)) state = States.Grab;
        else state = States.Move;
        handSprite.Visible = lenLower <= 0.2 ? false : true;
    }

    void UpdateIk(){
        Vector2 offset = targetPos - GlobalPosition;
        float distToTarget = offset.Length();

        if(distToTarget < minDist){
            offset = (offset / distToTarget) * minDist;
            distToTarget = minDist;
        }

        float baseRotation = offset.Angle();
        float lenTotal = lenUpper + lenMiddle + lenLower;
        float lenDummySide = (lenUpper + lenMiddle) * Mathf.Clamp(distToTarget / lenTotal, 0, 1);

        float[] baseAngles = SideCalculation(lenDummySide, lenLower, distToTarget);
        float[] nextAngles = SideCalculation(lenUpper, lenMiddle, lenDummySide);

        GlobalRotation = baseAngles[1] + nextAngles[1] + baseRotation;
        joint1.Rotation = nextAngles[2];
        joint2.Rotation = baseAngles[2] + nextAngles[0];
    }

    float[] SideCalculation(float sideA, float sideB, float sideC){
        float[] newSides = new float[3];
        if(sideC >= sideA + sideB){
            for (int i = 0; i < newSides.Length; i++){
                newSides[i] = 0;
            }
            return newSides;
        }

        float angleA = LawOfCos(sideB, sideC, sideA);
        float angleB = LawOfCos(sideC, sideA, sideB) + Mathf.Pi;
        float angleC = Mathf.Pi - angleA - angleB;

        if(fliped){
            angleA = -angleA;
            angleB = -angleB;
            angleC = -angleC;
        }

        newSides[0] = angleA;
        newSides[1] = angleB;
        newSides[2] = angleC;
        
        return newSides;
    }

    float LawOfCos(float a, float b, float c){
        if(2 * a * b == 0) return 0;
        else return Mathf.Acos( (a * a + b * b - c * c) / ( 2 * a * b) );
    }

    public override void _Draw(){
        Color col = new Color(0, 0, 0);
        DrawLine(ToLocal(GlobalPosition), ToLocal(joint1.GlobalPosition), col, 2);
        DrawLine(ToLocal(joint1.GlobalPosition), ToLocal(joint2.GlobalPosition), col, 2);
        DrawLine(ToLocal(joint2.GlobalPosition), ToLocal(hand.GlobalPosition), col, 2);
    }
}