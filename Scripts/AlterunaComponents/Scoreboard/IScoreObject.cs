using System;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Defines the interface for a score object, providing methods for managing and serializing score data.
	/// </summary>
	public interface IScoreObject
	{
		/// <summary>
		/// Gets the key associated with the score object.
		/// </summary>
		string Key { get; }

		/// <summary>
		/// Retrieves an object corresponding to the given ID.
		/// </summary>
		/// <param name="id">The ID for which the object is to be retrieved.</param>
		/// <returns>The object associated with the specified ID.</returns>
		object Get(ushort id);

		/// <summary>
		/// Sets the value for the given ID.
		/// </summary>
		/// <param name="id">The ID for which the value is to be set.</param>
		/// <param name="obj">The object to set.</param>
		void Set(ushort id, object obj);

		/// <summary>
		/// Adds a new user with the specified userID to the score object.
		/// </summary>
		/// <param name="userID">The userID of the user to add.</param>
		void AddUser(ushort userID);

		/// <summary>
		/// Appends a score value for a specified user.
		/// </summary>
		/// <param name="userID">The userID for which the score is to be appended.</param>
		/// <param name="value">The score value to append.</param>
		/// <typeparam name="T">The type of the score value, must be a struct and implement IConvertible.</typeparam>
		void AppendScore<T>(ushort userID, T value) where T : struct, IConvertible;

		/// <summary>
		/// Serializes all values using the provided writer.
		/// </summary>
		/// <param name="writer">The writer to use for serialization.</param>
		void SerializeValues(Writer writer);

		/// <summary>
		/// Serializes a single value identified by the userID.
		/// </summary>
		/// <param name="writer">The writer to use for serialization.</param>
		/// <param name="userID">The user ID identifying the value to serialize.</param>
		void SerializeValue(Writer writer, ushort userID);

		/// <summary>
		/// Deserializes all values using the provided reader.
		/// </summary>
		/// <param name="reader">The reader to use for deserialization.</param>
		void DeserializeValues(Reader reader);

		/// <summary>
		/// Deserializes a single value identified by the userID.
		/// </summary>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <param name="userID">The user ID identifying the value to deserialize.</param>
		void DeserializeValue(Reader reader, ushort userID);

		/// <summary>
		/// Serializes the score object using the provided writer.
		/// </summary>
		/// <param name="writer">The writer to use for serialization.</param>
		void Serialize(Writer writer);

		/// <summary>
		/// Returns a string representation of the score object for the specified userID.
		/// </summary>
		/// <param name="userID">The user ID for which to generate the string representation.</param>
		string ToString(ushort userID);

		/// <summary>
		/// Action invoked when a value in the score object is changed.
		/// </summary>
		Action<int, IScoreObject> OnChanged { get; set; }
		
		/// <summary>
		/// Get or set the size of the value array.
		/// Size need to be as large as the highest index user in room.
		/// </summary>
		int Size { get; set; }
	}
}