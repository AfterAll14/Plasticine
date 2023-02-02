using EasySerializer;
using System;
using real = System.Double;

namespace Plasticine
{
    public class Layer : CustomSerializable
    {
        public Layer previousLayer { get; private set; }

        real[][] weights;
        real[] biases;

        real[] weightedSums;

        real[][] accumulatedWeightsGradients;
        real[] accumulatedBiasesGradients;

        real[][] weightDeltas;
        real[] biasesDeltas;

        public real[] activatedNeuronsValues { get; private set; }
        public real[] inputActivations { get; private set; }
        public real[] activationsDerivatives { get; private set; }

        real[][][] dZdZ_matrices;

        public Func<real[], int, (real, real)> activationFunction;

        public int neurons
        {
            get
            {
                return biases == null ? 0 : biases.Length;
            }
        }

        public int activations
        {
            get
            {
                return weights == null ? 0 : weights[0].Length;
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
            this.activatedNeuronsValues = new real[neurons];
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

            this.inputActivations = input;

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
                (activatedNeuronsValues[neuronId], activationsDerivatives[neuronId]) = activationFunction(weightedSums, neuronId);
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

                            for (int inputId = 0; inputId < inputActivations.Length; inputId++)
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

        public override void WriteValues(BinaryDataWriter binaryWriter)
        {
            binaryWriter.Write(biases);

            for (int i = 0; i < weights.Length; i++)
                binaryWriter.Write(weights[i]);
        }

        public override void ReadValues(BinaryDataReader binaryReader)
        {
            biases = binaryReader.ReadDoubleArray();

            for (int i = 0; i < weights.Length; i++)
                weights[i] = binaryReader.ReadDoubleArray();
        }
    }
}