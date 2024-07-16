﻿const webpack = require("webpack");
const packageJson = require("./package.json");
const path = require("path");
const settings = require("../../../settings.local.json");

const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");

module.exports = (env, argv) => {
    const isProduction = argv.mode === "production";
    return {
        entry: "./src/main.jsx",
        optimization: {
            minimize: isProduction,
        },
        output: {
            path:
        isProduction || settings.WebsitePath === ""
            ? path.resolve(
                "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Security/scripts/bundles/"
            )
            : path.join(
                settings.WebsitePath,
                "DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Security\\scripts\\bundles\\"
            ),
            publicPath: isProduction ? "" : "http://localhost:8080/dist/",
            filename: "security-settings-bundle.js",
        },
        devServer: {
            disableHostCheck: !isProduction,
        },
        resolve: {
            extensions: ["*", ".js", ".json", ".jsx"],
            modules: [
                path.resolve("./src"), // Look in src first
                path.resolve("./node_modules"), // Try local node_modules
                path.resolve("../../../node_modules"), // Last fallback to workspaces node_modules
            ],
        },
        module: {
            rules: [
                {
                    test: /\.(js|jsx)$/,
                    exclude: /node_modules/,
                    enforce: "pre",
                    use: [
                        {
                            loader: "eslint-loader",
                            options: {fix: true},
                        },
                    ],
                },
                {
                    test: /\.less$/,
                    use: [
                        {
                            loader: "style-loader", // creates style nodes from JS strings
                        },
                        {
                            loader: "css-loader",
                            options: {
                                importLoaders: 1,
                                sourceMap: true,
                                modules: {
                                    auto: true,
                                    mode: "global",
                                    localIdentName: "[name]__[local]___[hash:base64:5]",
                                },
                                esModule: false,
                            },
                        },
                        {
                            loader: "less-loader", // compiles Less to CSS
                        },
                    ],
                },
                {
                    test: /\.(js|jsx)$/,
                    exclude: /node_modules/,
                    use: {
                        loader: "babel-loader",
                        options: {
                            presets: ["@babel/preset-env", "@babel/preset-react"],
                        },
                    },
                },
                {
                    test: /\.(ttf|woff)$/,
                    use: ["url-loader?limit=8192"],
                },
            ],
        },
        externals: webpackExternals,
        plugins:
            [
                isProduction
                    ? new webpack.DefinePlugin({
                        VERSION: JSON.stringify(packageJson.version),
                        "process.env": {
                            NODE_ENV: JSON.stringify("production"),
                        },
                    })
                    : new webpack.DefinePlugin({
                        VERSION: JSON.stringify(packageJson.version),
                    }),
                new webpack.SourceMapDevToolPlugin({
                    filename: "security-settings-bundle.js.map",
                    append: "\n//# sourceMappingURL=/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Security/scripts/bundles/security-settings-bundle.js.map",
                })
            ],
        devtool: false,
    };
};
