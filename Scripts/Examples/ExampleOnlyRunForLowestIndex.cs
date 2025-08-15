using System;
using Alteruna;
using Alteruna.Multiplayer;
using UnityEngine;

namespace Alteruna_Examples
{
	[RequireComponent(typeof(SingleClientEvent))]
	public class ExampleOnlyRunForLowestIndex : CommunicationBridge
	{
		[NonSerialized]
		private SingleClientEvent _singleClientEvent;
		
		private void Awake()
		{
			_singleClientEvent = GetComponent<SingleClientEvent>();
			_singleClientEvent.OnClientChanged.AddListener(OnClientChanged);
		}

		private void OnClientChanged(bool isControlled)
		{
			enabled = isControlled;
		}
		
		public void Update()
		{
			// Only run on the client with the lowest user index
		}
		
		// This is an alternative way to do the same thing but without needing to disable the component
		/*
		public void Update()
		{
			if (!_singleClientEvent.IsControlled) return;
			// Only run on the client with the lowest user index
		}
		*/
	}
}