using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using real = System.Double;

namespace Plasticine
{
    public class Layer
    {
        public Layer previousLayer { get; private set; }

        real[][] weights;
        real[] biases;

        real[] weightedSums;

        real[][] accumulatedWeightsGradients;
        real[] accumulatedBiasesGradients;

        real[][] weightDeltas;
        real[] biasesDeltas;

        public real[] activations { get; private set; }
        public real[] input { get; private set; }
        public real[] activationsDerivatives { get; private set; }

        real[][][] dZdZ_matrices;

        Func<real[], int, (real, real)> activationFunction;

        public int neurons
        {
            get
            {
                return biases == null ? 0 : biases.Length;
            }
        }

        public Layer(Layer previousLayer, int neurons, Func<real[], int, (real, real)> activationFunction) : this(previousLayer.neurons, neurons, activationFunction)
        {
            this.previousLayer = previousLayer;
        }

        public Layer(int activations, int neurons, Func<real[], int, (real, real)> activationFunction)
        {
            this.activationFunction = activationFunction;

            biases = new real[neurons];
            accumulatedBiasesGradients = new real[neurons];
            biasesDeltas = new real[neurons];

            weights = new real[neurons][];
            accumulatedWeightsGradients = new real[neurons][];
            weightDeltas = new real[neurons][];

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = new real[activations];
                accumulatedWeightsGradients[i] = new real[activations];
                weightDeltas[i] = new real[activations];
            }

            weightedSums = new real[neurons];
            this.activations = new real[neurons];
            activationsDerivatives = new real[neurons];

            InitializeWeightsAndBiases();
            ResetAccumulation();
            InitializeDeltas();
        }

        void InitializeWeightsAndBiases()
        {
            real maxValue = (real)1 / (real)(weights[0].Length + 1);
            Random randomNumberGenerator = new Random();

            for (int i = 0; i < neurons; i++)
            {
                biases[i] = ((real)randomNumberGenerator.NextDouble() - (real)0.5) * maxValue;

                for (int j = 0; j < weights[i].Length; j++)
                    weights[i][j] = ((real)randomNumberGenerator.NextDouble() - (real)0.5) * maxValue;
            }
        }

        void ResetAccumulation()
        {
            for (int i = 0; i < neurons; i++)
            {
                accumulatedBiasesGradients[i] = 0;

                for (int j = 0; j < accumulatedWeightsGradients[i].Length; j++)
                    accumulatedWeightsGradients[i][j] = 0;
            }
        }

        void InitializeDeltas()
        {
            for (int i = 0; i < neurons; i++)
            {
                biasesDeltas[i] = 0;

                for (int j = 0; j < accumulatedWeightsGradients[i].Length; j++)
                    weightDeltas[i][j] = 0;
            }
        }

        public void ApplyAccumulation(real batchSize, real accumulatedLearningCoefficient, real momentumCoefficient)
        {
            for (int i = 0; i < neurons; i++)
            {
                real biasDelta = -(accumulatedLearningCoefficient * accumulatedBiasesGradients[i]) / batchSize;
                biasesDeltas[i] = biasesDeltas[i] * momentumCoefficient + biasDelta;
                biases[i] += biasesDeltas[i];

                for (int j = 0; j < weights[i].Length; j++)
                {
                    real weightDelta = -(accumulatedLearningCoefficient * accumulatedWeightsGradients[i][j]) / batchSize;
                    weightDeltas[i][j] = weightDeltas[i][j] * momentumCoefficient + weightDelta;
                    weights[i][j] += weightDeltas[i][j];
                }
            }

            ResetAccumulation();
        }

        public void AccumulateBiasGradient(int neuronId, real gradient)
        {
            accumulatedBiasesGradients[neuronId] += gradient;
        }

        public void AccumulateWeightGradient(int neuronId, int weightId, real gradient)
        {
            accumulatedWeightsGradients[neuronId][weightId] += gradient;
        }

        public void Activate(real[] input)
        {
            if (input.Length != weights[0].Length)
                throw new Exception("Layer::Propagate - activation input length doesn't match layer activations");

            this.input = input;

            for (int neuronId = 0; neuronId < neurons; neuronId++)
            {
                weightedSums[neuronId] = biases[neuronId];

                for (int inputId = 0; inputId < input.Length; inputId++)
                {
                    weightedSums[neuronId] += weights[neuronId][inputId] * input[inputId];
                }
            }

            for (int neuronId = 0; neuronId < neurons; neuronId++)
            {
                (activations[neuronId], activationsDerivatives[neuronId]) = activationFunction(weightedSums, neuronId);
            }
        }

