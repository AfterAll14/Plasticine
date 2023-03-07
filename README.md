Plasticine is a minimalistic neural network library

Validation results: achieved 97.5% accuracy on MNIST handwritten digits dataset with 2 hidden layers 128 neurons each.

Example usage:

```
// init datasets
List<NeuralNetworkData> trainingDataSet = new List<NeuralNetworkData>();
List<NeuralNetworkData> testDataSet = new List<NeuralNetworkData>();

// add data to datasets or load from file
SomeFunctionToLoadData(trainingDataSet, testDataSet);

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
```