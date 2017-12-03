using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System;
using Random = System.Random;


public class CM242_MC_support : MC_support 
{
    private int[] currentState;

    [Serializable]
    public class StepWithState
    {
        public int a;//whichAction;
        public int c;//numChoices;
        public int[] state;//field layout; 
    }


    [Serializable]
    protected class RequestWithState
    {
        public StepWithState[] prefix;
    }

    [Serializable]
    protected class ResponseWithState
    {
        public StepWithState[] path;
        public int score;
    }

    public class ExperimentFinishedException : Exception {}


    private Queue<StepWithState> pathWithState;
    private Queue<StepWithState> prefixWithState;

    public void SetState(int[] state)
    {
        currentState = state;
    }

    override public int Select(int limit, int[] weights = null, double temperature = 1) 
    {
        Assert.IsTrue(tcpClient.Connected);
        if (weights != null)
        {
            Assert.AreEqual(limit, weights.Length);
        }

        if (prefixWithState == null)
        {
            string line = reader.ReadLine();
            if (line == null)
            {
                throw new ExperimentFinishedException();
            }
            RequestWithState request = JsonUtility.FromJson<RequestWithState>(line);
            prefixWithState = new Queue<StepWithState>(request.prefix);
            pathWithState = new Queue<StepWithState>();
        }

        StepWithState s;

        if (prefixWithState.Count() > 0) 
        {
            s = prefixWithState.Dequeue();
            Assert.AreEqual(limit, s.c);//numChoices); 
        } 
        else 
        {
            s = new StepWithState();
            s.c/*numChoices*/ = limit;
            if (weights != null)
            {
                s.a/*which action*/ = GetWeightedSelection(weights, temperature);
            }
            else
                s.a/*whichAction*/ = random.Next(limit);
            s.state = currentState;
        }
        pathWithState.Enqueue(s);
        return s.a;//whichAction;
    }

    private int GetWeightedSelection(int[] weights, double temperature)
    {
        int numWeights = weights.Count();
        double[] expWeights = new double[numWeights];
        double expSum = 0;
        for (int i = 0; i < numWeights; i++)
        {
            expWeights[i] = Math.Exp(weights[i]/temperature);
            expSum += expWeights[i];
        }
        double randomChoice = expSum * random.NextDouble();

        double total = 0;
        int selection = 0;
        for (selection = 0; selection < numWeights; selection++)
        {
            total += expWeights[selection];
            if (total >= randomChoice)
                break;
        }
        return selection;
    }


    override public void SupplyOutcome(int score)
    {
        ResponseWithState response = new ResponseWithState();
        response.score = score;
        response.path = pathWithState.ToArray();
        string line = JsonUtility.ToJson(response);
        writer.WriteLine(line);
        prefixWithState = null;
        pathWithState = null;
    }
}
