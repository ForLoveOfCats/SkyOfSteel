using Godot;
using System.Collections.Generic;



public class PipeSystem
{
	public static int Number = 0;

	public List<IPipe> Pipes = null;

	public PipeSystem(IPipe First)
	{
		Number++;
		GD.Print($"Constructed, now there are {Number} systems");

		Pipes = new List<IPipe>{ First };

		GD.Print("Created system");
	}

	~PipeSystem()
	{
		Number--;
		GD.Print($"Destructed, now there are {Number} systems");
	}


	public void Consume(PipeSystem Other)
	{
		if(this == Other)
			return;

		List<IPipe> NewPipes = new List<IPipe>(Pipes.Count + Other.Pipes.Count);
		NewPipes.AddRange(Pipes);
		NewPipes.AddRange(Other.Pipes);
		Pipes = NewPipes;

		foreach(IPipe CurrentPipe in Other.Pipes)
		{
			CurrentPipe.System = this;
		}

		GD.Print("Consumed");
	}
}
