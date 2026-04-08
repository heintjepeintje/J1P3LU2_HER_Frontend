public class World
{
	public World(string name, string id, int width, int height)
	{
		Name = name;
		ID = id;
		Width = width;
		Height = height;
	}

	public World(string name, int width, int height)
	{
		Name = name;
		Width = width;
		Height = height;
	}

	public string Name;
	public string ID;
	public int Width;
	public int Height;
}