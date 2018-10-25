using Godot;
using System;


public class Player : Spatial
{
	public bool Possessed = false;

	private int ForwardAxis = 0;
	private int RightAxis = 0;

	public override void _Ready()
	{
		Translation = new Vector3(0,1,0);
		if(Possessed)
		{
			((Camera)GetNode("SteelCamera")).MakeCurrent();
			((MeshInstance)GetNode("FPSMesh")).Hide();
			AddChild(((PackedScene)GD.Load("res://UI/SteelHUD.tscn")).Instance());
		}
	}

	public void ForwardMove(double Sens)
	{
		if(Sens > 0d)
		{
			ForwardAxis = 1;
		}
		else if(ForwardAxis > 0)
		{
			ForwardAxis = 0;
		}
	}


	public void BackwardMove(double Sens)
	{
		if(Sens > 0d)
		{
			ForwardAxis = -1;
		}
		else if(ForwardAxis < 0)
		{
			ForwardAxis = 0;
		}
	}


	public void RightMove(double Sens)
	{
		if(Sens > 0d)
		{
			RightAxis = 1;
		}
		else if(RightAxis > 0)
		{
			RightAxis = 0;
		}
	}


	public void LeftMove(double Sens)
	{
		if(Sens > 0d)
		{
			RightAxis = -1;
		}
		else if(RightAxis < 0)
		{
			RightAxis = 0;
		}
	}


	public override void _Process(float Delta)
	{
		GD.Print(ForwardAxis, RightAxis);
	}
}
