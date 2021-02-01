using CitizenFX.Core;
using System.Collections.Generic;
using System.IO;

namespace TrafficManager.Helpers
{
    public static class Extensions
    {
        public static void Write(this BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        public static void Write<T>(this BinaryWriter writer, T item) where T : ISerialisable
        {
            item.Serialise(writer);
        }

        public static void Write<T>(this BinaryWriter writer, List<T> list) where T : ISerialisable
        {
            writer.Write(list.Count);
            foreach (ISerialisable item in list)
            {
                item.Serialise(writer);
            }
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            Vector3 vector = new Vector3
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };
            return vector;
        }

        public static T Read<T>(this BinaryReader reader) where T : ISerialisable, new()
        {
            T item = new T();
            item.Deserialise(reader);
            return item;
        }

        public static List<T> ReadList<T>(this BinaryReader reader) where T : ISerialisable, new()
        {
            List<T> list = new List<T>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                T item = reader.Read<T>();
                list.Add(item);
            }
            return list;
        }
    }
}
