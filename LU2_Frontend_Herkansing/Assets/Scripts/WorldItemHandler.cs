using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class WorldItemHandler : MonoBehaviour
{

	[SerializeField]
	private GameObject WorldNameText;

	[SerializeField]
	private GameObject WorldSizeText;

	[SerializeField]
	private string WorldSceneName;

	private World _world { get; set; }

	public void UpdateWorldItem()
	{
		WorldNameText.GetComponent<TMP_Text>().text = _world.Name;
		WorldSizeText.GetComponent<TMP_Text>().text = $"{_world.Width}x{_world.Height}";
	}

	public void SetWorldInfo(string name, string id, int width, int height)
	{
		_world = new World(name, id, width, height);
		UpdateWorldItem();
	}

	public void OnOpenButton()
	{
		WorldMaker.Instance.SetWorld(_world);
		SceneManager.LoadScene(WorldSceneName);
	}

	public async void OnDeleteButton() {
		string bearerToken = AccountManager.Instance.GetBearerToken();
		Debug.Log($"ID: {_world.ID}");
		UnityWebRequest request = await AccountManager.Instance.PerformApiCall(
			AccountManager.ApiUrl + $"/environments?environmentId={_world.ID}",
			"DELETE",
			"",
			bearerToken);

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(_world.ID);
			Debug.LogError(request.downloadHandler.text);
			return;
		}
	}

}
