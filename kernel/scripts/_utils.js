"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
// tslint:disable:no-console
const fs = require("fs-extra");
const path = require("path");
/**
 * @returns the resolved absolute path
 */
function ensureFileExists(root, file) {
    const x = path.resolve(root, file.replace(/^\//, ''));
    if (!fs.existsSync(x)) {
        throw new Error(`${x} does not exist`);
    }
    return x;
}
exports.ensureFileExists = ensureFileExists;
function copyFile(from, to) {
    if (!fs.existsSync(from)) {
        throw new Error(`${from} does not exist`);
    }
    fs.copySync(from, to);
    if (!fs.existsSync(to)) {
        throw new Error(`${to} does not exist`);
    }
}
exports.copyFile = copyFile;
