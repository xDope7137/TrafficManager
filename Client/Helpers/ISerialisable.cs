using System.IO;

namespace TrafficManager.Helpers
{
    public interface ISerialisable
    {
        public void Serialise(BinaryWriter writer);

        public void Deserialise(BinaryReader reader);
    }
}
