using System.Collections.Generic;
/// <summary>
/// Store information about weights, inputs, bias, error
/// </summary>
public class Neuron {

    public int numInputs; //how many inputs come in neuron
    public double bias; //extra weight
    public double output;
    public double errorGradient;
    public List<double> weights = new List<double>();
    public List<double> inputs = new List<double>(); //all of inputs

    public Neuron(int nInputs)
	{
		float weightRange = (float) 2.4/(float) nInputs;
		bias = UnityEngine.Random.Range(-weightRange,weightRange);
		numInputs = nInputs;

		for(int i = 0; i < nInputs; i++)
			weights.Add(UnityEngine.Random.Range(-weightRange,weightRange));
	}
}
