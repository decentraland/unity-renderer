"use strict";
// tslint:disable-next-line:no-commented-out-code
// tslint:disable:no-console
Object.defineProperty(exports, "__esModule", { value: true });
const path = require("path");
const fs_extra_1 = require("fs-extra");
const _utils_1 = require("./_utils");
const root = path.resolve(__dirname, '../packages/decentraland-ecs');
const original = _utils_1.ensureFileExists(root, '/dist/index.d.ts');
_utils_1.copyFile(original, root + '/types/dcl/index.d.ts');
const dtsFile = _utils_1.ensureFileExists(root, '/types/dcl/index.d.ts');
{
    const content = fs_extra_1.readFileSync(dtsFile).toString();
    fs_extra_1.writeFileSync(dtsFile, content.replace(/^export /gm, ''));
    if (content.match(/\bimport\b/)) {
        throw new Error(`The file ${dtsFile} contains imports:\n${content}`);
    }
    if (content.includes('/// <ref')) {
        throw new Error(`The file ${dtsFile} contains '/// <ref':\n${content}`);
    }
}
