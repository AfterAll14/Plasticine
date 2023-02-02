using EasySerializer;
using System;
using System.Collections.Generic;
using real = System.Double;

namespace Plasticine
{
    public class NeuralNetwork : CustomSerializable
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

        public NeuralNetwork(string filePath)
        {
            ReadFromFile(filePath);
        }

        public real[] CalculateResults(NeuralNetworkData data)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Activate(i == 0 ? data.GetInput() : layers[i - 1].activatedNeuronsValues);
            }

            return layers[layers.Count - 1].activatedNeuronsValues;
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

                for (int targetWeightId = 0; targetWeightId < targetLayer.inputActivations.Length; targetWeightId++)
                {
                    real dCdWij = targetLayer.inputActivations[targetWeightId] * dCdBi;
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

        public override void WriteValues(BinaryDataWriter binaryWriter)
        {
            binaryWriter.Write(minTrainingCoefficient);
            binaryWriter.Write(batchSize);
            binaryWriter.Write(momentumCoefficient);
            binaryWriter.Write(layers.Count);

            for (int i = 0; i < layers.Count; i++)
            {
                Layer layer = layers[i];

                binaryWriter.Write(layer.activations);
                binaryWriter.Write(layer.neurons);
                binaryWriter.Write(layer.activationFunction.Method.Name);
                layer.WriteValues(binaryWriter);
            }
        }

        public override void ReadValues(BinaryDataReader binaryReader)
        {
            minTrainingCoefficient = binaryReader.ReadDouble();
            batchSize = binaryReader.ReadInt();
            momentumCoefficient = binaryReader.ReadDouble();
            int layersCount = binaryReader.ReadInt();

            layers = new List<Layer>();

            for (int i = 0; i < layersCount; i++)
            {
                int layerActivations = binaryReader.ReadInt();
                int layerNeurons = binaryReader.ReadInt();
                
                string layerActivationFunctionName = binaryReader.ReadString();
                Func<real[], int, (real, real)> layerActivationFunction = (Func<real[], int, (real, real)>)typeof(ActivationFunctions).GetMethod(layerActivationFunctionName).CreateDelegate(typeof(Func<real[], int, (real, real)>));

                Layer layer = i == 0 ? new Layer(layerActivations, layerNeurons, layerActivationFunction) : new Layer(layers[i - 1], layerNeurons, layerActivationFunction);
                layer.ReadValues(binaryReader);

                layers.Add(layer);
            }

            finalLayer = layers[layers.Count - 1];
        }
    }
}
