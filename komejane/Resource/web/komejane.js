var ws = null;

axios
  .get('/api/wsInfo')
  .then((res) => {
    const wsUri = res.data.websocket.streamUri || "/stream";
    if (ws == null) {
      ws = new WebSocket("ws://" + location.host + wsUri);//, ['soap', 'xmpp']);
      var pingInterval = null;

      ws.onopen = () => {
        ws.send("connect komejane client");
        pingInterval = setInterval(() => {
          console.log("ping");
          ws.send("/ping");
        }, 1000)
      }

      ws.onmessage = (msg) => {
        if (msg.type == 'message') {
          switch (msg.data) {
            case "/pong":
              console.log("pong☆");
              break;
          }
        }
      }

      ws.onerror = (err) => {
        console.log(err);
      }

      ws.onclose = () => {
        if (pingInterval != null)
          clearInterval(pingInterval);
      }
    }
  })
  .catch((error) => {

  })