using UnityEngine;

public class GameObjectActiveStep : SequenceStep
{
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private bool setActive = true;

    protected override void OnExecute()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(setActive);
        }
        Complete();
    }
}
