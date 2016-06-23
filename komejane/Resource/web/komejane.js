axios
  .get('/api/wsInfo')
  .then((res) => {
      const wsUri = res.data.websocket.streamUri || "/stream";
      var socket = io(wsUri);
      socket.on('news', function (data) {
          console.log(data);
          socket.emit('my other event', { my: 'data' });
      });
  })
  .catch((error) => {

  })