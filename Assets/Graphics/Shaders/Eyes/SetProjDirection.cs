using UnityEngine;

[ExecuteInEditMode]

public class SetProjDirection : MonoBehaviour
{
    public Material mat;
    public Transform directionObj;

    void Update()
    {
        mat.SetVector("_Look_Position",directionObj.position);
    }
}
