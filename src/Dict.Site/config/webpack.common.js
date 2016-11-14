// https://angular.io/docs/ts/latest/guide/webpack.html

var webpack = require('webpack')
var HtmlWebpackPlugin = require('html-webpack-plugin')
var ExtractTextPlugin = require('extract-text-webpack-plugin')
var helpers = require('./helpers')

module.exports = {
  entry: {
    'polyfills': './src/polyfills.ts',
    'vendor': './src/vendor.ts',
    'main': './src/main.ts'
  },

  resolve: {
    extensions: ['', '.js', '.ts']
  },

  module: {
    loaders: [
      // ts - a loader to transpile our Typescript code to ES5, guided by the tsconfig.json file
      // angular2-template-loader - loads angular components' template and styles
      {
        test: /\.ts$/,
        loaders: ['ts', 'angular2-template-loader'],
        exclude: ['node_modules']
      },
      // html - for component templates
      {
        test: /\.html$/,
        loader: 'html',
        exclude: ['node_modules']
      },
      // images/fonts - Images and fonts are bundled as well.
      {
        test: /\.(png|jpe?g|gif|svg|woff|woff2|ttf|eot|ico)$/,
        loader: 'file?name=assets/[name].[hash].[ext]'
      },
      // css - The pattern matches application-wide styles
      {
        test: /\.css$/,
        exclude: helpers.root('src', 'app'),
        loader: ExtractTextPlugin.extract('style', 'css?sourceMap')
      },
      // this handles component-scoped styles (the ones specified in a component's styleUrls metadata property)
      {
        test: /\.css$/,
        include: helpers.root('src', 'app'),
        loader: 'raw'
      }
    ]
  },

  plugins: [
    // We want the app.js bundle to contain only app code and the vendor.js bundle to contain only the vendor code.
    new webpack.optimize.CommonsChunkPlugin({
      name: ['main', 'vendor', 'polyfills']
    }),

    // Webpack can inject js and css files
    new HtmlWebpackPlugin({
      template: 'src/index.html'
    })
  ]
}