        void GeneratedZdZ_matrices()
        {
            if (previousLayer == null)
                return;

            int layerCounter = 0;

            Layer layerIterator = previousLayer;
            while (layerIterator != null)
            {
                layerIterator = layerIterator.previousLayer;
                layerCounter++;
            }

            dZdZ_matrices = new real[layerCounter][][];
            layerIterator = previousLayer;

            for (int layerId = 0; layerId < dZdZ_matrices.Length; layerId++)
            {
                real[][] dZdZ_matrix = new real[neurons][];
                for (int neuronId = 0; neuronId < neurons; neuronId++)
                {
                    dZdZ_matrix[neuronId] = new real[layerIterator.neurons];
                }

                dZdZ_matrices[layerId] = dZdZ_matrix;
                layerIterator = layerIterator.previousLayer;
            }
        }

        public void Calculate_dZdZ_matrices()
        {
            if (previousLayer == null)
                return;

            if (dZdZ_matrices == null)
                GeneratedZdZ_matrices();

            Layer layerIterator = previousLayer;

            for (int layerId = 0; layerId < dZdZ_matrices.Length; layerId++)
            {
                real[][] dZdZ_matrix = dZdZ_matrices[layerId];

                for (int neuronId = 0; neuronId < neurons; neuronId++)
                {
                    for (int iteratedLayerNeuronId = 0; iteratedLayerNeuronId < layerIterator.neurons; iteratedLayerNeuronId++)
                    {
                        if (layerId == 0)
                            dZdZ_matrix[neuronId][iteratedLayerNeuronId] = weights[neuronId][iteratedLayerNeuronId] * previousLayer.activationsDerivatives[iteratedLayerNeuronId];
                        else
                        {
                            real sum = 0;

                            for (int inputId = 0; inputId < input.Length; inputId++)
                                sum += weights[neuronId][inputId] * previousLayer.activationsDerivatives[inputId] * previousLayer.Get_dZdZ(layerId - 1, inputId, iteratedLayerNeuronId);

                            dZdZ_matrix[neuronId][iteratedLayerNeuronId] = sum;
                        }
                    }
                }

                layerIterator = layerIterator.previousLayer;
            }
        }

        public real Get_dZdZ(int layerDepth, int neuronId, int targetNeuronId)
        {
            if (layerDepth == -1)
                return neuronId == targetNeuronId ? 1 : 0;

            return dZdZ_matrices[layerDepth][neuronId][targetNeuronId];
        }

        public Layer ProduceOffspring(Layer previousLayer, real diversity)
        {
            Layer offspring = previousLayer == null ? new Layer(weights[0].Length, neurons, activationFunction) : new Layer(previousLayer, neurons, activationFunction);

            Random rng = new Random();
            real diversityFactor = diversity * 2;

            for (int i = 0; i < biases.Length; i++)
            {
                real biasMutation = (real)(rng.NextDouble() - 0.5) * diversityFactor;
                offspring.biases[i] = biases[i] * ((real)1 + biasMutation);

                for (int j = 0; j < weights[i].Length; j++)
                {
                    real weightMutation = (real)(rng.NextDouble() - 0.5) * diversityFactor;
                    offspring.weights[i][j] = weights[i][j] * ((real)1 + weightMutation);
                }
            }

            return offspring;
        }

        public void CopyWeightsAndBiases(Layer layer)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                biases[i] = layer.biases[i];

