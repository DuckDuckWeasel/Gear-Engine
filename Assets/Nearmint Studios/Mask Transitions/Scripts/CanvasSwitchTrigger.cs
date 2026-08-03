using MaskTransitions;
using UnityEngine;

/// <summary>
/// Coloque este componente em qualquer GameObject (ex: o próprio botão)
/// e arraste os Canvas de origem/destino pelo Inspector.
/// Depois, no componente Button, em On Click (), arraste este GameObject
/// e escolha CanvasSwitchTrigger -> TriggerSwitch().
/// </summary>
public class CanvasSwitchTrigger : MonoBehaviour
{
    [Header("Canvas de origem (o que está visível agora)")]
    [SerializeField] private GameObject fromCanvas;

    [Header("Canvas de destino (o que vai aparecer)")]
    [SerializeField] private GameObject toCanvas;

    [Header("Atraso opcional antes de iniciar a transição (segundos)")]
    [SerializeField] private float delay = 0f;

    // Método sem parâmetros: aparece disponível no dropdown do On Click () do Button
    public void TriggerSwitch()
    {
        if (TransitionManager.Instance == null)
        {
            Debug.LogWarning("TransitionManager.Instance não encontrado na cena.");
            return;
        }

        TransitionManager.Instance.SwitchCanvas(fromCanvas, toCanvas, delay);
    }
}
