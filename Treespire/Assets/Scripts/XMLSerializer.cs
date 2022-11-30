using System.IO;
using System.Xml.Serialization;

public class XMLSerializer
{
	/// <summary>
	/// Call to serialize the given object and write it to the given path.
	/// </summary>
	/// <param name="item">The object to serialize</param>
	/// <param name="path">The path to save the serialized object to</param>
	public static void Serialize(object item, string path)
	{
		// define the serializer w/ type of the given object
		XmlSerializer serializer = new XmlSerializer(item.GetType());

		// define the writer with the given path 
		StreamWriter writer = new StreamWriter(path);

		// serialize the object to the path
		serializer.Serialize(writer.BaseStream, item);

		// close the writer
		writer.Close();
	}

	/// <summary>
	/// Call to deserialize the object at the given path.
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="path">The path to get the object from</param>
	/// <returns>A deserialized object</returns>
	public static T Deserialize<T>(string path)
	{
		// define the serializer w/ type of the given object
		XmlSerializer serializer = new XmlSerializer(typeof(T));

		// define the reader with the given path 
		StreamReader reader = new StreamReader(path);

		// deserialize the object with the serializer
		T deserialized = (T)serializer.Deserialize(reader.BaseStream);

		// close the reader
		reader.Close();

		// return the deserialized object
		return deserialized;
	}
}
