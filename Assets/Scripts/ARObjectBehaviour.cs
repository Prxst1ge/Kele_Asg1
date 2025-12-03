using UnityEngine;

public class ARObjectBehaviour : MonoBehaviour

{

[SerializeField] private MeshRenderer meshRendererToToggle;

public void ToggleMeshRenderer()
{
        meshRendererToToggle.enabled = !meshRendererToToggle.enabled;

}
}