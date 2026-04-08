public class WorldObject
{
	public WorldObject() {
		X = 0;
		Y = 0;
		Width = 1; 
		Height = 1;
		Rotation = 0;
		Layer = 0;
	}

	public WorldObject(string id, string prefabId, int x, int y, int width, int height, int rotation, int layer)
	{
		ID = id;
		PrefabID = prefabId;
		X = x;
		Y = y;
		Width = width;
		Height = height;
		Rotation = rotation;
		Layer = layer;
	}

	public WorldObject(string prefabId, int x, int y, int width, int height, int rotation, int layer) {
		PrefabID = prefabId;
		X = x;
		Y = y;
		Width = width;
		Height = height;
		Rotation = rotation;
		Layer = layer;
	}

	public string ID;
	public string PrefabID;
	public int X, Y;
	public int Width, Height;
	public int Rotation;
	public int Layer;
}