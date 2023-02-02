using EasySerializer;
using System;
using real = System.Double;

namespace Plasticine
{
    public class NeuralNetworkData : CustomSerializable
    {
        protected real[] input;
        protected real[] correctResults;

        public NeuralNetworkData(real[] input, real[] correctResults)
        {
            this.input = input;
            this.correctResults = correctResults;
        }

        public NeuralNetworkData()
        {

        }

        public real[] GetInput()
        {
            return input;
        }

        public real[] GetCorrectResult()
        {
            return correctResults;
        }

        virtual public real CalculateResultAccuracy(real[] nnOuput)
        {
            int correctPredictions = 0;

            for (int i = 0; i < correctResults.Length; i++)
            {
                if (Math.Abs(correctResults[i] - nnOuput[i]) < (real)0.05)
                    correctPredictions++;
            }

            return (real)correctPredictions / (real)correctResults.Length;
        }

        public override void WriteValues(BinaryDataWriter binaryWriter)
        {
            binaryWriter.Write(input);
            binaryWriter.Write(correctResults);
        }

        public override void ReadValues(BinaryDataReader binaryReader)
        {
            input = binaryReader.ReadDoubleArray();
            correctResults = binaryReader.ReadDoubleArray();
        }
    }
}