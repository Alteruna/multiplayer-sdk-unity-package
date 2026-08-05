using System;
using System.Collections.Generic;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Provides static extension methods for managing and manipulating score objects within a array of IScoreObjects.
	/// </summary>
	public static class ScoreObjectMethods
	{
		private const string KEY_OF_TYPE_NOT_FOUND_ERROR = "No score of type {0} with key \"{1}\" found.";
		private const string KEY_NOT_FOUND_ERROR = "No score with key \"{0}\" found.";
		private const string INDEX_OF_TYPE_NOT_FOUND_ERROR = "No score of type {0} with index {1} found.";

		/// <summary>
		/// Retrieves a ScoreObject of a specific type from a array of IScoreObjects.
		/// </summary>
		/// <typeparam name="T">The type of the ScoreObject to retrieve.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects to search through.</param>
		/// <param name="key">The key of the ScoreObject to find.</param>
		/// <returns>A ScoreObject of the specified type.</returns>
		public static ScoreObject<T> GetScore<T>(this List<IScoreObject> scoreList, string key) where T : struct, IConvertible
		{
			foreach (var scoreObject in scoreList)
			{
				if (scoreObject is ScoreObject<T> so && so.Key == key)
				{
					return so;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_OF_TYPE_NOT_FOUND_ERROR, typeof(T), key));
		}
		
		/// <summary>
		/// Retrieves a IScoreObject identified by a key from a array of IScoreObjects.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects to search through.</param>
		/// <param name="key">The key of the ScoreObject to find.&lt;/param&gt;
		/// <returns>A IScoreObject.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if no score with the specified key found.</exception>
		public static IScoreObject GetScore(this List<IScoreObject> scoreList, string key)
		{
			foreach (var scoreObject in scoreList)
			{
				if (scoreObject.Key == key)
				{
					return scoreObject;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_NOT_FOUND_ERROR, key));
		}

		/// <summary>
		/// Retrieves a ScoreObject by its ID from a array of IScoreObjects.
		/// </summary>
		/// <typeparam name="T">The type of the ScoreObject to retrieve.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects to search through.</param>
		/// <param name="id">The ID of the ScoreObject to find.</param>
		/// <returns>A ScoreObject of the specified type.</returns>
        /// <exception cref="InvalidTypeException">Thrown if no score of the specified type with the given ID is found.</exception>
		public static ScoreObject<T> GetScore<T>(this List<IScoreObject> scoreList, int scoreId) where T : struct, IConvertible
		{
			var score = scoreList[scoreId];
			if (score is ScoreObject<T> so)
			{
				return so;
			}

			throw new ArgumentException(string.Format(INDEX_OF_TYPE_NOT_FOUND_ERROR, typeof(T), scoreId));
		}
		
		/// <summary>
		/// Retrieves the ID of a ScoreObject identified by a key from an array of IScoreObjects.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects to search through.</param>
		/// <param name="key">The key of the ScoreObject to find the ID for.</param>
		/// <returns>The ID of the ScoreObject, if found.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if no score with the given key is found.</exception>
		public static int GetScoreID(this List<IScoreObject> scoreList, string key)
		{
			for (int i = 0; i < scoreList.Count; i++)
			{
				if (scoreList[i].Key == key)
				{
					return i;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_NOT_FOUND_ERROR, key));
		}
		
		/// <summary>
		/// Retrieves the ID of a ScoreObject of a specific type, identified by a key, from an array of IScoreObjects.
		/// </summary>
		/// <typeparam name="T">The type of the ScoreObject to retrieve the ID for.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects to search through.</param>
		/// <param name="key">The key of the ScoreObject to find the ID for.</param>
		/// <returns>The ID of the ScoreObject, if found.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if no score of the specified type with the given key is found.</exception>
		public static int GetScoreID<T>(this List<IScoreObject> scoreList, string key) where T : struct, IConvertible
		{
			for (int i = 0; i < scoreList.Count; i++)
			{
				if (scoreList[i] is ScoreObject<T> && scoreList[i].Key == key)
				{
					return i;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_OF_TYPE_NOT_FOUND_ERROR, typeof(T), key));
		}
		
		/// <summary>
		/// Retrieves the ID of a given IScoreObject from an array of IScoreObjects.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects to search through.</param>
		/// <param name="score">The IScoreObject to find the ID for.</param>
		/// <returns>The ID of the specified IScoreObject, if found.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the IScoreObject is not found in the array.</exception>
		public static int GetScoreID(this List<IScoreObject> scoreList, IScoreObject score)
		{
			for (int i = 0; i < scoreList.Count; i++)
			{
				if (scoreList[i] == score)
				{
					return i;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_NOT_FOUND_ERROR, score.Key));
		}
		
		/// <summary>
		/// Adds a user with the specified userID to all IScoreObjects in the array.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects to add the user to.</param>
		/// <param name="userID">The userID of the user to add.</param>
		public static void AddUser(this List<IScoreObject> scoreList, ushort userID)
		{
			foreach (var scoreObject in scoreList)
			{
				scoreObject.AddUser(userID);
			}
		}
		
		/// <summary>
		/// Appends a score value for a specified user, identified by a key, in a ScoreObject from an array of IScoreObjects.
		/// </summary>
		/// <typeparam name="T">The type of the score value to append.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="key">The key of the ScoreObject to append the score to.</param>
		/// <param name="userID">The userID for which the score is to be appended.</param>
		/// <param name="value">The score value to append.</
		public static void AppendScore<T>(this List<IScoreObject> scoreList, string key, ushort userID, T value) where T : struct, IConvertible =>
			scoreList.GetScore<T>(key).AppendScore(userID, value);
		
		/// <summary>
		/// Appends a score value for a specified user, identified by an ID, in a ScoreObject from an array of IScoreObjects.
		/// </summary>
		/// <typeparam name="T">The type of the score value to append.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="scoreId">The ID of the ScoreObject to append the score to.</param>
		/// <param name="userID">The userID for which the score is to be appended.</param>
		/// <param name="value">The score value to append.</param>
		/// <exception cref="InvalidTypeException">Thrown if no score of the specified type with the given ID is found.</exception>
		public static void AppendScore<T>(this List<IScoreObject> scoreList, int scoreId, ushort userID, T value) where T : struct, IConvertible =>
			scoreList.GetScore<T>(scoreId).AppendScore(userID, value);

		/// <summary>
		/// Retrieves the score of type T for the given key and userID.
		/// </summary>
		/// <typeparam name="T">The type of the score to retrieve.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects to search.</param>
		/// <param name="key">The key of the score object.</param>
		/// <param name="userID">The userID for which the score is to be retrieved.</param>
		/// <returns>The score of type T.</returns>
		public static T GetScore<T>(this List<IScoreObject> scoreList, string key, ushort userID) where T : struct, IConvertible =>
			scoreList.GetScore<T>(key).Get(userID);

		/// <summary>
		/// Retrieves the score of type T for the given score ID and userID.
		/// </summary>
		/// <typeparam name="T">The type of the score to retrieve.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects to search.</param>
		/// <param name="scoreId">The ID of the score object.</param>
		/// <param name="userID">The userID for which the score is to be retrieved.</param>
		/// <returns>The score of type T.</returns>
        /// <exception cref="InvalidTypeException">Thrown if no score of the specified type with the given ID is found.</exception>
		public static T GetScore<T>(this List<IScoreObject> scoreList, int scoreId, ushort userID) where T : struct, IConvertible =>
			scoreList.GetScore<T>(scoreId).Get(userID);

		/// <summary>
		/// Sets the score for the specified key with the given array of values.
		/// </summary>
		/// <typeparam name="T">The type of the score to set.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="key">The key of the score object.</param>
		/// <param name="array">The array of values to set.</param>
		public static void SetScore<T>(this List<IScoreObject> scoreList, string key, T[] array) where T : struct, IConvertible
		{
			ScoreObject<T> scoreObj = scoreList.GetScore<T>(key);
			scoreObj.Value = array;
			scoreObj.OnChanged?.Invoke(-1, scoreObj);
		}

		/// <summary>
		/// Sets the score for the specified score ID with the given array of values.
		/// </summary>
		/// <typeparam name="T">The type of the score to set.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="scoreId">The ID of the score object.</param>
		/// <param name="array">The array of values to set.</param>
        /// <exception cref="InvalidTypeException">Thrown if no score of the specified type with the given ID is found.</exception>
		public static void SetScore<T>(this List<IScoreObject> scoreList, int scoreId, T[] array) where T : struct, IConvertible
		{
			ScoreObject<T> scoreObj = scoreList.GetScore<T>(scoreId);
			scoreObj.Value = array;
			scoreObj.OnChanged?.Invoke(-1, scoreObj);
		}

		/// <summary>
		/// Sets the individual score for the specified key and userID.
		/// </summary>
		/// <typeparam name="T">The type of the score to set.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="key">The key of the score object.</param>
		/// <param name="value">The value to set for the specified userID.</param>
		/// <param name="userID">The userID for which the score is to be set.</param>
		public static void SetScore<T>(this List<IScoreObject> scoreList, string key, T value, ushort userID) where T : struct, IConvertible =>
			scoreList.GetScore<T>(key).Set(userID, value);

		/// <summary>
		/// Sets the individual score for the specified score ID and userID.
		/// </summary>
		/// <typeparam name="T">The type of the score to set.</typeparam>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="scoreId">The ID of the score object.</param>
		/// <param name="value">The value to set for the specified userID.</param>
		/// <param name="userID">The userID for which the score is to be set.</param>
        /// <exception cref="InvalidTypeException">Thrown if no score of the specified type with the given ID is found.</exception>
		public static void SetScore<T>(this List<IScoreObject> scoreList, int scoreId, T value, ushort userID) where T : struct, IConvertible =>
			scoreList.GetScore<T>(scoreId).Set(userID, value);

#region full
		
		/// <summary>
		/// Serializes the array of IScoreObjects.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects to serialize.</param>
		/// <param name="writer">The writer to use for serialization.</param>
		public static void SerializeList(this List<IScoreObject> scoreList, Writer writer)
		{
			writer.Write((ushort)scoreList.Count);
			foreach (var scoreObject in scoreList)
			{
				scoreObject.Serialize(writer);
			}
		}
		
		/// <summary>
		/// Deserializes and returns an IScoreObject from the reader.
		/// </summary>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <returns>The deserialized IScoreObject.</returns>
		public static IScoreObject Deserialize(Reader reader)
		{
			IScoreObject scoreObject = ((ScoreType)reader.ReadByte()).TypeToScoreObject(reader.ReadString());
			scoreObject.DeserializeValues(reader);
			return scoreObject;
		}

		/// <summary>
		/// Deserializes and returns a array of IScoreObjects from the reader.
		/// </summary>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <returns>A array of deserialized IScoreObjects.</returns>
		public static List<IScoreObject> DeserializeList(Reader reader)
		{
			int count = reader.ReadUshort();
			List<IScoreObject> scoreList = new List<IScoreObject>(count);
			for (int i = 0; i < count; i++)
			{
				scoreList.Add(Deserialize(reader));
			}

			return scoreList;
		}
		
#endregion
#region Values
		
		/// <summary>
		/// Serializes the values of the IScoreObjects in the array.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects to serialize.</param>
		/// <param name="writer">The writer to use for serialization.</param>
		public static void SerializeValues(this List<IScoreObject> scoreList, Writer writer)
		{
			foreach (var scoreObject in scoreList)
			{
				scoreObject.SerializeValues(writer);
			}
		}
		
		/// <summary>
		/// Deserializes the values of IScoreObjects in the array from the reader.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="reader">The reader to use for deserialization.</param>
		public static void DeserializeValues(this List<IScoreObject> scoreList, Reader reader)
		{
			foreach (var scoreObject in scoreList)
			{
				scoreObject.DeserializeValues(reader);
			}
		}

#endregion
#region Value name userID
		
		/// <summary>
		/// Serializes the value of a specific IScoreObject identified by name and userID.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="writer">The writer to use for serialization.</param>
		/// <param name="name">The name of the IScoreObject to serialize.</param>
		/// <param name="userID">The userID associated with the value to serialize.</param>
		public static void SerializeValue(this List<IScoreObject> scoreList, Writer writer, string name, ushort userID)
		{
			scoreList.GetScore(name).SerializeValue(writer, userID);
		}
		
		/// <summary>
		/// Deserializes the value of a specific IScoreObject identified by name and userID from the reader.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <param name="name">The name of the IScoreObject to deserialize.</param>
		/// <param name="userID">The userID associated with the value to deserialize.</param>
		public static void DeserializeValue(this List<IScoreObject> scoreList, Reader reader, string name, ushort userID)
		{
			scoreList.GetScore(name).DeserializeValue(reader, userID);
		}

#endregion
#region Value scoreID userID
		
		/// <summary>
		/// Serializes the value of a specific IScoreObject identified by scoreID and userID.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="writer">The writer to use for serialization.</param>
		/// <param name="scoreId">The ID of the IScoreObject to serialize.</param>
		/// <param name="userID">The userID associated with the value to serialize.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the scoreID is out of range of the scoreList.</exception>
		public static void SerializeValue(this List<IScoreObject> scoreList, Writer writer, int scoreId, ushort userID)
		{
			scoreList[scoreId].SerializeValue(writer, userID);
		}
		
		/// <summary>
		/// Deserializes the value of a specific IScoreObject identified by scoreID and userID from the reader.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <param name="scoreId">The ID of the IScoreObject to deserialize.</param>
		/// <param name="userID">The userID associated with the value to deserialize.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the scoreID is out of range of the scoreList.</exception>
		public static void DeserializeValue(this List<IScoreObject> scoreList, Reader reader, int scoreID, ushort userID)
		{
			scoreList[scoreID].DeserializeValue(reader, userID);
		}

#endregion
#region Values userID

		/// <summary>
		/// Serializes the values of IScoreObjects in the array for a specific userID.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="writer">The writer to use for serialization.</param>
		/// <param name="userID">The userID associated with the values to serialize.</param>
		public static void SerializeValues(this List<IScoreObject> scoreList, Writer writer, ushort userID)
		{
			foreach (var scoreObject in scoreList)
			{
				scoreObject.SerializeValue(writer, userID);
			}
		}
		
		/// <summary>
		/// Deserializes the values of IScoreObjects in the array for a specific userID from the reader.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <param name="userID">The userID associated with the values to deserialize.</param>
		public static void DeserializeValues(this List<IScoreObject> scoreList, Reader reader, ushort userID)
		{
			foreach (var scoreObject in scoreList)
			{
				scoreObject.DeserializeValue(reader, userID);
			}
		}

#endregion
#region Values name

		/// <summary>
		/// Serializes the values of a specific IScoreObject identified by name.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="writer">The writer to use for serialization.</param>
		/// <param name="name">The name of the IScoreObject to serialize.</param>
		public static void SerializeValues(this List<IScoreObject> scoreList, Writer writer, string name)
		{
			foreach (var scoreObject in scoreList)
			{
				if (scoreObject.Key == name)
				{
					scoreObject.SerializeValues(writer);
					return;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_NOT_FOUND_ERROR, name));
		}
		
		/// <summary>
		/// Deserializes the values of a specific IScoreObject identified by name from the reader.
		/// </summary>
		/// <param name="scoreList">The array of IScoreObjects.</param>
		/// <param name="reader">The reader to use for deserialization.</param>
		/// <param name="name">The name of the IScoreObject to deserialize.</param>
		public static void DeserializeValues(this List<IScoreObject> scoreList, Reader reader, string name)
		{
			foreach (var scoreObject in scoreList)
			{
				if (scoreObject.Key == name)
				{
					scoreObject.DeserializeValues(reader);
					return;
				}
			}

			throw new KeyNotFoundException(string.Format(KEY_NOT_FOUND_ERROR, name));
		}
#endregion

	}
}