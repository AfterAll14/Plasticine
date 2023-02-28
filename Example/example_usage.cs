using System.Collections.Generic;

namespace Plasticine
{
    class UsageExample
    {
        static List<NeuralNetworkData> trainingDataSet = new List<NeuralNetworkData>();
        static List<NeuralNetworkData> testDataSet = new List<NeuralNetworkData>();

        public static void CreateNetworkAndTrain()
        {
            // neural network parameters
            int inputNeurons = 784;
            int layer1Neurons = 128;
            int layer2Neurons = 128;
            int outputNeurons = 10;

            // create neural network
            Layer layer1 = new Layer(inputNeurons, layer1Neurons, ActivationFunctions.LeakyReLU);
            Layer layer2 = new Layer(layer1, layer2Neurons, ActivationFunctions.LeakyReLU);
            Layer layer3 = new Layer(layer2, outputNeurons, ActivationFunctions.Sigmoid);
            NeuralNetwork neuralNetwork = new NeuralNetwork(layer3, minTrainingCoefficient: 0.01f, batchSize: 1, momentumCoefficient: 0.0f);

            // train
            NetworkTrainer.TrainNetwork(neuralNetwork, epochs: 4, offspringsCount: 1, offspringDiversity: 0.0f, trainingDataSet, testDataSet);
        }
    }
}