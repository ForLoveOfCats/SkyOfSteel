using Godot;
using System.Collections.Generic;



public class PipeSystem
{
	public List<IPipe> Pipes = null;

	public PipeSystem(IPipe First)
	{
		Pipes = new List<IPipe>{ First };
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
	}
}
