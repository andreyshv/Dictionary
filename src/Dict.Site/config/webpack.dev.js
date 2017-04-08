var webpackMerge = require('webpack-merge');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var commonConfig = require('./webpack.common.js');
var helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, {

  devtool: 'cheap-module-eval-source-map',

  // The HtmlWebpackPlugin (added in webpack.common.js) use the publicPath and the filename settings to generate appropriate <script> and <link> tags into the index.html
  output: {
    path: helpers.root('wwwroot'),
    publicPath: 'http://localhost:8080/',
    filename: '[name].js',
    chunkFilename: '[id].chunk.js'
  },

  // CSS are buried inside our Javascript bundles by default.
  // The ExtractTextPlugin extracts them into external .css files that the HtmlWebpackPlugin inscribes as <link> tags into the index.html
  plugins: [
    new ExtractTextPlugin('[name].css')
  ],

  devServer: {
    historyApiFallback: true,

    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        secure: false
      },
      '/media': {
        target: 'http://localhost:5000',
        secure: false
      }
    },    
    // stats: 'minimal'
  }
});
