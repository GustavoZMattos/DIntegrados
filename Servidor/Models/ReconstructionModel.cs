public class ReconstructionModel
{
    // Propriedades da classe ReconstructionModel
    public string ModelName { get; set; }
    public double Gain { get; set; }

    // Construtor da classe ReconstructionModel
    public ReconstructionModel(string modelName, double gain)
    {
        ModelName = modelName;
        Gain = gain;
    }

    // Método da classe ReconstructionModel para execução do algoritmo de reconstrução
    public Image Reconstruct(Signal signal)
    {
        // Executa o algoritmo de reconstrução
        // ...
        // Retorna a imagem reconstruída
        return new Image();
    }
}