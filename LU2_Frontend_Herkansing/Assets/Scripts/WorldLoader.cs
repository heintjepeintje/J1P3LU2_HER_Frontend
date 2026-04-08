using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class WorldLoader : MonoBehaviour
{
	[SerializeField]
	private GameObject ObjectPanel;

	[SerializeField]
	private GameObject ObjectUIItem;

	[SerializeField]
	private GameObject DragDropScript;

	public void Awake()
	{
		WorldMaker.Instance.Generate();
		WorldMaker.Instance.Load();

		List<GameObject> prefabs = WorldMaker.Instance.GetPrefabs();

		for (int i = 0; i < prefabs.Count; i++)
		{
			GameObject newItem = Instantiate(ObjectUIItem, ObjectPanel.transform);
			newItem.GetComponent<Image>().sprite = prefabs[i].GetComponent<SpriteRenderer>().sprite;
			newItem.GetComponent<WorldObjectButton>().PrefabIndex = i;
			newItem.GetComponent<WorldObjectButton>().DragDropScript = DragDropScript;
		}
	}
}
