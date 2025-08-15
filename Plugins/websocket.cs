#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Alteruna
{
	public class WebSocketImplementation : Multiplayer.Core.Transport.WebSocket.IWebSocketImplementation
	{
		private static WebSocketImplementation _instance = new WebSocketImplementation();

		private static Action<byte[], int, int> OnMessage;
		private static Action OnOpen;

		[RuntimeInitializeOnLoadMethod]
		public static void Init() => Multiplayer.Core.Transport.WebSocket.Implementation.Instance = _instance;

		[DllImport("__Internal")]
		private static extern void WebSocketInit(IntPtr url, Action openCallback, Action<byte[], int, int> msgCallback);

		[DllImport("__Internal")]
		private static extern void WebSocketSend(byte[] data, int length);

		[DllImport("__Internal")]
		private static extern void WebSocketSendStr(IntPtr msgPtr);

		[DllImport("__Internal")]
		private static extern void WebSocketClose();

		[MonoPInvokeCallback(typeof(Action<byte[], int, int>))]
		public static void ReceiveMsg([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 1)] byte[] data, int lenght, int opcode)
		{
			//Debug.Log("ReceiveMsg of type " + opcode);
			OnMessage(data, lenght, opcode);
		}

		[MonoPInvokeCallback(typeof(Action))]
		public static void Open()
		{
			Debug.Log("WebSocket open");
			OnOpen();
		}

		public void WsInit(IntPtr url, Action openCallback, Action<byte[], int, int> msgCallback)
		{
			Debug.Log("WebSocket init");
			OnMessage = msgCallback;
			OnOpen = openCallback;
			WebSocketInit(url, Open, ReceiveMsg);
		}

		public void WsSend(byte[] data, int length) => WebSocketSend(data, length);

		public void WsSendStr(IntPtr msgPtr) => WebSocketSendStr(msgPtr);

		public void WsClose() => WebSocketClose();
	}
}
#endif