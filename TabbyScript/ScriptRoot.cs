using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


class ExpressionNode
{
	public enum TYPE {DATA, OPERATION};

	public ExpressionNode(TYPE TypeArg, string ElementArg)
	{
		this.Type = TypeArg;
		this.Element = ElementArg;
	}

	public ExpressionNode.TYPE Type;
	public string Element;
	public List<ExpressionNode> Children = new List<ExpressionNode>();
}


class FlatTokenList
{
	List<ExpressionNode> InternalList = new List<ExpressionNode>();

	public List<ExpressionNode> List()
	{
		return this.InternalList;
	}

	public void AddToken(ExpressionNode NewToken)
	{
		if(NewToken.Element.Length > 0)
		{
			this.InternalList.Add(NewToken);
		}
	}
}


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


	bool IsExpressionOperation(string In)
	{
		char Op = Convert.ToChar(In);
		switch(Op)
		{
			case '+':
				return true;
			case '-':
				return true;
			case '*':
				return true;
			case '/':
				return true;
			default:
				return false;
		}
	}


	int OperationPrecedence(string In)
	{
		char Op = Convert.ToChar(In);
		switch(Op)
		{
			case '+':
				return 1;
			case '-':
				return 1;
			case '*':
				return 2;
			case '/':
				return 2;
			default:
				return 0;
		}
	}


	ExpressionNode ParseExpression(string Expression)
	{
		int OpenParenthesesCount = 0;
		string CurrentTokenString = "";
		string SubExpression = "";
		FlatTokenList FlatTokenList = new FlatTokenList();

		foreach(char Car in Expression)
		{
			if(OpenParenthesesCount > 0) //This weirdness detects SubExpressions and builds a string which can be sent to another call to ParseExpression
			{
				switch(Car)
				{
					case '(':
						OpenParenthesesCount++;
						break;
					case ')':
						OpenParenthesesCount--;
						break;
				}
				if(OpenParenthesesCount > 0)
				{
					SubExpression += Car.ToString();
				}
				continue;
			}

			if(OpenParenthesesCount == 0 && SubExpression.Length > 0)
			{
				FlatTokenList.AddToken(ParseExpression(SubExpression));
				SubExpression = "";
			}

			switch(Car)
			{
				case '(':
					OpenParenthesesCount++;
					break;
				case ')':
					break;
				case '+':
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.DATA, CurrentTokenString));
					CurrentTokenString = "";
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.OPERATION, "+"));
					break;
				case '-':
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.DATA, CurrentTokenString));
					CurrentTokenString = "";
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.OPERATION, "-"));
					break;
				case '*':
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.DATA, CurrentTokenString));
					CurrentTokenString = "";
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.OPERATION, "*"));
					break;
				case '/':
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.DATA, CurrentTokenString));
					CurrentTokenString = "";
					FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.OPERATION, "/"));
					break;
				default:
					CurrentTokenString += Car;
					break;
			}
		}

		if(CurrentTokenString.Length > 0)
		{
			FlatTokenList.AddToken(new ExpressionNode(ExpressionNode.TYPE.DATA, CurrentTokenString));
			CurrentTokenString = "";
		}

		//Prints FlatTokenList
		//GD.Print("Start FlatTokenList: " + Expression);
		foreach(ExpressionNode Entry in FlatTokenList.List())
		{
			//GD.Print(Entry.Type.ToString() + " " + Entry.Element);
		}
		//GD.Print("End FlatTokenList: " + Expression + "\n");


		return new ExpressionNode(ExpressionNode.TYPE.DATA, ""); //THIS IS WRONG, should return topmost item in the ExpressionNode tree
	}


	void ShuntingYard(FlatTokenList Expression)
	{
		foreach(ExpressionNode Node in Expression.List())
		{
			if(Node.Type == ExpressionNode.TYPE.DATA)
			{

			}
		}
	}


	List<List<object>> CompileExpression(string Expression)
	{
		ParseExpression(Expression);
		List<List<object>> Instructions = new List<List<object>>();
		//Instructions.Add(new List<object> {Tabby.OP.PUSH, new Tabby.DataClass(Tabby.TYPE.NUM, Int32.Parse(Expression))});
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

			foreach(List<object> Instruction in CompileExpression(Line.Substring(EqualIndex+1 ,Line.Length-EqualIndex-1)))
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

		/*this.Bytecode.Add(new List<object> {Tabby.OP.PUSH, new Tabby.DataClass(Tabby.TYPE.NUM, 10)});
		this.Bytecode.Add(new List<object> {Tabby.OP.PUSH, new Tabby.DataClass(Tabby.TYPE.NUM, 5)});
		this.Bytecode.Add(new List<object> {Tabby.OP.ADD});
		this.Bytecode.Add(new List<object> {Tabby.OP.STORE, 0});
		BufferData();*/

		/*foreach(List<object> Instruction in this.Bytecode)
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
		}*/
	}
}
