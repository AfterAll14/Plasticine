using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ArrayLengthType = System.UInt32;

namespace EasySerializer
{
    public class CustomSerializable
    {
        public virtual void WriteValues(BinaryDataWriter binaryWriter)
        {

        }
        public virtual void ReadValues(BinaryDataReader binaryReader)
        {

        }

        public void WriteToFile(string filePath, int maxSizeMB = 32)
        {
            BinaryDataWriter dataWriter = new BinaryDataWriter(new byte[1024 * 1024 * maxSizeMB]);

            dataWriter.Write(true);
            WriteValues(dataWriter);

            if (!File.Exists(filePath))
            {
                FileStream f = File.Create(filePath);
                f.Close();
            }

            File.WriteAllBytes(filePath, dataWriter.GetByteArray());
        }

        public void ReadFromFile(string filePath)
        {
            BinaryDataReader dataReader = new BinaryDataReader(File.ReadAllBytes(filePath));
            if (dataReader.ReadBool())
                ReadValues(dataReader);
            else
                throw new Exception("Trying to read invalid data");
        }

        public void PatchSavedData(string filePath, int maxSizeMB = 32)
        {
            BinaryDataReader dataReader = new BinaryDataReader(File.ReadAllBytes(filePath));
            ReadValues(dataReader);
            WriteToFile(filePath, maxSizeMB);
            Console.WriteLine("\"" + filePath + "\" patched!");
        }

        public static void WriteItemToFile(CustomSerializable item, string filePath, int maxSizeMB = 32)
        {
            BinaryDataWriter dataWriter = new BinaryDataWriter(new byte[1024 * 1024 * maxSizeMB]);

            dataWriter.Write(item);

            if (!File.Exists(filePath))
            {
                FileStream f = File.Create(filePath);
                f.Close();
            }

            File.WriteAllBytes(filePath, dataWriter.GetByteArray());
        }

        public static void WriteItemArrayToFile<T>(T[] items, string filePath, int maxSizeMB = 32) where T : CustomSerializable
        {
            BinaryDataWriter dataWriter = new BinaryDataWriter(new byte[1024 * 1024 * maxSizeMB]);

            dataWriter.Write(items);

            if (!File.Exists(filePath))
            {
                FileStream f = File.Create(filePath);
                f.Close();
            }

            File.WriteAllBytes(filePath, dataWriter.GetByteArray());
        }

        public static void WriteItemListToFile<T>(List<T> items, string filePath, int maxSizeMB = 32) where T : CustomSerializable
        {
            BinaryDataWriter dataWriter = new BinaryDataWriter(new byte[1024 * 1024 * maxSizeMB]);

            dataWriter.Write(items);

            if (!File.Exists(filePath))
            {
                FileStream f = File.Create(filePath);
                f.Close();
            }

            File.WriteAllBytes(filePath, dataWriter.GetByteArray());
        }

        public static T ReadItemFromFile<T>(string filePath) where T : CustomSerializable, new()
        {
            BinaryDataReader dataReader = new BinaryDataReader(File.ReadAllBytes(filePath));
            T item = dataReader.ReadValue<T>();

            return item;
        }

        public static T[] ReadItemArrayFromFile<T>(string filePath) where T : CustomSerializable, new()
        {
            BinaryDataReader dataReader = new BinaryDataReader(File.ReadAllBytes(filePath));
            T[] items = dataReader.ReadValueArray<T>();

            return items;
        }

        public static List<T> ReadItemListFromFile<T>(string filePath) where T : CustomSerializable, new()
        {
            T[] items = ReadItemArrayFromFile<T>(filePath);
            List<T> itemList = new List<T>(items);

            return itemList;
        }
    }

    public class BinaryDataReader
    {
        MemoryStream memoryStream;
        BinaryReader binaryReader;

        public BinaryDataReader(byte[] array)
        {
            memoryStream = new MemoryStream(array);
            binaryReader = new BinaryReader(memoryStream);
        }

        public void ResetMemoryPosition()
        {
            memoryStream.Position = 0;
        }

        public bool ReadBool()
        {
            return binaryReader.ReadBoolean();
        }

        public sbyte ReadSByte()
        {
            return binaryReader.ReadSByte();
        }

        public byte ReadByte()
        {
            return binaryReader.ReadByte();
        }

        public short ReadShort()
        {
            return binaryReader.ReadInt16();
        }

        public ushort ReadUShort()
        {
            return binaryReader.ReadUInt16();
        }

        public int ReadInt()
        {
            return binaryReader.ReadInt32();
        }

        public uint ReadUInt()
        {
            return binaryReader.ReadUInt32();
        }

        public long ReadLong()
        {
            return binaryReader.ReadInt64();
        }

        public ulong ReadULong()
        {
            return binaryReader.ReadUInt64();
        }

        public float ReadFloat()
        {
            return binaryReader.ReadSingle();
        }

        public double ReadDouble()
        {
            return binaryReader.ReadDouble();
        }

        public decimal ReadDecimal()
        {
            return binaryReader.ReadDecimal();
        }

        public string ReadString()
        {
            int stringLength = binaryReader.ReadInt32();
            if (stringLength == 0)
                return null;
            byte[] stringBuffer = binaryReader.ReadBytes(stringLength);
            return Encoding.UTF8.GetString(stringBuffer);
        }

        public bool[] ReadBoolArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            bool[] outputArray = new bool[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadBoolean();
            return outputArray;
        }

        public sbyte[] ReadSByteArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            sbyte[] outputArray = new sbyte[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadSByte();
            return outputArray;
        }

