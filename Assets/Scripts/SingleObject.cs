using UnityEngine;

public class SingleObject : MonoBehaviour
{
    public GameObject placedObject;

    public void OnObjectPlaced(GameObject newObject)
    {
        if (placedObject != null && placedObject != newObject)
        {
            Destroy(placedObject);
        }

        placedObject = newObject;

        Debug.Log("Objeto único atualizado na cena.");
    }
}
