using System.Collections;
using UnityEngine;

public class EmissaoIntermitente : MonoBehaviour
{
    [SerializeField] private ParticleSystem particulas;
    [SerializeField] private float tempoEmitindo = 3f;  // X
    [SerializeField] private float tempoParado = 2f;    // Y

    private void Start()
    {
        if (particulas == null) particulas = GetComponent<ParticleSystem>();
        StartCoroutine(Ciclo());
    }

    private IEnumerator Ciclo()
    {
        while (true)
        {
            particulas.Play();
            yield return new WaitForSeconds(tempoEmitindo);

            // StopEmitting para de criar novas partículas,
            // mas deixa as existentes terminarem a vida naturalmente
            particulas.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return new WaitForSeconds(tempoParado);
        }
    }
}