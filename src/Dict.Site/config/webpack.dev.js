var webpackMerge = require('webpack-merge');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var commonConfig = require('./webpack.common.js');
var helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, {

  // Switch loaders to debug mode.
  debug: true,

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
    // historyApiFallback: {
    //   rewrites: [
    //       // shows views/landing.html as the landing page
    //       { from: /^\/$/, to: '/dist/index.html' },
    //       // shows views/subpage.html for all routes starting with /subpage
    //       { from: /^\/heroes/, to: '/dist/heroes' },
    //       // shows views/404.html on all other pages
    //       { from: /./, to: '/dist/404.html' },
    //   ],
    // },

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
