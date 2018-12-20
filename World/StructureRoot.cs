using System;
using System.Collections.Generic;


public class StructureRootClass : Godot.Node
{
	public Dictionary<Tuple<int,int>, List<Structure>> Chunks = new Dictionary<Tuple<int,int>, List<Structure>>();
	public List<Tuple<int,int>> LocalLoadedChunks = new List<Tuple<int,int>>();
}
