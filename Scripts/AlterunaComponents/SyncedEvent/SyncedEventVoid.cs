using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;
using UnityEngine.Events;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Sync a UnityEvent without parameters.
	/// </summary>
	/// <seealso cref="Alteruna.Multiplayer.Unity.SyncedEventBase"/>
	[AddComponentMenu("Alteruna/Event/Synced Event"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class SyncedEventVoid : Synchronizable, ISyncedEvent
	{
		public UnityEvent OnEvent;
		
		/// <summary>
		/// True if the event has been invoked previously.
		/// </summary>
		public bool HaveBeenInvoked { get; private set; }

		public new void Reset()
		{
			base.Reset();
			Reliability = Reliability.Reliable;
		}

		public void Invoke()
		{
			HaveBeenInvoked = true;
			OnEvent.Invoke();
			Multiplayer.Sync(this, Reliability);
		}

		public void InvokeSilent()
		{
			HaveBeenInvoked = true;
			Multiplayer.Sync(this, Reliability);
		}

		public override void AssembleData(Writer writer, SerializeInfo info)
		{
			writer.Write(HaveBeenInvoked);
		}

		public override void DisassembleData(Reader reader, UnserializeInfo info)
		{
			HaveBeenInvoked = reader.ReadBool();
			if (HaveBeenInvoked) OnEvent.Invoke();
		}
	}
}