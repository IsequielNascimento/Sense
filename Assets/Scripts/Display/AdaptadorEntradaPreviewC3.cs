using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AdaptadorMenuC3))]
public class AdaptadorEntradaPreviewC3 : MonoBehaviour
{
    [SerializeField] private AdaptadorMenuC3 adaptador;

    private void Awake()
    {
        if (adaptador == null) adaptador = GetComponent<AdaptadorMenuC3>();
    }

    private void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null || adaptador == null) return;

        if (teclado.mKey.wasPressedThisFrame) adaptador.EntrarMenu();
        if (teclado.rightArrowKey.wasPressedThisFrame) adaptador.ProximoB3();
        if (teclado.leftArrowKey.wasPressedThisFrame) adaptador.AnteriorB1();
        if (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame) adaptador.ConfirmarB2();
        if (teclado.escapeKey.wasPressedThisFrame) adaptador.CancelarSair();
    }
}
