using Alteruna;
using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;

namespace Alteruna_Examples
{
	public class ExampleRpcSerializable : CommunicationBridge, ISerializable
	{
		// Name of our RPC event.
		public const string RPC_NAME = "ExampleRpc2";

		private void Start()
		{
			// Register our remote procedure.
			Multiplayer.RegisterRemoteProcedure(RPC_NAME, RpcMethod);
		}

		// Call our rpc.
		public void SendRpc()
		{
			// Send our RPC event.
			Multiplayer.InvokeRemoteProcedure(RPC_NAME, UserId.All, null, this);
		}

		// Receive RPC event.
		private void RpcMethod(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
		{
			Unserialize(processor);
			// Log our message.
			Debug.Log(parameters.Get("msg", ""));
		}

		// Write data to the transport stream.
		public void Serialize(ITransportStreamWriter processor)
		{
			Writer writer = new Writer(processor);
			writer.Write("Hello, world!");
		}

		// Read data from the transport stream.
		public void Unserialize(ITransportStreamReader processor)
		{
			Reader reader = new Reader(processor);
			Debug.Log(reader.ReadString());
		}
	}
}