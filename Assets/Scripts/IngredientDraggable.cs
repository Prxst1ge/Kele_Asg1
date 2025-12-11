using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Drag a 3D ingredient object by touching / clicking on it.
/// Works with the New Input System via the EventSystem.
/// </summary>
[RequireComponent(typeof(Collider))]
public class IngredientDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Drag Settings")]
    [Tooltip("How far in front of the camera the object should sit while dragging.")]
    [SerializeField] private float dragDistance = 0.7f;   // tune this for your AR scale

    private Camera mainCam;
    private bool isDragging = false;
    private Vector3 dragOffset;

    void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[IngredientDraggable] No Main Camera found in scene.");
        }
    }

    // Called when finger/mouse first presses this object
    public void OnPointerDown(PointerEventData eventData)
    {
        if (mainCam == null) return;

        isDragging = true;

        // world position under the pointer at our drag distance
        Vector3 worldPos = ScreenToWorldAtDistance(eventData.position, dragDistance);
        dragOffset = transform.position - worldPos;
    }

    // Called every frame while finger/mouse moves over the screen
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || mainCam == null) return;

        Vector3 worldPos = ScreenToWorldAtDistance(eventData.position, dragDistance);
        transform.position = worldPos + dragOffset;
    }

    // Called when finger/mouse is released
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    /// <summary>
    /// Converts a screen position to a world position at a fixed distance in front of the camera.
    /// </summary>
    private Vector3 ScreenToWorldAtDistance(Vector2 screenPos, float distanceFromCamera)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        return ray.GetPoint(distanceFromCamera);
    }
}
