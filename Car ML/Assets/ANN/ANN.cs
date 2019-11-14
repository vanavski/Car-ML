using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Perceptron with back propogation and different activation functions
/// </summary>
public class ANN
{
    #region Fields
    public int numInputs; // count of neurons which come in network right the start
    public int numOutputs; // count of outputs
    public int numHidden; // count of layers between inputs and outputs
    public int numNPerHidden; //how many neurons do u want in ur layer
    public double alpha; //learning rates
    List<Layer> layers = new List<Layer>();
    #endregion

    public ANN(int inputsCount, int outputsCount, int hiddenLayersCount, int neuronsInLayerCount, double alpha)
    {
        numInputs = inputsCount;
        numOutputs = outputsCount;
        numHidden = hiddenLayersCount;
        numNPerHidden = neuronsInLayerCount;
        this.alpha = alpha;

        if (numHidden > 0)
        {
            layers.Add(new Layer(numNPerHidden, numInputs)); //numInputs: how many inputs come in for each neuron in layer

            for (int i = 0; i < numHidden - 1; i++)
            {
                layers.Add(new Layer(numNPerHidden, numNPerHidden)); //create all of hidden layers
            }

            layers.Add(new Layer(numOutputs, numNPerHidden));
        }
        else //if no have hidden layers, then create output layer
        {
            layers.Add(new Layer(numOutputs, numInputs));
        }
    }

    /// <summary>
    /// Train output values
    /// </summary>
    /// <param name="inputValues"></param>
    /// <param name="desiredOutput"></param>
    /// <returns></returns>
    public List<double> Train(List<double> inputValues, List<double> desiredOutput)
    {
        List<double> outputValues = new List<double>();
        outputValues = CalcOutput(inputValues, desiredOutput); //calculate output
        UpdateWeights(outputValues, desiredOutput);
        return outputValues;
    }

    /// <summary>
    /// Calculate output values
    /// </summary>
    /// <param name="inputValues"></param>
    /// <param name="desiredOutput"></param>
    /// <returns></returns>
    public List<double> CalcOutput(List<double> inputValues, List<double> desiredOutput)
    {
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();

        if (inputValues.Count != numInputs)
        {
            Debug.Log("ERROR: Number of Inputs must be " + numInputs);
            return outputs;
        }

        inputs = new List<double>(inputValues);
        for (int i = 0; i < numHidden + 1; i++) //loop through the layers
        {
            if (i > 0) //if this layer not input layer
            {
                inputs = new List<double>(outputs); //put values from previous layer to current layer
            }
            outputs.Clear(); //after clear outputs for new values

            for (int j = 0; j < layers[i].numNeurons; j++) // loop through the neurons
            {
                double N = 0;
                layers[i].neurons[j].inputs.Clear();

                for (int k = 0; k < layers[i].neurons[j].numInputs; k++) // loop through the inputs in each neuron
                {
                    layers[i].neurons[j].inputs.Add(inputs[k]);
                    N += layers[i].neurons[j].weights[k] * inputs[k]; //calculate value of neuron
                }

                N -= layers[i].neurons[j].bias; //calculate

                if (i == numHidden)
                    layers[i].neurons[j].output = ActivationFunctionOutput(N); //calculate output
                else
                    layers[i].neurons[j].output = ActivationFunction(N);
                outputs.Add(layers[i].neurons[j].output);
            }
        }

        UpdateWeights(outputs, desiredOutput);

        return outputs;
    }

    /// <summary>
    /// Get string of weights
    /// </summary>
    /// <returns></returns>
	public string PrintWeights()
    {
        string weightStr = "";
        foreach (Layer l in layers)
        {
            foreach (Neuron n in l.neurons)
            {
                foreach (double w in n.weights)
                {
                    weightStr += w + ",";
                }
                weightStr += n.bias + ",";
            }
        }
        return weightStr;
    }

    /// <summary>
    /// Load weights from file
    /// </summary>
    /// <param name="weightStr"></param>
	public void LoadWeights(string weightStr)
    {
        if (weightStr == "") return;
        string[] weightValues = weightStr.Split(',');
        int w = 0;
        foreach (Layer l in layers)
        {
            foreach (Neuron n in l.neurons)
            {
                for (int i = 0; i < n.weights.Count; i++)
                {
                    n.weights[i] = System.Convert.ToDouble(weightValues[w]);
                    w++;
                }
                n.bias = System.Convert.ToDouble(weightValues[w]);
                w++;
            }
        }
    }

    /// <summary>
    /// Back propogation function
    /// </summary>
    /// <param name="outputs"></param>
    /// <param name="desiredOutput"></param>
    void UpdateWeights(List<double> outputs, List<double> desiredOutput)
    {
        double error;
        for (int i = numHidden; i >= 0; i--) //layers from the end to start
        {
            for (int j = 0; j < layers[i].numNeurons; j++) // neurons
            {
                if (i == numHidden) //if we at the end 
                {
                    error = desiredOutput[j] - outputs[j];
                    layers[i].neurons[j].errorGradient = outputs[j] * (1 - outputs[j]) * error;
                    //errorGradient calculated with Delta Rule: en.wikipedia.org/wiki/Delta_rule
                }
                else
                {
                    layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1 - layers[i].neurons[j].output);
                    double errorGradSum = 0;
                    for (int p = 0; p < layers[i + 1].numNeurons; p++) // add error gradient sum from the next layer to current
                    {
                        errorGradSum += layers[i + 1].neurons[p].errorGradient * layers[i + 1].neurons[p].weights[j];
                    }
                    layers[i].neurons[j].errorGradient *= errorGradSum; //error gradient sum move from output layer to input layer
                }
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++) //each neuron inputs
                {
                    if (i == numHidden) // if this layer is output layer
                    {
                        error = desiredOutput[j] - outputs[j];
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * error;
                    }
                    else //if this layer not output then we update our weights
                    {
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
                    }
                }
                layers[i].neurons[j].bias += alpha * -1 * layers[i].neurons[j].errorGradient;
            }
        }
    }
    
    #region ActivationFunctions
    double ActivationFunction(double value)
    {
        return TanH(value);
    }

    double ActivationFunctionOutput(double value)
    {
        return TanH(value);
    }

    double Step(double value) //(aka binary step)
    {
        if (value < 0) return 0;
        else return 1;
    }

    double Sigmoid(double value) //(aka logistic regression softstep) really soft, so it's better to use in output
    {
        double k = (double)System.Math.Exp(value);
        return k / (1.0f + k);
    }

    double TanH(double value) // (-1;+1) if u want output with negative values then use TanH
                              // TanH useless for binary results coz it's really difficult to train it
    {
        return (2 * (Sigmoid(2 * value)) - 1);
    }

    double ReLu(double value)
    {
        if (value > 0)
            return value;
        return 0;
    }

    double LeakyReLu(double value)
    {
        if (value < 0)
            return 0.01 * value;
        return value;
    }

    double Sinusoid(double value)
    {
        return Mathf.Sin((float)value);
    }

    double ArtTan(double value)
    {
        return Mathf.Atan((float)value);
    }

    double SoftSign(double value)
    {
        return value / (1 + Mathf.Abs((float)value));
    }
    #endregion
}
