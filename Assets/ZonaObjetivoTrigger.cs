using UnityEngine;

public class ZonaObjetivoTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pelota"))
        {
            Debug.Log("¡PELOTA LLEGÓ AL OBJETIVO!");

            EmpujarPelota scriptRespawn = FindObjectOfType<EmpujarPelota>();

            if (scriptRespawn != null)
            {
                scriptRespawn.RespawnearTodo();
            }
        }
    }
}