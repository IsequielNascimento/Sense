using UnityEngine;

public class SingleObject : MonoBehaviour
{
    public GameObject placedObject;

    // Esta função agora recebe o objeto diretamente
    public void OnObjectPlaced(GameObject newObject)
    {
        // Destroi o objeto anteriormente colocado, se existir
        if (placedObject != null && placedObject != newObject)
        {
            Destroy(placedObject);
        }

        // Atualiza a referência para o novo objeto colocado
        placedObject = newObject;

        Debug.Log("Objeto único atualizado na cena.");
    }
}
