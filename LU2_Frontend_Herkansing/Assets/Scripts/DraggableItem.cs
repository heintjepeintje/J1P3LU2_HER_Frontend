using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public WorldObject WorldObject;

	public void OnBeginDrag(PointerEventData eventData) { }

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint =
			Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, -Camera.main.transform.position.z - 1));
		transform.position = new Vector3(
			Mathf.Round(worldPoint.x),
			Mathf.Round(worldPoint.y),
			Mathf.Round(worldPoint.z)
		);

		WorldObject.X = (int)worldPoint.x;
		WorldObject.Y = (int)worldPoint.y;
	}

	public void OnEndDrag(PointerEventData eventData) { Debug.Log("End Drag"); }

	public void OnPointerDown(PointerEventData eventData) { Debug.Log("Pointer Down"); }
}
