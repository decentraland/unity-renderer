var path = require('path')
var childProcess = require('child_process')
var webpack = require('webpack')
// var ExtractTextPlugin = require('extract-text-webpack-plugin');
var HtmlWebpackPlugin = require('html-webpack-plugin')
var AssetsPlugin = require('assets-webpack-plugin')

// const extractText = new ExtractTextPlugin("styles.css")

var entry = {
  client: ['babel-polyfill', './src/index.js'],
  preview: ['babel-polyfill', './src/preview.js']
}

function getBuildTimestamp () {
  function pad2 (value) {
    return ('0' + value).slice(-2)
  }
  var date = new Date()
  var timestamp = [
    pad2(date.getUTCDate()),
    pad2(date.getUTCMonth() + 1),
    date.getUTCFullYear()
  ]
  return timestamp.join('-')
}

var commitHash = childProcess.execSync('git rev-parse HEAD').toString()

// Minification.
var plugins = [
  new webpack.DefinePlugin({
    'process.env': {
      'NODE_ENV': JSON.stringify(process.env.NODE_ENV)
    },
    VERSION: JSON.stringify(require('./package.json').version),
    BUILD_TIMESTAMP: JSON.stringify(getBuildTimestamp()),
    COMMIT_HASH: JSON.stringify(commitHash)
  }),
  new HtmlWebpackPlugin({
    template: './src/index.html',
    chunks: ['client'],

  })
]
if (process.env.NODE_ENV === 'production') {
  // plugins.push(new webpack.optimize.UglifyJsPlugin({
  //   compress: {warnings: false}
  // }))
  plugins.push(new AssetsPlugin({filename: 'dist/assets.json'}))
} else {
  // Development
  plugins.push(new webpack.NamedModulesPlugin())
  plugins.push(new webpack.NoEmitOnErrorsPlugin())
}

let filename = '[name].js'

// dist/
var outPath = 'dist'

if (process.env.NODE_ENV === 'production') {
  filename = '[name]-[hash].min.js'
}

module.exports = {
  devServer: {
    contentBase: path.join(__dirname, 'public'),
    disableHostCheck: true,
    hot: false
  },
  devtool: process.env.NODE_ENV === 'production' ? undefined : 'eval',
  entry: entry,
  output: {
    path: path.join(__dirname, outPath),
    filename,
    publicPath: './dist/'
  },
  module: {
    rules: [
      {
        test: /\.(jpe?g|png|gif|svg)$/i,
        use: [
          {
            loader: 'url-loader',
            options: {
              limit: 512000
            }
          }
        ]
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['env'],
            plugins: ['transform-object-rest-spread', 'transform-class-properties']
          }
        }
      },
      {
        test: /\.less$/,
        loaders: ['style-loader', 'css-loader', 'less-loader']
      },
      {
        test: /\.worker\.js$/,
        use: [
          {
            loader: 'worker-loader',
            options: {
              inline: true,
              fallback: false
            }
          },
          {
            loader: 'babel-loader'
          }
        ]
      }
    ]
  },
  plugins: plugins
}
