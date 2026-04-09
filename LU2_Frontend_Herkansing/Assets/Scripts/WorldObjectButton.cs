using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldObjectButton : MonoBehaviour
{
    public int PrefabIndex;

	public void SetRandomTransformInWorld(Transform transform, World world)
	{
		int halfWidth = (int)Mathf.Round(world.Width / 2);
		int halfHeight = (int)Mathf.Round(world.Height / 2);

		transform.position = new(
			Random.Range(-halfWidth, halfWidth),
			Random.Range(-halfHeight, halfHeight),
			-1
		);
	}

    public void OnButtonPress() {
        GameObject prefab = WorldMaker.Instance.GetPrefabs()[PrefabIndex];
        GameObject newObject = Instantiate(prefab);
		SetRandomTransformInWorld(newObject.transform, WorldMaker.Instance.GetWorld());

		WorldObject createdWorldObject = new(
			PrefabIndex.ToString(),
			(int)newObject.transform.position.x,
			(int)newObject.transform.position.y,
			1,
			1,
			0,
			(int)newObject.transform.position.z);

		newObject.GetComponent<DraggableItem>().WorldObject = createdWorldObject;

		WorldMaker.Instance.SaveObject(createdWorldObject);
		WorldMaker.Instance.AddPrefab(newObject);
	}
}
