using System;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Defines the types of scores that can be managed within the scoreboard system.
	/// This enumeration is used to specify the data type of the scores being handled,
	/// allowing for flexible and type-safe management of score data.
	/// </summary>
	public enum ScoreType : byte
	{
		Byte,
		Ushort,
		Uint,
		Int,
		Float,
		Double
	}

	/// <summary>
	/// Provides extension methods for the ScoreType enumeration.
	/// This class is used to dynamically create IScoreObject instances based on different ScoreTypes.
	/// It simplifies the creation of score objects for various data types without hardcoding specific types.
	/// </summary>
	public static class ScoreTypeMethods
	{
		/// <summary>
		/// Converts a ScoreType to an IScoreObject with the specified name and capacity.
		/// </summary>
		/// <param name="type">The ScoreType to convert.</param>
		/// <param name="name">The name for the new IScoreObject.</param>
		/// <param name="capacity">The initial capacity for the ScoreObject, defaulting to 1.</param>
		/// <returns>An IScoreObject of the specified type, defaulting to <c>System.Object</c>.</returns>
		public static IScoreObject TypeToScoreObject(this ScoreType type, string name, int capacity = 1)
		{
			switch (type)
			{
				case ScoreType.Byte:
					return new ScoreObject<byte>(name, new byte[capacity], (a, b) => (byte)(a + b));
				case ScoreType.Ushort:
					return new ScoreObject<ushort>(name, new ushort[capacity], (a, b) => (ushort)(a + b));
				case ScoreType.Uint:
					return new ScoreObject<uint>(name, new uint[capacity], (a, b) => a + b);
				case ScoreType.Int:
					return new ScoreObject<int>(name, new int[capacity], (a, b) => a + b);
				case ScoreType.Float:
					return new ScoreObject<float>(name, new float[capacity], (a, b) => a + b);
				case ScoreType.Double:
					return new ScoreObject<double>(name, new double[capacity], (a, b) => a + b);
				default:
					throw new NotImplementedException("ScoreType " + type + " not implemented.");
			}
		}
		
		/// <summary>
		/// Get ScoreType from a Type.
		/// </summary>
		/// <param name="type">The Type to convert.</param>
		/// <returns>The ScoreType corresponding to the specified Type.</returns>
		public static ScoreType TypeToScoreType(Type type)
		{
			if (type == typeof(byte))
				return ScoreType.Byte;
			if (type == typeof(ushort))
				return ScoreType.Ushort;
			if (type == typeof(uint))
				return ScoreType.Uint;
			if (type == typeof(int))
				return ScoreType.Int;
			if (type == typeof(float))
				return ScoreType.Float;
			if (type == typeof(double))
				return ScoreType.Double;
			throw new NotImplementedException("Type " + type + " not implemented.");
		}
		
		public static ScoreType TypeToScoreType<T>() => TypeToScoreType(typeof(T));
	}
}