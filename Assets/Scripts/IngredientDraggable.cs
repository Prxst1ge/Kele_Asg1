/*
 * Author: Javier
 * Date: 11 December2025
 * Description:
 * Enables drag-and-drop interaction for 3D ingredient objects in AR.
 * Allows users to move ingredients by touching or clicking, using
 * Unity's EventSystem and pointer interfaces.
 */

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Enables dragging of a 3D ingredient object using touch or mouse input.
/// Works with Unity's EventSystem and New Input System.
/// </summary>
[RequireComponent(typeof(Collider))]
public class IngredientDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Drag Settings")]

    /// <summary>
    /// Distance in front of the camera where the object will be positioned
    /// while being dragged.
    /// </summary>
    [Tooltip("How far in front of the camera the object should sit while dragging.")]
    [SerializeField] private float dragDistance = 0.7f;

    /// <summary>
    /// Cached reference to the main camera.
    /// </summary>
    private Camera mainCam;

    /// <summary>
    /// Tracks whether the object is currently being dragged.
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// Offset between the object's position and the pointer's world position.
    /// Prevents snapping during drag.
    /// </summary>
    private Vector3 dragOffset;

    /// <summary>
    /// Initializes the camera reference on startup.
    /// </summary>
    void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[IngredientDraggable] No Main Camera found in scene.");
        }
    }

    /// <summary>
    /// Called when the user first presses on the ingredient object.
    /// Calculates initial drag offset.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (mainCam == null) return;

        isDragging = true;

        // Calculate world position under the pointer at the drag distance
        Vector3 worldPos = ScreenToWorldAtDistance(eventData.position, dragDistance);
        dragOffset = transform.position - worldPos;
    }

    /// <summary>
    /// Called continuously while the user drags their finger or mouse.
    /// Updates the ingredient's position.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || mainCam == null) return;

        Vector3 worldPos = ScreenToWorldAtDistance(eventData.position, dragDistance);
        transform.position = worldPos + dragOffset;
    }

    /// <summary>
    /// Called when the user releases the finger or mouse button.
    /// Stops dragging.
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    /// <summary>
    /// Converts a screen-space position into a world-space position
    /// at a fixed distance in front of the camera.
    /// </summary>
    /// <param name="screenPos">Screen position of the pointer.</param>
    /// <param name="distanceFromCamera">Distance from the camera.</param>
    /// <returns>World-space position.</returns>
    private Vector3 ScreenToWorldAtDistance(Vector2 screenPos, float distanceFromCamera)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        return ray.GetPoint(distanceFromCamera);
    }
}
