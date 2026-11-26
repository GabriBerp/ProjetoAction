using UnityEngine;

// Como o rato se comporta, alem dos numeros. Cada tamanho tem um jeito proprio de brigar.
public enum RatArchetype
{
    Skittish,   // Mini   - assustadico: recua cedo, circula de longe e desiste rapido da busca
    Flanker,    // Agile  - flanqueador: orbita o jogador e ataca pelas laterais
    Standard,   // Normal - linha de base: persegue direto e so recua com vida baixa
    Brawler,    // Strong - brigao: nunca recua e investe quando o jogador tenta fugir
    Bruiser     // Big    - brutamontes: investe e para pra respirar, sem recuar nunca
}

// Dados que mudam de acordo com o RatSize. Ajuste os numeros aqui, em um unico lugar.
public readonly struct RatSizeProfile
{
    public readonly float speedMultiplier;   // multiplica a velocidade base
    public readonly float healthMultiplier;  // multiplica a vida base
    public readonly Vector3 scale;           // localScale final (X, Y, Z separados)
    public readonly bool getsWinded;         // para pra "recuperar o folego"?

    // --- comportamento (lido pelo EnemyNavmesh) ---
    public readonly RatArchetype archetype;
    public readonly float retreatScale;      // multiplica o limiar de vida pra recuar (0 = nunca recua)
    public readonly float circle;            // 0..1: quanto circula o alvo em vez de ir reto
    public readonly float circleRadius;      // raio inicial dessa orbita (m)
    public readonly float chargeFrom;        // distancia (m) em que comeca a investida (0 = nao investe)
    public readonly float chargeSpeed;       // multiplicador de velocidade durante a investida
    public readonly float searchScale;       // multiplica o tempo de busca depois de perder o alvo

    public RatSizeProfile(float speed, float health, Vector3 scale, bool winded,
                          RatArchetype archetype, float retreatScale, float circle, float circleRadius,
                          float chargeFrom, float chargeSpeed, float searchScale)
    {
        speedMultiplier = speed;
        healthMultiplier = health;
        this.scale = scale;
        getsWinded = winded;

        this.archetype = archetype;
        this.retreatScale = retreatScale;
        this.circle = circle;
        this.circleRadius = circleRadius;
        this.chargeFrom = chargeFrom;
        this.chargeSpeed = chargeSpeed;
        this.searchScale = searchScale;
    }
}

public static class RatSizeExtensions
{
    public static RatSizeProfile GetProfile(this RatSize size)
    {
        switch (size)
        {
            // Mini: assustadico. Recua com mais de metade da vida, circula de longe e procura pouco.
            case RatSize.Mini:
                return new RatSizeProfile(
                    speed: 1.60f, health: 0.40f, scale: new Vector3(0.52f, 1.00f, 0.31f), winded: false,
                    archetype: RatArchetype.Skittish, retreatScale: 1.60f,
                    circle: 0.55f, circleRadius: 7f, chargeFrom: 0f, chargeSpeed: 1f, searchScale: 0.45f);

            // Agile: flanqueador. Orbita rapido e ataca pelas laterais; procura o jogador por mais tempo.
            case RatSize.Agile:
                return new RatSizeProfile(
                    speed: 1.30f, health: 0.70f, scale: new Vector3(0.22f, 0.90f, 1.28f), winded: false,
                    archetype: RatArchetype.Flanker, retreatScale: 0.90f,
                    circle: 1f, circleRadius: 5.5f, chargeFrom: 0f, chargeSpeed: 1f, searchScale: 1.40f);

            // Normal: linha de base. Vai reto, recua so com vida baixa.
            case RatSize.Normal:
                return new RatSizeProfile(
                    speed: 1.00f, health: 1.00f, scale: new Vector3(1.00f, 1.00f, 1.00f), winded: false,
                    archetype: RatArchetype.Standard, retreatScale: 1f,
                    circle: 0.15f, circleRadius: 5f, chargeFrom: 0f, chargeSpeed: 1f, searchScale: 1f);

            // Strong: brigao. Nao recua nunca e investe quando o jogador se afasta.
            case RatSize.Strong:
                return new RatSizeProfile(
                    speed: 0.80f, health: 1.60f, scale: new Vector3(1.30f, 1.30f, 1.00f), winded: false,
                    archetype: RatArchetype.Brawler, retreatScale: 0f,
                    circle: 0f, circleRadius: 0f, chargeFrom: 9f, chargeSpeed: 1.50f, searchScale: 1.20f);

            // Big: brutamontes. Investe de longe, nao recua e para pra respirar (getsWinded).
            case RatSize.Big:
                return new RatSizeProfile(
                    speed: 0.55f, health: 2.50f, scale: new Vector3(2.50f, 2.00f, 1.40f), winded: true,
                    archetype: RatArchetype.Bruiser, retreatScale: 0f,
                    circle: 0f, circleRadius: 0f, chargeFrom: 7f, chargeSpeed: 1.40f, searchScale: 0.90f);

            default:
                return new RatSizeProfile(
                    speed: 1f, health: 1f, scale: Vector3.one, winded: false,
                    archetype: RatArchetype.Standard, retreatScale: 1f,
                    circle: 0f, circleRadius: 0f, chargeFrom: 0f, chargeSpeed: 1f, searchScale: 1f);
        }
    }
}