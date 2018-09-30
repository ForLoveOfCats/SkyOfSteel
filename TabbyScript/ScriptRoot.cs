using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ScriptRoot : Node
{
	bool ParseError = false;
	List<string> VariableNames = new List<string>();

	List<List<object>> Bytecode = new List<List<object>>();
	List<Tabby.DataClass> VmData = new List<Tabby.DataClass>();
	List<Tabby.DataClass> VmStack = new List<Tabby.DataClass>();


	void SetData(object Index, object Value)
	{
		this.VmData[(int)Index] = (Tabby.DataClass)Value;
	}


	void BufferData()
	{
		VmData.Add(new Tabby.DataClass());
	}


	void PushStack(object Value)
	{
		this.VmStack.Add((Tabby.DataClass)Value);
	}


	Tabby.DataClass PopStack()
	{
		Tabby.DataClass Output = this.VmStack.Last();
		this.VmStack.RemoveAt(this.VmStack.Count-1);
		return Output;
	}


	void ExecuteBytecode()
	{
		foreach(List<object> Instruction in this.Bytecode)
		{
			switch(Instruction[0])
			{
				case Tabby.OP.PUSH:
					PushStack(Instruction[1]);
					break;
				case Tabby.OP.STORE:
					SetData(Instruction[1], PopStack());
					break;
				case Tabby.OP.ADD:
					PushStack(PopStack() + PopStack());
					break;
			}
		}
	}


	string StripWhite(string Input)
	{
		return Input.Replace(" ", String.Empty).Replace("	", String.Empty);
	}


	void ThrowCompileError(string Message, int LineNumber)
	{
		GD.Print(Message + " @ line " + LineNumber.ToString());
		this.ParseError = true;
	}


	List<List<object>> ParseExpression(string Expression)
	{
		List<List<object>> Instructions = new List<List<object>>();
		Instructions.Add(new List<object> {Tabby.OP.STORE, new Tabby.DataClass(Tabby.TYPE.NUM, Int32.Parse(Expression))});
		return Instructions;
	}


	List<List<object>> CompileLine(string Line, int LineNumber)
	{
		Line = this.StripWhite(Line);
		List<List<object>> Output = new List<List<object>>();

		if(Line.Substring(0,3) == "var")
		{
			int EqualIndex = Line.IndexOf("=");
			if(EqualIndex == -1)
			{
				ThrowCompileError("Missing equal sign when declaring a variable", LineNumber);
			}

			string Name = Line.Substring(3, EqualIndex-3);
			int NameIndex = VariableNames.IndexOf(Name);
			if(NameIndex == -1)
			{
				NameIndex = VariableNames.Count;
				VariableNames.Add(Name);
				BufferData();
			}

			foreach(List<object> Instruction in ParseExpression(Line.Substring(EqualIndex+1 ,Line.Length-EqualIndex-1)))
			{
				Output.Add(Instruction);
			}
			Output.Add(new List<object> {Tabby.OP.STORE, NameIndex}); //Store contents of scratch at stack location NameIndex
		}

		return Output;
	}


	public void ExecScript(string Script){
		int LineNumber = 0;
		foreach(string Line in Script.Split("\n").ToList())
		{
			LineNumber += 1;

			List<List<object>> BundledInstructions = this.CompileLine(Line, LineNumber);
			if(this.ParseError)
			{
				break;
			}

			foreach(List<object> Instruction in BundledInstructions)
			{
				this.Bytecode.Add(Instruction);
			}
		}

		this.Bytecode.Add(new List<object> {Tabby.OP.PUSH, new Tabby.DataClass(Tabby.TYPE.NUM, 10)});
		this.Bytecode.Add(new List<object> {Tabby.OP.PUSH, new Tabby.DataClass(Tabby.TYPE.NUM, 5)});
		this.Bytecode.Add(new List<object> {Tabby.OP.ADD});
		this.Bytecode.Add(new List<object> {Tabby.OP.STORE, 0});

		BufferData();

		foreach(List<object> Instruction in this.Bytecode)
		{
			string OutPrint = "";
			foreach(object Entry in Instruction)
			{
				OutPrint += Entry.ToString() + " ";
			}
			GD.Print(OutPrint);
		}

		ExecuteBytecode();

		foreach(Tabby.DataClass StackItem in VmData)
		{
			GD.Print(StackItem.ToString() + " " + StackItem.Type);
		}
	}
}
