using Godot;


public class PyVector3
{
	public Vector3 Vec = new Vector3();

	public float x
	{
		get { return Vec.x; }
		set { Vec.x = value; }
	}

	public float y
	{
		get { return Vec.y; }
		set { Vec.y = value; }
	}

	public float z
	{
		get { return Vec.z; }
		set { Vec.z = value; }
	}

	public PyVector3()
	{}

	public PyVector3(Vector3 VecArg)
	{
		Vec = VecArg;
	}

	public PyVector3(float X, float Y, float Z)
	{
		x = X;
		y = Y;
		z = Z;
	}


	public static PyVector3 operator+(PyVector3 First, PyVector3 Second)
	{
		return new PyVector3(First.x+Second.x, First.y+Second.y, First.z+Second.z);
	}


	public static PyVector3 operator-(PyVector3 First, PyVector3 Second)
	{
		return new PyVector3(First.x-Second.x, First.y-Second.y, First.z-Second.z);
	}


	public static PyVector3 operator*(PyVector3 First, PyVector3 Second)
	{
		return new PyVector3(First.x*Second.x, First.y*Second.y, First.z*Second.z);
	}


	public static PyVector3 operator/(PyVector3 First, PyVector3 Second)
	{
		return new PyVector3(First.x/Second.x, First.y/Second.y, First.z/Second.z);
	}


	public static implicit operator Vector3(PyVector3 PyVec)
	{
		return PyVec.Vec;
	}


	public static implicit operator PyVector3(Vector3 Vec)
	{
		return new PyVector3(Vec);
	}


	public override string ToString()
	{
		return $"({x},{y},{z})";
	}
}