        public byte[] ReadByteArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            return binaryReader.ReadBytes((int)arrayLength);
        }

        public short[] ReadShortArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            short[] outputArray = new short[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadInt16();
            return outputArray;
        }

        public ushort[] ReadUShortArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            ushort[] outputArray = new ushort[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadUInt16();
            return outputArray;
        }

        public int[] ReadIntArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            int[] outputArray = new int[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadInt32();
            return outputArray;
        }

        public uint[] ReadUIntArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            uint[] outputArray = new uint[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadUInt32();
            return outputArray;
        }

        public long[] ReadLongArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            long[] outputArray = new long[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadInt64();
            return outputArray;
        }

        public ulong[] ReadULongArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            ulong[] outputArray = new ulong[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadUInt64();
            return outputArray;
        }

        public float[] ReadFloatArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            float[] outputArray = new float[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadSingle();
            return outputArray;
        }

        public double[] ReadDoubleArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            double[] outputArray = new double[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadDouble();
            return outputArray;
        }

        public decimal[] ReadDecimalArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            decimal[] outputArray = new decimal[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = binaryReader.ReadDecimal();
            return outputArray;
        }

        public string[] ReadStringArray()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            string[] outputArray = new string[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = ReadString();
            return outputArray;
        }

        public List<T> ReadList<T>() where T : CustomSerializable, new()
        {
            if (!binaryReader.ReadBoolean())
                return null;

            T[] array = ReadValueArray<T>();
            List<T> list = new List<T>();
            if (array != null)
                list.AddRange(array);
            return list;
        }

        public T ReadValue<T>() where T : CustomSerializable, new()
        {
            if (!binaryReader.ReadBoolean())
                return null;
            T outValue = new T();
            outValue.ReadValues(this);
            return outValue;
        }

        public T[] ReadValueArray<T>() where T : CustomSerializable, new()
        {
            ArrayLengthType arrayLength = ReadArrayLength();
            if (arrayLength == 0)
                return null;
            T[] outputArray = new T[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                outputArray[i] = ReadValue<T>();
            return outputArray;
        }

        ArrayLengthType ReadArrayLength()
        {
            return binaryReader.ReadUInt32();
        }
    }

    public class BinaryDataWriter
    {
        byte[] byteArray;
        MemoryStream memoryStream;
        BinaryWriter binaryWriter;

        public BinaryDataWriter(byte[] array)
        {
            byteArray = array;
            memoryStream = new MemoryStream(byteArray);
            binaryWriter = new BinaryWriter(memoryStream);
        }

        public void ResetMemoryPosition()
        {
            memoryStream.Position = 0;
        }

        public void Write(bool value)
        {
            binaryWriter.Write(value);
        }

        public void Write(byte value)
        {
            binaryWriter.Write(value);
        }

        public void Write(sbyte value)
        {
            binaryWriter.Write(value);
        }

        public void Write(short value)
        {
            binaryWriter.Write(value);
        }

        public void Write(ushort value)
        {
            binaryWriter.Write(value);
        }

        public void Write(int value)
        {
            binaryWriter.Write(value);
        }

        public void Write(uint value)
        {
            binaryWriter.Write(value);
        }

        public void Write(long value)
        {
            binaryWriter.Write(value);
        }

        public void Write(ulong value)
        {
            binaryWriter.Write(value);
        }

        public void Write(float value)
        {
            binaryWriter.Write(value);
        }

        public void Write(double value)
        {
            binaryWriter.Write(value);
        }

        public void Write(decimal value)
        {
            binaryWriter.Write(value);
        }

        public void Write(string value)
        {
            if (value == null)
                binaryWriter.Write(0);
            else
            {
                byte[] stringBuffer = new byte[Encoding.UTF8.GetByteCount(value)];
                int byteCount = Encoding.UTF8.GetBytes(value, 0, value.Length, stringBuffer, 0);

                binaryWriter.Write(byteCount);
                binaryWriter.Write(stringBuffer);
            }
        }

        public void Write(bool[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(sbyte[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(byte[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            binaryWriter.Write(array);
        }

        public void WriteBytes(byte[] array)
        {
            binaryWriter.Write(array);
        }

        public void Write(short[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(ushort[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(int[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(uint[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(long[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(ulong[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(float[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(double[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(decimal[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                binaryWriter.Write(array[i]);
        }

        public void Write(string[] array)
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                Write(array[i]);
        }

        public void Write<T>(List<T> list) where T : CustomSerializable
        {
            if (list == null)
                binaryWriter.Write(false);
            else
            {
                binaryWriter.Write(true);
                Write(list.ToArray());
            }
        }

        public void Write<T>(T value) where T : CustomSerializable
        {
            if (value == null)
                binaryWriter.Write(false);
            else
            {
                binaryWriter.Write(true);
                value.WriteValues(this);
            }
        }

        public void Write<T>(T[] array) where T : CustomSerializable
        {
            if (array == null)
            {
                binaryWriter.Write((ArrayLengthType)0);
                return;
            }

            binaryWriter.Write((ArrayLengthType)array.Length);
            for (int i = 0; i < array.Length; i++)
                if (array[i] == null)
                    binaryWriter.Write(false);
                else
                {
                    binaryWriter.Write(true);
                    array[i].WriteValues(this);
                }
        }

        public byte[] GetByteArray()
        {
            byte[] outputArray = new byte[memoryStream.Position];
            Array.Copy(byteArray, outputArray, memoryStream.Position);
            return outputArray;
        }

        public int GetSize()
        {
            return (int)memoryStream.Position;
        }
    }
}
