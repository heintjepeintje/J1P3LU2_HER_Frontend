using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class WorldMaker : MonoBehaviour
{
	private World _currentWorld;

	[SerializeField]
	private GameObject GrassTile;

	private List<WorldObject> _savedObjects;
	private List<WorldObject> _newObjects;

	[SerializeField]
	private List<GameObject> _prefabs;

	public static WorldMaker Instance;

	public void Awake() {
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	public void SetWorld(World world)
	{
		_currentWorld = world;
		Debug.Log($"SetWorld ID: {_currentWorld.ID}");
	}

	public async void Load()
	{
		if (_currentWorld == null) return;
		_newObjects = new List<WorldObject>();

		string bearerToken = AccountManager.Instance.GetBearerToken();
		Debug.Log($"ID: {_currentWorld.ID}");
		UnityWebRequest request = await AccountManager.Instance.PerformApiCall(
			AccountManager.ApiUrl + $"/objects?environmentId={_currentWorld.ID}",
			"GET",
			"",
			bearerToken);
	
		if (request.result != UnityWebRequest.Result.Success) {
			Debug.LogError(request.downloadHandler.text);
			return;
		}

		JArray objectsJson = JArray.Parse(request.downloadHandler.text);
		_savedObjects = new List<WorldObject>(objectsJson.Count);
		
		for (int i = 0; i < objectsJson.Count; i++) {
			JObject savedObject = objectsJson[i] as JObject;
			WorldObject newObject = new(
				savedObject["id"].Value<string>(),
				savedObject["prefabID"].Value<string>(),
				savedObject["x"].Value<int>(),
				savedObject["y"].Value<int>(),
				savedObject["width"].Value<int>(),
				savedObject["height"].Value<int>(),
				savedObject["rotation"].Value<int>(),
				savedObject["layer"].Value<int>()
			);

			int prefabIndex = int.Parse(newObject.PrefabID);
			GameObject newGameObject = Instantiate(_prefabs[prefabIndex]);
			newGameObject.transform.position = new Vector3(newObject.X, newObject.Y, -1);

			_savedObjects.Add(newObject);
		}
	}

	public void Generate()
	{
		if (_currentWorld == null) return;

		Debug.Log($"Generating World: {_currentWorld.Width}x{_currentWorld.Height}");

		int halfWidth = _currentWorld.Width / 2;
		int halfHeight = _currentWorld.Height / 2;
		
		for (int x = 0; x < _currentWorld.Width; x++)
		{
			for (int y = 0; y < _currentWorld.Height; y++)
			{
				GameObject newTile = Instantiate(GrassTile);
				newTile.transform.position = new Vector3(-halfWidth + x, -halfHeight + y, 0);
			}
		}
	}

	public void SaveObject(WorldObject worldObject)
	{
		_newObjects.Add(worldObject);
	}

	public List<GameObject> GetPrefabs() {
		return _prefabs;
	}

	public async void Save()
	{
		JArray newObjectsJson = new JArray();
		foreach (WorldObject obj in _newObjects)
		{
			JObject newObjectJson = new JObject();
			newObjectJson["environmentId"] = _currentWorld.ID;
			newObjectJson["prefabId"] = obj.PrefabID;
			newObjectJson["x"] = obj.X;
			newObjectJson["y"] = obj.Y;
			newObjectJson["width"] = obj.Width;
			newObjectJson["height"] = obj.Height;
			newObjectsJson["rotation"] = obj.Rotation;
			newObjectsJson["layer"] = obj.Layer;

			newObjectsJson.Add(newObjectJson);
		}

		string json = newObjectsJson.ToString();
		Debug.Log(json);

		string bearerToken = AccountManager.Instance.GetBearerToken();
		UnityWebRequest webRequest = await AccountManager.Instance.PerformApiCall(AccountManager.ApiUrl + $"/objects", "POST", json, bearerToken);
	
		if (webRequest.result != UnityWebRequest.Result.Success) {
			Debug.LogError(webRequest.downloadHandler.text);
			return;
		}

		Debug.Log("Success!");
	}

}
