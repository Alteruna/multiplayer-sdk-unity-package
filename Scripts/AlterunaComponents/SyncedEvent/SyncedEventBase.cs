using System;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine.Events;

namespace Alteruna.Multiplayer.Unity
{
	
	/// <summary>
	/// Base class for syncronizing events with any type of argument.
	/// </summary>
	/// <remarks>
	///	Cannot be used as a compoment but can be inherited to create any type of SyncedEvent.
	/// To sync an event with no arguments, <see cref="SyncedEventVoid">Synced Event &lt;Void&gt;</see>.
	/// </remarks>
	/// <example>
	/// You can create a new SyncedEvent of any type by inheriting from SyncedEventBase.
	/// Here is an example of creating a SyncedEvent for int64 (long).
	/// <code>
	/// // Create a new SyncedEvent of given type by inheriting from SyncedEventBase.
	///	public class SyncedEventLong : Alteruna.SyncedEventBase&lt;long&gt; { }
	/// </code>
	/// </example>
	/// <typeparam name="T">Type of the argument that is passed in the event.</typeparam>
	public class SyncedEventBase<T> : Synchronizable, ISyncedEventType
	{
	
		/// <summary>
		/// Event to be invoked.
		/// </summary>
		public UnityEvent<T> OnEvent;

		/// <summary>
		/// Last value used in the event.
		/// </summary>
		[NonSerialized]
		protected T Value;

		/// <summary>
		/// Last value used in the event.
		/// </summary>
		public T LastValue => Value;
		
		/// <summary>
		/// True if the event has been invoked previously.
		/// </summary>
		public bool HaveBeenInvoked { get; private set; }

		/// <summary>
		/// Invoke the event with the given argument.
		/// </summary>
		/// <param name="arg">passed object</param>
		public void Invoke(T arg)
		{
			HaveBeenInvoked = true;
			OnEvent.Invoke(Value = arg);
			Multiplayer.Sync(this, Reliability);
		}
		
		/// <summary>
		/// Invoke the event with the last used argument.
		/// </summary>
		public void Invoke() => Invoke(Value);

		/// <summary>
		/// Invoke without triggering local event with the given argument.
		/// </summary>
		/// <param name="arg">passed object</param>
		public void InvokeSilent(T arg)
		{
			HaveBeenInvoked = true;
			Value = arg;
			Multiplayer.Sync(this, Reliability);
		}
		
		/// <summary>
		/// Invoke without triggering local event with the last used argument.
		/// </summary>
		public void InvokeSilent() => InvokeSilent(Value);

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			if (HaveBeenInvoked)
			{
				writer.Write(true);
				writer.WriteGeneric(Value);
			}
			else
			{
				writer.Write(false);
			}
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			HaveBeenInvoked = reader.ReadBool();
			if (HaveBeenInvoked)
			{
				Value = reader.ReadGeneric<T>();
				OnEvent.Invoke(Value);
			}
		}

		public new void Reset()
		{
			base.Reset();
			Reliability = Reliability.Reliable;
		}
		
		/// <summary>
		/// Get the last used argument as string.
		/// </summary>
		public string ValueToString() => Value.ToString();
	}

	public interface ISyncedEventType : ISyncedEvent
	{
		/// <summary>
		/// Get the last used argument as string.
		/// </summary>
		string ValueToString();
	}

	public interface ISyncedEvent
	{
		
		/// <summary>
		/// True if the event has been invoked previously.
		/// </summary>
		bool HaveBeenInvoked { get; }
		
		/// <summary>
		/// Invoke the event with the last used argument.
		/// </summary>
		void Invoke();
		
		/// <summary>
		/// Invoke without triggering local event with the last used argument.
		/// </summary>
		void InvokeSilent();
	}
}