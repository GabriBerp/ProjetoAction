using UnityEngine;

// Dados que mudam de acordo com o RatSize. Ajuste os numeros aqui, em um unico lugar.
public readonly struct RatSizeProfile
{
    public readonly float speedMultiplier;   // multiplica a velocidade base
    public readonly float healthMultiplier;  // multiplica a vida base
    public readonly Vector3 scale;           // localScale final (X, Y, Z separados)
    public readonly bool getsWinded;         // para pra "recuperar o folego"?

    public RatSizeProfile(float speed, float health, Vector3 scale, bool winded)
    {
        speedMultiplier = speed;
        healthMultiplier = health;
        this.scale = scale;
        getsWinded = winded;
    }
}

public static class RatSizeExtensions
{
    public static RatSizeProfile GetProfile(this RatSize size)
    {
        switch (size)
        {
            //                                   velocidade  vida   escala (X, Y, Z)                  folego
            case RatSize.Mini:   return new RatSizeProfile(1.60f, 0.40f, new Vector3(0.52f, 1.00f, 0.31f), false);
            case RatSize.Agile:  return new RatSizeProfile(1.30f, 0.70f, new Vector3(0.22f, 0.90f, 1.28f), false);
            case RatSize.Normal: return new RatSizeProfile(1.00f, 1.00f, new Vector3(1.00f, 1.00f, 1.00f), false);
            case RatSize.Strong: return new RatSizeProfile(0.80f, 1.60f, new Vector3(1.30f, 1.30f, 1.00f), false);
            case RatSize.Big:    return new RatSizeProfile(0.55f, 2.50f, new Vector3(2.50f, 2.00f, 1.40f), true);
            default:             return new RatSizeProfile(1f, 1f, Vector3.one, false);
        }
    }
}