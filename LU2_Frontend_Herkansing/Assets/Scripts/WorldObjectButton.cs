using UnityEngine;
using UnityEngine.UI;

public class WorldObjectButton : MonoBehaviour
{
    public GameObject DragDropScript;
    public int PrefabIndex;

    public void OnButtonPress() {
        GameObject prefab = WorldMaker.Instance.GetPrefabs()[PrefabIndex];
        GameObject newObject = Instantiate(prefab);
        newObject.GetComponent<SpriteRenderer>().sprite = GetComponent<Image>().sprite;
        Debug.Log($"Clicked on: {PrefabIndex}");
    }
}
