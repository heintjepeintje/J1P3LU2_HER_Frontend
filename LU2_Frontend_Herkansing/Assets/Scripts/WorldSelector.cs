using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
	[SerializeField]
	private GameObject WorldSelectorPrefab;

	[SerializeField]
	private GameObject WorldSelectorPanel;

	[SerializeField]
	private GameObject WorldCreatorPanel;

	[SerializeField]
	private GameObject WorldNameInput;

	[SerializeField]
	private GameObject WorldWidthInput;

	[SerializeField]
	private GameObject WorldHeightInput;

	[SerializeField]
	private GameObject WorldCreateStatusText;

	[SerializeField]
	private string WorldSceneName;

	public class Environment2D
	{
		public Environment2D(string id, string userId, string name, int width, int height)
		{
			ID = id;
			UserID = userId;
			Name = name;
			Width = width;
			Height = height;
		}

		public string ID;
		public string UserID;
		public string Name;
		public int Width;
		public int Height;
	}

	private Environment2D[] _userEnvironments;
	private GameObject[] _worldPanels;

	public void Awake()
	{
		RefreshWorldList();
	}

	public async void RefreshWorldList()
	{
		if (_worldPanels != null)
		{
			foreach (GameObject panel in _worldPanels)
			{
				Destroy(panel);
			}
		}

		string bearerToken = AccountManager.Instance.GetBearerToken();
		UnityWebRequest request = await AccountManager.Instance.PerformApiCall(AccountManager.ApiUrl + $"/environments", "GET", "", bearerToken);

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError($"{request.result}: {request.downloadHandler.text}");
		}

		JArray array = JArray.Parse(request.downloadHandler.text);
		_userEnvironments = new Environment2D[array.Count];
		_worldPanels = new GameObject[array.Count];

		for (int i = 0; i < array.Count; i++)
		{
			JObject environmentJson = array[i] as JObject;

			string worldName = environmentJson["name"].Value<string>();
			string worldUserId = environmentJson["userID"].Value<string>();
			string worldId = environmentJson["id"].Value<string>();
			int worldWidth = environmentJson["width"].Value<int>();
			int worldHeight = environmentJson["height"].Value<int>();

			_userEnvironments[i] = new Environment2D(
				worldId,
				worldUserId,
				worldName,
				worldWidth,
				worldHeight
			);

			_worldPanels[i] = Instantiate(WorldSelectorPrefab, WorldSelectorPanel.transform);
			_worldPanels[i].GetComponent<WorldItemHandler>().SetWorldInfo(worldName, worldId, worldWidth, worldHeight);
		}
	}

	public void OpenCreateWorldPanel()
	{
		WorldCreatorPanel.SetActive(true);
	}

	public void CloseCreateWorldPanel()
	{
		WorldCreatorPanel.SetActive(false);
	}

	public async void OnCreateWorldButtonPress()
	{
		World? world = await CreateWorld(
			WorldNameInput.GetComponent<TMP_InputField>().text,
			int.Parse(WorldWidthInput.GetComponent<TMP_InputField>().text),
			int.Parse(WorldHeightInput.GetComponent<TMP_InputField>().text)
		);

		if (world != null) {
			CloseCreateWorldPanel();
			RefreshWorldList();

			WorldMaker.Instance.SetWorld(world);
			SceneManager.LoadScene(WorldSceneName);
			WorldMaker.Instance.Generate();
		}
	}

	public async Task<World?> CreateWorld(string name, int width, int height)
	{
		JObject worldJson = new()
		{
			["name"] = name,
			["width"] = width,
			["height"] = height
		};

		string bearerToken = AccountManager.Instance.GetBearerToken();
		UnityWebRequest request = await AccountManager.Instance.PerformApiCall(AccountManager.ApiUrl + $"/environments", "POST", worldJson.ToString(), bearerToken);

		if (request.result != UnityWebRequest.Result.Success)
		{
			WorldCreateStatusText.GetComponent<TMP_Text>().color = Color.red;
			WorldCreateStatusText.GetComponent<TMP_Text>().text = request.downloadHandler.text;
			Debug.LogError(request.downloadHandler.text);
			return null;
		}

		return new World(name, width, height);
	}
}
