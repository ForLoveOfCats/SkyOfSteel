using Godot;
using System.Collections.Generic;



public class PipeSystem {
	public List<PipeCoreLogic> Pipes = null;

	public PipeSystem(PipeCoreLogic First) {
		Pipes = new List<PipeCoreLogic> { First };
	}


	public void Consume(PipeSystem Other) {
		if(this == Other)
			return;

		List<PipeCoreLogic> NewPipes = new List<PipeCoreLogic>(Pipes.Count + Other.Pipes.Count);
		NewPipes.AddRange(Pipes);
		NewPipes.AddRange(Other.Pipes);
		Pipes = NewPipes;

		foreach(PipeCoreLogic CurrentPipe in Other.Pipes) {
			CurrentPipe.System = this;
		}
	}
}
