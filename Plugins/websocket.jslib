mergeInto(LibraryManager.library, {
	WebSocketInit: function(urlPtr, openCallback, msgCallback) {

		// if Runtime not defined. create and add functon!!
		if(typeof Runtime === "undefined") Runtime = { dynCall : dynCall }

		var url = UTF8ToString(urlPtr);
		if (!url.startsWith("ws://") && !url.startsWith("wss://")) {
			url = "ws://" + url;
		}
		this.socket = new WebSocket(url);

		this.socket.open = function(e) {
			Runtime.dynCall('v', openCallback);
		};

		this.socket.onmessage = function(e) {
		
			if (typeof e.data === "string") {
				const length = e.data.length;
				var bytes = new Uint8Array(length);
				
				for (var i = 0; i < length; ++i) {
					bytes[i] = [e.data.charCodeAt(i)];
				}
				
				const buffer = _malloc(length);
				HEAPU8.set(bytes, buffer);
				
				Runtime.dynCall('viii', msgCallback, [buffer, length, 1]);
				_free(buffer);
				
			} else {
				e.data.arrayBuffer().then(function(buffer) {
					const byteArray = new Uint8Array(buffer);
					const length = byteArray.length;
					const bufferPtr = _malloc(length);
					HEAPU8.set(byteArray, bufferPtr);
					Runtime.dynCall('viii', msgCallback, [bufferPtr, length, 2]);
					_free(bufferPtr);
				});
			}
		};
	},
	WebSocketSend: function(byteArrayPtr, length) {
		var data = new Uint8Array(length);
		for (var i = 0; i < length; i++) {
			data[i] = HEAPU8[byteArrayPtr + i];
		}
		this.socket.send(data);
	},
	WebSocketSendStr: function(messagePtr) {
		var msg = UTF8ToString(messagePtr);
		this.socket.send(msg);
	},
	WebSocketClose: function() {
		this.socket.close();
	}
});
