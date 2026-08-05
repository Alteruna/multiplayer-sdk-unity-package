using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;

namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		private struct OutgoingEvent
		{
			public readonly byte Type;
			public readonly Vector3 Vector;

			public OutgoingEvent(byte type, Vector3 vector)
			{
				Type = type;
				Vector = vector;
			}

			public OutgoingEvent(byte type)
			{
				Type = type;
				Vector = Vector3.zero;
			}

			public void Write(Writer writer)
			{
				writer.Write(Type);
				if (Type < 4) writer.Write(Vector);
			}
		}
	}
}