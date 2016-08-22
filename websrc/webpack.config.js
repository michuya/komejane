module.exports = {
  context: __dirname,
  entry: {
    'komejane/Resource/web/': './src/komejane.js',
    'KomejaneUserTest/bin/Debug/komejane/web/': './src/komejane.js'
  },
  output: {
    path: __dirname + '/../',
    filename: '[name]komejane.js'
  },
  module: {
    loaders: [
      { 
        test: /\.js$/, 
        exclude: /node_modules/, 
        loader: "babel", 
        query:{
          presets: ['react', 'es2015', 'stage-3']
        }
      }
    ]
  },
  devServer: {
    proxy: [
      { "path": "/api/*", "target": "http://localhost:4815" },
      { "path": "/stream", "target": "ws://localhost:4815", "ws": true }
    ],
    contentBase: __dirname + '/../komejane/Resource/web/',
    port: 4816
  }
};