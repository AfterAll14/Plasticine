using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using real = System.Double;

namespace Plasticine
{
    public static class NetworkTrainer
    {
        static object consoleLock = new object();

        public static void TrainNetwork(NeuralNetwork neuralNetwork, int epochs, int offspringsCount, real offspringDiversity, List<NeuralNetworkData> trainingDataSet, List<NeuralNetworkData> testDataSet)
        {
            Console.WriteLine("Started training...");

            List<NeuralNetwork> offsprings = new List<NeuralNetwork>();
            List<Task<real>> trainingTasks = new List<Task<real>>();

            real globalWinnerAccuracy = 0;
            NeuralNetwork globalWinner = null;

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                Console.WriteLine("Epoch " + +(epoch + 1) + "/" + epochs);
                int cursorTop = Console.CursorTop;

                offsprings.Clear();
                trainingTasks.Clear();

                for (int i = 0; i < offspringsCount; i++)
                {
                    NeuralNetwork offspring = neuralNetwork.ProduceOffspring(offspringDiversity);
                    string name = "Offspring " + (i + 1);
                    int cursorTopI = cursorTop + i;

                    Task<real> trainingTask = Task<real>.Factory.StartNew(() => ProcessEpoch(offspring, name, trainingDataSet, testDataSet, cursorTopI));

                    offsprings.Add(offspring);
                    trainingTasks.Add(trainingTask);
                }

                NeuralNetwork winner = null;
                real winnerAccuracy = 0;

                for (int i = 0; i < trainingTasks.Count; i++)
                {
                    real accuracy = trainingTasks[i].Result;

                    if (accuracy > winnerAccuracy)
                    {
                        winner = offsprings[i];
                        winnerAccuracy = accuracy;
                    }
                }

                neuralNetwork.CopyWeightsAndBiases(winner);

                if (winnerAccuracy > globalWinnerAccuracy)
                {
                    globalWinnerAccuracy = winnerAccuracy;
                    globalWinner = winner;
                }

                Console.CursorTop = cursorTop + offspringsCount;
                Console.WriteLine("Winner accuracy : " + winnerAccuracy.ToString("0.00") + "%");
            }

            neuralNetwork.CopyWeightsAndBiases(globalWinner);

            Console.WriteLine("Training complete! Achieved accuracy " + globalWinnerAccuracy.ToString("0.00") + "%");
        }

        static real ProcessEpoch(NeuralNetwork neuralNetwork, string name, List<NeuralNetworkData> trainingDataSet, List<NeuralNetworkData> testDataSet, int cursorTop)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < trainingDataSet.Count; i++)
            {
                NeuralNetworkData trainingData = trainingDataSet[i];

                try
                {
                    neuralNetwork.Train(trainingData);
                }
                catch (Exception exception)
                {
                    lock (consoleLock)
                    {
                        Console.WriteLine(exception);
                    }

                    return 0;
                }

                if (i % 100 == 0 || i == trainingDataSet.Count - 1)
                {
                    lock (consoleLock)
                    {
                        Console.CursorTop = cursorTop;
                        Utils.ClearCurrentConsoleLine();

                        real progress = (real)i / (real)(trainingDataSet.Count - 1) * (real)100;
                        Console.WriteLine(name + " training progress " + progress.ToString("0.00") + "%");
                    }
                }
            }

            real accuracy = CalculateAccuracy(neuralNetwork, name, testDataSet, cursorTop, stopwatch);
            return accuracy;
        }

        static real CalculateAccuracy(NeuralNetwork neuralNetwork, string name, List<NeuralNetworkData> testDataSet, int cursorTop, Stopwatch stopwatch)
        {
            real totalResults = 0;
            real correctResults = 0;
            real accuracy = 0;

            for (int i = 0; i < testDataSet.Count; i++)
            {
                NeuralNetworkData testData = testDataSet[i];

                real[] results = neuralNetwork.CalculateResults(testData);

                totalResults++;
                correctResults += testData.CalculateResultAccuracy(results);

                if (i % 100 == 0 || i == testDataSet.Count - 1)
                {
                    lock (consoleLock)
                    {
                        Console.CursorTop = cursorTop;
                        Utils.ClearCurrentConsoleLine();

                        real progress = (real)i / (real)(testDataSet.Count - 1) * (real)100;
                        accuracy = correctResults / totalResults * (real)100;

                        if (i == testDataSet.Count - 1)
                            Console.WriteLine(name + " accuracy: " + accuracy.ToString("0.00") + "%" + "        Processing time: " + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + "s");
                        else
                            Console.WriteLine("Testing " + progress.ToString("0.00") + "%" + " accuracy: " + accuracy.ToString("0.00") + "%");
                    }
                }
            }

            return accuracy;
        }
    }
}