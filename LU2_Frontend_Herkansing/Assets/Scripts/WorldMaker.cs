using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class WorldMaker : MonoBehaviour
{
	private World _currentWorld;

	[SerializeField]
	private GameObject GrassTile;

	private List<WorldObject> _objects = new();
	private List<WorldObject> _newObjects = new();

	[SerializeField]
	private List<GameObject> _prefabs;

	public static WorldMaker Instance;

	private List<GameObject> _worldPrefabs = new();

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

	public World GetWorld()
	{
		return _currentWorld;
	}

	public async void Load()
	{
		if (_currentWorld == null) return;

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
		_objects = new List<WorldObject>(objectsJson.Count);
		
		for (int i = 0; i < objectsJson.Count; i++) {
			JObject savedObject = objectsJson[i] as JObject;
			WorldObject newObject = new(
				Guid.Parse(savedObject["id"].Value<string>()),
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
			newGameObject.GetComponent<DraggableItem>().WorldObject = newObject;
			newGameObject.transform.position = new Vector3(newObject.X, newObject.Y, -1);
			_worldPrefabs.Add(newGameObject);

			_objects.Add(newObject);
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

	public void AddPrefab(GameObject prefab)
	{
		_worldPrefabs.Add(prefab);
	}

	public void DestroyPrefabs()
	{
		foreach (GameObject prefab in _worldPrefabs)
		{
			Destroy(prefab);
		}

		_worldPrefabs.Clear();
	}

	public List<GameObject> GetPrefabs() {
		return _prefabs;
	}

	public async void Save()
	{
		if (_newObjects.Count > 0)
		{
			JArray newObjectsJson = new JArray();
			foreach (WorldObject obj in _newObjects)
			{
				JObject objectJson = new JObject();
				objectJson["environmentId"] = _currentWorld.ID;
				objectJson["prefabId"] = obj.PrefabID;
				objectJson["x"] = obj.X;
				objectJson["y"] = obj.Y;
				objectJson["width"] = obj.Width;
				objectJson["height"] = obj.Height;
				objectJson["rotation"] = obj.Rotation;
				objectJson["layer"] = obj.Layer;

				newObjectsJson.Add(objectJson);
			}

			string bearerToken = AccountManager.Instance.GetBearerToken();
			UnityWebRequest webRequest = await AccountManager.Instance.PerformApiCall(
				$"{AccountManager.ApiUrl}/objects", "POST", newObjectsJson.ToString(), bearerToken);

			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(webRequest.downloadHandler.text);
				return;
			}
		}

		foreach (WorldObject obj in _objects)
		{
			JObject updatedObjectJson = new JObject();
			updatedObjectJson["id"] = obj.ID.ToString();
			updatedObjectJson["environmentId"] = _currentWorld.ID;
			updatedObjectJson["prefabId"] = obj.PrefabID;
			updatedObjectJson["x"] = obj.X;
			updatedObjectJson["y"] = obj.Y;
			updatedObjectJson["width"] = obj.Width;
			updatedObjectJson["height"] = obj.Height;
			updatedObjectJson["rotation"] = obj.Rotation;
			updatedObjectJson["layer"] = obj.Layer;

			UnityWebRequest result = await AccountManager.Instance.PerformApiCall(
				$"{AccountManager.ApiUrl}/objects", "PUT", updatedObjectJson.ToString(), AccountManager.Instance.GetBearerToken());
		
			if (result.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(result.downloadHandler.text);
				break;
			}
		}

		foreach (GameObject instancedObject in _worldPrefabs)
		{
			Destroy(instancedObject);
		}
		_objects.Clear();
		_newObjects.Clear();

		Load();
	}

}
