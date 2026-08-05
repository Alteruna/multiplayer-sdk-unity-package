using System;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Represents a score object holding a list of values of type T.
	/// This class manages the storage, retrieval, and manipulation of score data.
	/// </summary>
	/// <typeparam name="T">The type of the score data.</typeparam>
	public class ScoreObject<T> : IScoreObject where T : struct, IConvertible
	{
		/// <inheritdoc />
		public string Key { get; }

		private T[] _value;

		private Func<T, T, T> _adder;

		/// <inheritdoc />
		public Action<int, IScoreObject> OnChanged { get; set; }

		/// <summary>
		/// Gets or sets the list of values. Setting this property invokes OnChanged.
		/// </summary>
		public T[] Value
		{
			get => _value;
			set
			{
				_value = value;
				OnChanged?.Invoke(-1, this);
			}
		}

		public ScoreObject(string name, T[] obj, Func<T, T, T> adder)
		{
			Key = name;
			_value = obj;
			_adder = adder;
		}

		public ScoreObject(string name, T[] obj)
		{
			Key = name;
			_value = obj;
		}

		public ScoreObject(string name)
		{
			Key = name;
			_value = new T[1];
		}

		public T Get(ushort id)
		{
			if (_value.Length <= id) AddUser(id, true);
			return _value[id];
		}

		object IScoreObject.Get(ushort id) => Get(id);

		public void Set(ushort id, T obj)
		{
			if (_value.Length <= id)
			{
				AddUser(id, true, obj);
			}
			else
			{
				_value[id] = obj;
				OnChanged?.Invoke(id, this);
			}
		}

		/// <inheritdoc />
		public void Set(ushort id, object obj)
		{
			if (obj is T foo)
			{
				Set(id, foo);
			}
			else
			{
				throw new InvalidCastException("Cannot cast " + obj.GetType() + " to " + typeof(T));
			}
		}

		/// <inheritdoc />
		void IScoreObject.AddUser(ushort userID)
		{
			AddUser(userID, _value.Length <= userID);
		}

		private void AddUser(ushort userID, bool v, T defaultValue = default)
		{
			if (v)
			{
				T[] newArray = new T[userID + 1];
				Array.Copy(_value, newArray, _value.Length);
				_value = newArray;
			}
			_value[userID] = defaultValue;
			OnChanged?.Invoke(userID, this);
		}

		/// <inheritdoc />
		public void AppendScore<T2>(ushort userID, T2 value) where T2 : struct, IConvertible
		{
			if (typeof(T2) == typeof(T))
			{
				// Safe to cast value to T because T2 is the same as T
				AppendScore(userID, (T)(object)value);
			}
			else
			{
				throw new InvalidOperationException("Type mismatch");
			}
		}

		/// <summary>
		/// Appends a score value for a specified user.
		/// </summary>
		/// <param name="userID">The userID for which the score is to be appended.</param>
		/// <param name="value">The score value to append.</param>
		/// <typeparam name="T">The type of the score value, must be a struct and implement IConvertible.</typeparam>
		public void AppendScore(ushort userID, T value)
		{
			if (_value.Length <= userID)
			{
				AddUser(userID, true, value);
			}
			else
			{
				_value[userID] = _adder(_value[userID], value);
				OnChanged?.Invoke(userID, this);
			}
		}

		public override string ToString() => Key;

		/// <inheritdoc />
		string IScoreObject.ToString(ushort userID) => _value[userID].ToString();

		/// <inheritdoc />
		public void SerializeValues(Writer writer)
		{
			writer.Write((ushort)_value.Length);
			foreach (var item in _value)
			{
				writer.WriteGeneric<T>(item);
			}
		}

		/// <inheritdoc />
		void IScoreObject.SerializeValue(Writer writer, ushort userID)
		{
			writer.WriteGeneric<T>(_value[userID]);
		}

		/// <inheritdoc />
		public void DeserializeValues(Reader reader)
		{
			int l = reader.ReadUshort();
			_value = new T[l];
			for (int i = 0; i < l; i++)
			{
				_value[i] = reader.ReadGeneric<T>();
			}
			OnChanged?.Invoke(-1, this);
		}

		/// <inheritdoc />
		void IScoreObject.DeserializeValue(Reader reader, ushort userID)
		{
			if (_value.Length <= userID)
			{
				AddUser(userID, true, reader.ReadGeneric<T>());
			}
			else
			{
				_value[userID] = reader.ReadGeneric<T>();
				OnChanged?.Invoke(userID, this);
			}
		}

		/// <inheritdoc />
		void IScoreObject.Serialize(Writer writer)
		{
			writer.Write((byte)ScoreTypeMethods.TypeToScoreType<T>());
			writer.Write(Key);
			SerializeValues(writer);
		}

		/// <inheritdoc />
		int IScoreObject.Size
		{
			get => _value.Length;
			set => Array.Resize(ref _value, value);
		}
	}
}