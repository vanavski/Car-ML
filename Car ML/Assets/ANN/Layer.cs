using System.Collections.Generic;

/// <summary>
/// Layer of neurons
/// </summary>
public class Layer {

    public int numNeurons; // how many neurons is creates on this layer
    public List<Neuron> neurons = new List<Neuron>();

    public Layer(int nNeurons,
        int numNeuronInputs) //count of neurons in the previous layer
    {
        numNeurons = nNeurons;
        for (int i = 0; i < nNeurons; i++)
        {
            neurons.Add(new Neuron(numNeuronInputs));
        }
    }
}