                for (int j = 0; j < weights[i].Length; j++)
                    weights[i][j] = layer.weights[i][j];
            }

            ResetAccumulation();
        }
    }

    public abstract class NeuralNetworkData
    {
        abstract public real[] GetInput();

        abstract public real[] GetCorrectResult();

        abstract public real CalculateResultAccuracy(real[] nnOuput);
    }

    public class NeuralNetwork
    {
        List<Layer> layers;
        Layer finalLayer;

        real minTrainingCoefficient;

        int batchCounter = 0;
        int batchSize;

        real momentumCoefficient;

        real accumulatedLearningCoefficient = 0;

        public NeuralNetwork(Layer finalLayer, real minTrainingCoefficient, int batchSize, real momentumCoefficient)
        {
            layers = new List<Layer>();
            layers.Add(finalLayer);

            Layer layerIterator = finalLayer.previousLayer;

            while (layerIterator != null)
            {
                layers.Add(layerIterator);
                layerIterator = layerIterator.previousLayer;
            }

            layers.Reverse();
            this.finalLayer = finalLayer;

            this.minTrainingCoefficient = minTrainingCoefficient;
            this.batchSize = batchSize;

            this.momentumCoefficient = momentumCoefficient;
        }

        public real[] CalculateResults(NeuralNetworkData data)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Activate(i == 0 ? data.GetInput() : layers[i - 1].activations);
            }

            return layers[layers.Count - 1].activations;
        }

        (real, real[]) CostFunction(real[] results, real[] correctResults)
        {
            real cost = 0;
            real[] costsDerivatives = new real[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                cost += (results[i] - correctResults[i]) * (results[i] - correctResults[i]);
                costsDerivatives[i] = 2 * (results[i] - correctResults[i]);
            }

            real maxCost = results.Length;
            real learningCoefficient = Math.Max(minTrainingCoefficient, cost * cost / (maxCost * maxCost));

            return (learningCoefficient, costsDerivatives);
        }

        public void Train(NeuralNetworkData data)
        {
            real[] results = CalculateResults(data);

            (real learningCoefficient, real[] costsDerivatives) = CostFunction(results, data.GetCorrectResult());

            accumulatedLearningCoefficient += learningCoefficient;

            for (int i = 0; i < layers.Count; i++)
                layers[i].Calculate_dZdZ_matrices();

            real[] dCidZi = new real[finalLayer.neurons];

            for (int i = 0; i < finalLayer.neurons; i++)
                dCidZi[i] = costsDerivatives[i] * finalLayer.activationsDerivatives[i];

            TrainLayers(dCidZi);
        }

        void TrainLayers(real[] dCidZi)
        {
            for (int layerId = layers.Count - 1; layerId >= 0; layerId--)
                TrainLayer(layerId, dCidZi);

            batchCounter++;

            if (batchCounter >= batchSize)
            {
                for (int i = 0; i < layers.Count; i++)
                    layers[i].ApplyAccumulation(batchCounter, accumulatedLearningCoefficient, momentumCoefficient);

                batchCounter = 0;
                accumulatedLearningCoefficient = 0;
            }
        }

        void TrainLayer(int layerId, real[] dCidZi)
        {
            Layer targetLayer = layers[layerId];
            int targetLayerDepth = layers.Count - 2 - layerId;

            for (int targetNeuronId = 0; targetNeuronId < targetLayer.neurons; targetNeuronId++)
            {
                real dCdBi = 0;

                for (int i = 0; i < dCidZi.Length; i++)
                    dCdBi += dCidZi[i] * finalLayer.Get_dZdZ(targetLayerDepth, i, targetNeuronId);

                targetLayer.AccumulateBiasGradient(targetNeuronId, dCdBi);

                for (int targetWeightId = 0; targetWeightId < targetLayer.input.Length; targetWeightId++)
                {
                    real dCdWij = targetLayer.input[targetWeightId] * dCdBi;
                    targetLayer.AccumulateWeightGradient(targetNeuronId, targetWeightId, dCdWij);
                }
            }
        }

        public NeuralNetwork ProduceOffspring(real diversity)
        {
            List<Layer> offspringLayers = new List<Layer>();
            for (int i = 0; i < layers.Count; i++)
                offspringLayers.Add(layers[i].ProduceOffspring(i == 0 ? null : offspringLayers[i - 1], diversity));

            NeuralNetwork offspring = new NeuralNetwork(offspringLayers[offspringLayers.Count - 1], minTrainingCoefficient, batchSize, momentumCoefficient);
            return offspring;
        }

        public void CopyWeightsAndBiases(NeuralNetwork neuralNetwork)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].CopyWeightsAndBiases(neuralNetwork.layers[i]);

            batchCounter = 0;
        }
    }

    public static class NetworkTrainer
    {
        static object consoleLock = new object();

        public static void EvolveNetwork(NeuralNetwork neuralNetwork, int epochs, int offspringsCount, real offspringDiversity, List<NeuralNetworkData> trainingDataSet, List<NeuralNetworkData> testDataSet)
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

                    Task<real> trainingTask = Task<real>.Factory.StartNew(() => TrainNetwork(offspring, name, trainingDataSet, testDataSet, cursorTopI));

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

        static real TrainNetwork(NeuralNetwork neuralNetwork, string name, List<NeuralNetworkData> trainingDataSet, List<NeuralNetworkData> testDataSet, int cursorTop)
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

            real accuracy = TestNetwork(neuralNetwork, name, testDataSet, cursorTop, stopwatch);
            return accuracy;
        }

        static real TestNetwork(NeuralNetwork neuralNetwork, string name, List<NeuralNetworkData> testDataSet, int cursorTop, Stopwatch stopwatch)
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
