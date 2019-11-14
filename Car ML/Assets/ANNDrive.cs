using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ANNDrive : MonoBehaviour
{
    //we can try to play a lot of times for good results coz our neural network can find local optima, not global for good result
    #region Fields
    ANN ann;
    public float visibleDistance = 50;
    public int epochs = 1000;//count of iterations for teaching
    public float speed = 50.0F;
    public float rotationSpeed = 100.0F;

    bool trainingDone = false;
    float trainingProgress = 0;
    double sse = 0; //sum square error
    double lastSSE = 1;

    public float translation;
    public float rotation;

    public bool loadFromFile = true;

    List<double> calcOutputs;
    List<double> inputs;
    List<double> outputs;

    #endregion


    // Use this for initialization
    void Start()
    {
        ann = new ANN(5, 2, 1, 10, 0.5); //create perceptron
        if (loadFromFile)
        {
            LoadWeightsFromFile();
            trainingDone = true;
        }
        else
            StartCoroutine(LoadTrainingSet());
    }

    void OnGUI()
    {
        GUI.Label(new Rect(25, 25, 250, 30), "SSE: " + lastSSE);
        GUI.Label(new Rect(25, 40, 250, 30), "Alpha: " + ann.alpha);
        GUI.Label(new Rect(25, 55, 250, 30), "Trained: " + trainingProgress);
    }

    /// <summary>
    /// Load file, train data, calculate sse, increase or decrease alpha in the result
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadTrainingSet()
    {
        //string path = Application.dataPath + "/trainingDataStripped.txt"; //use it for the best training set
        string path = Application.dataPath + "/trainingData.txt";
        string line;
        if (File.Exists(path))
        {
            int lineCount = File.ReadAllLines(path).Length;
            StreamReader trainingDataFile = File.OpenText(path);
            List<double> calcOutputs = new List<double>();
            List<double> inputs = new List<double>();
            List<double> outputs = new List<double>();

            //if we have data => load it, train data, calculate sse, increase or decrease alpha in the result
            for (int i = 0; i < epochs; i++)
            {
                //set file pointer to beginning of file
                sse = 0;
                trainingDataFile.BaseStream.Position = 0;
                string currentWeights = ann.PrintWeights();

                while ((line = trainingDataFile.ReadLine()) != null)
                {
                    string[] data = line.Split(',');
                    //if nothing to be learned ignore this line
                    float thisError = 0;
                    //training data has only 5 inputs and the end is our translation
                    //if both of this equal to zero then our user didn't doing anything
                    //we don't want data is going to undo training from the other data sets
                    if (System.Convert.ToDouble(data[5]) != 0 && System.Convert.ToDouble(data[6]) != 0)
                    {
                        inputs.Clear();
                        outputs.Clear();
                        inputs.Add(System.Convert.ToDouble(data[0]));
                        inputs.Add(System.Convert.ToDouble(data[1]));
                        inputs.Add(System.Convert.ToDouble(data[2]));
                        inputs.Add(System.Convert.ToDouble(data[3]));
                        inputs.Add(System.Convert.ToDouble(data[4]));

                        //translation
                        double translationOutput = Map(0, 1, -1, 1, System.Convert.ToSingle(data[5]));
                        outputs.Add(translationOutput);
                        //rotation
                        double rotationOutput = Map(0, 1, -1, 1, System.Convert.ToSingle(data[6]));
                        outputs.Add(rotationOutput);

                        //train data
                        calcOutputs = ann.Train(inputs, outputs);
                        //calculate error via translation and rotation
                        thisError = ((Mathf.Pow((float)(outputs[0] - calcOutputs[0]), 2) +
                            Mathf.Pow((float)(outputs[1] - calcOutputs[1]), 2))) / 2.0f;
                    }
                    sse += thisError;
                }
                trainingProgress = (float)i / (float)epochs;
                sse /= lineCount;

                //if sse isn't better then reload previous set of weights
                //then decrease alpha
                if (lastSSE < sse)
                {
                    ann.LoadWeights(currentWeights);
                    ann.alpha = Mathf.Clamp((float)ann.alpha - 0.001f, 0.01f, 0.9f);
                }
                else //increase alpha
                {
                    ann.alpha = Mathf.Clamp((float)ann.alpha + 0.001f, 0.01f, 0.9f);
                    lastSSE = sse;
                }

                yield return null;
            }

        }
        trainingDone = true;
        SaveWeightsToFile();
    }

    /// <summary>
    /// Save file weights.txt
    /// </summary>
    void SaveWeightsToFile()
    {
        string path = Application.dataPath + "/weights.txt";
        StreamWriter wf = File.CreateText(path);
        wf.WriteLine(ann.PrintWeights());
        wf.Close();
    }

    /// <summary>
    /// Load weight.txt
    /// </summary>
    void LoadWeightsFromFile()
    {
        string path = Application.dataPath + "/weights.txt";
        StreamReader wf = File.OpenText(path);

        if (File.Exists(path))
        {
            string line = wf.ReadLine();
            ann.LoadWeights(line);
        }
    }

    /// <summary>
    /// map new value between -1 and 1
    /// </summary>
    /// <param name="newfrom"></param>
    /// <param name="newto"></param>
    /// <param name="origfrom"></param>
    /// <param name="origto"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    float Map(float newfrom, float newto, float origfrom, float origto, float value)
    {
        if (value <= origfrom)
            return newfrom;
        else if (value >= origto)
            return newto;
        return (newto - newfrom) * ((value - origfrom) / (origto - origfrom)) + newfrom;
    }

    /// <summary>
    /// Round the number
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    float Round(float x)
    {
        return (float)System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 2.0f;
    }

    void Update()
    {
        if (!trainingDone) return;

        calcOutputs = new List<double>();
        inputs = new List<double>();
        outputs = new List<double>();

        #region Raycasts
        //raycasts
        RaycastHit hit;
        float fDist = 0, rDist = 0, lDist = 0, r45Dist = 0, l45Dist = 0;

        //forward
        if (Physics.Raycast(transform.position, transform.forward, out hit, visibleDistance))
        {
            fDist = 1 - Round(hit.distance / visibleDistance);
        }

        //right
        if (Physics.Raycast(transform.position, transform.right, out hit, visibleDistance))
        {
            rDist = 1 - Round(hit.distance / visibleDistance);
        }

        //left
        if (Physics.Raycast(transform.position, -transform.right, out hit, visibleDistance))
        {
            lDist = 1 - Round(hit.distance / visibleDistance);
        }

        //right 45
        if (Physics.Raycast(transform.position,
                            Quaternion.AngleAxis(-45, Vector3.up) * transform.right, out hit, visibleDistance))
        {
            r45Dist = 1 - Round(hit.distance / visibleDistance);
        }

        //left 45
        if (Physics.Raycast(transform.position,
                            Quaternion.AngleAxis(45, Vector3.up) * -transform.right, out hit, visibleDistance))
        {
            l45Dist = 1 - Round(hit.distance / visibleDistance);
        }

#endregion

        inputs.Add(fDist);
        inputs.Add(rDist);
        inputs.Add(lDist);
        inputs.Add(r45Dist);
        inputs.Add(l45Dist);

        outputs.Add(0);
        outputs.Add(0);

        calcOutputs = ann.CalcOutput(inputs, outputs);

        float translationInput = Map(-1, 1, 0, 1, (float)calcOutputs[0]);
        float rotationInput = Map(-1, 1, 0, 1, (float)calcOutputs[1]);

        translation = translationInput * speed * Time.deltaTime;
        rotation = rotationInput * rotationSpeed * Time.deltaTime;

        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);
    }
}
