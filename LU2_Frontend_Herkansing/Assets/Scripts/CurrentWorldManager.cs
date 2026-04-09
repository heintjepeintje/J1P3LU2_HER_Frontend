using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentWorldManager : MonoBehaviour
{
	[SerializeField]
	private string WorldSelectorScene;

    public void SaveWorld()
	{
		WorldMaker.Instance.Save();
	}

	public void ExitWorld()
	{
		WorldMaker.Instance.Save();
		WorldMaker.Instance.DestroyPrefabs();
		SceneManager.LoadScene(WorldSelectorScene);
	}
}
