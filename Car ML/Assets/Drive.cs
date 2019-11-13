using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Add data to file to teach algorithm how work
/// </summary>
public class Drive : MonoBehaviour
{
    //если в проекте нет trainingData.txt файла, нужно запустить проект и проехать на машине один/два круга
    //чтобы создать и забить файл тренировочными данными. Ќа этих данных другой алгоритм будет обучатьс€ ездить.
    #region Fields
    public float speed = 200.0F;
    public float rotationSpeed = 100.0F;
    public float visibleDistance = 50.0f;
    List<string> collectedTrainingData = new List<string>();
    //store our weights if we don't want to learn our program
    StreamWriter storedDataWeights;

    float translationInputButton;
    float rotationInputButton;
    float translation;
    float rotation;

    string trainingData;

    StringBuilder stringBuilder;

    //raycasts
    RaycastHit hit;
    float forwardDist = 0, rightDist = 0, leftDist = 0, right45Dist = 0, left45Dist = 0;
    #endregion;

    #region Methods

    void Start()
    {
        string path = Application.dataPath + "/trainingData.txt";
        storedDataWeights = File.CreateText(path);
        stringBuilder = new StringBuilder();
    }

    /// <summary>
    /// Save data to file
    /// </summary>
    void OnApplicationQuit()
    {
        foreach (string trainData in collectedTrainingData)
        {
            storedDataWeights.WriteLine(trainData);
        }
        storedDataWeights.Close();
    }

    /// <summary>
    /// ќкугление числа
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    float Round(float number)
    {
        return (float)System.Math.Round(number, System.MidpointRounding.AwayFromZero) / 2.0f;
    }

    void Update()
    {
        stringBuilder.Remove(0, stringBuilder.Length);
        translationInputButton = Input.GetAxis("Vertical");
        rotationInputButton = Input.GetAxis("Horizontal");

        translation = Time.deltaTime * speed * translationInputButton;
        rotation = Time.deltaTime * rotationSpeed * rotationInputButton;
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

        //draw rays for car. We want to understand where our ray is stop
        Debug.DrawRay(transform.position, transform.forward * visibleDistance, Color.red);
        Debug.DrawRay(transform.position, transform.right * visibleDistance, Color.red);
        Debug.DrawRay(transform.position, -transform.right * visibleDistance, Color.red);

        //look left
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * -transform.right * visibleDistance, Color.green);
        //look right
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45, Vector3.up) * transform.right * visibleDistance, Color.green);
        //forward
        if (Physics.Raycast(transform.position, transform.forward, out hit, visibleDistance))
            forwardDist = 1 - Round(hit.distance / visibleDistance);


        #region Calculate distances

        //right
        if (Physics.Raycast(transform.position, transform.right, out hit, visibleDistance))
        {
            rightDist = 1 - Round(hit.distance / visibleDistance);
        }

        //left
        if (Physics.Raycast(transform.position, -transform.right, out hit, visibleDistance))
        {
            leftDist = 1 - Round(hit.distance / visibleDistance);
        }

        //right 45
        if (Physics.Raycast(transform.position,
                            Quaternion.AngleAxis(-45, Vector3.up) * transform.right, out hit, visibleDistance))
        {
            right45Dist = 1 - Round(hit.distance / visibleDistance);
        }

        //left 45
        if (Physics.Raycast(transform.position,
                            Quaternion.AngleAxis(45, Vector3.up) * -transform.right, out hit, visibleDistance))
        {
            left45Dist = 1 - Round(hit.distance / visibleDistance);
        }
        #endregion

        //logic how store our data
        AddStrings(forwardDist, rightDist, leftDist,
            right45Dist, left45Dist, Round(translationInputButton), Round(rotationInputButton));

        //add data if it doesn't contain
        if (translationInputButton != 0 && rotationInputButton != 0)
        {
            if (!collectedTrainingData.Contains(stringBuilder.ToString()))
            {
                collectedTrainingData.Add(stringBuilder.ToString());
            }
        }


        //trainingData = forwardDist + "," + rightDist + "," + leftDist + "," +
        //              right45Dist + "," + left45Dist + "," +
        //              Round(translationInputButton) + "," + Round(rotationInputButton);

        //add data if it doesn't contain
        //if (translationInputButton != 0 && rotationInputButton != 0)
        //{
        //    if (!collectedTrainingData.Contains(trainingData))
        //    {
        //        collectedTrainingData.Add(trainingData);
        //    }
        //}


    }

    /// <summary>
    /// Add strings to string builder
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="right"></param>
    /// <param name="left"></param>
    /// <param name="right45"></param>
    /// <param name="left45"></param>
    /// <param name="translation"></param>
    /// <param name="rotation"></param>
    public void AddStrings(float forward, float right, float left, float right45, float left45, float translation, float rotation)
    {
        stringBuilder.Append(forward);
        stringBuilder.Append(",");
        stringBuilder.Append(right);
        stringBuilder.Append(",");
        stringBuilder.Append(left);
        stringBuilder.Append(",");
        stringBuilder.Append(right45);
        stringBuilder.Append(",");
        stringBuilder.Append(left45);
        stringBuilder.Append(",");
        stringBuilder.Append(translation);
        stringBuilder.Append(",");
        stringBuilder.Append(rotation);
    }
    #endregion
}
