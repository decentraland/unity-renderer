#!/usr/bin/env node
"use strict";
// tslint:disable:no-console
Object.defineProperty(exports, "__esModule", { value: true });
const fs = require("fs-extra");
const path = require("path");
const fs_extra_1 = require("fs-extra");
const child_process_1 = require("child_process");
const root = path.resolve(__dirname, '..');
const commitHash = child_process_1.execSync('git rev-parse HEAD')
    .toString()
    .trim();
function replaceVersion(placeholder) {
    const targetIndexHtml = path.resolve(root, 'static/index.html');
    if (!fs.existsSync(targetIndexHtml)) {
        throw new Error(`${targetIndexHtml} does not exist`);
    }
    const version = process.env.CIRCLE_TAG || process.env.TRAVIS_TAG || commitHash;
    console.log(`> replace '${placeholder}' -> '${version}' in html`);
    {
        let content = fs_extra_1.readFileSync(targetIndexHtml).toString();
        if (!content.includes(`${placeholder}`)) {
            throw new Error(`index.html is dirty and does\'t contain the text '${placeholder}'`);
        }
        content = content.replace(new RegExp(placeholder, 'g'), version);
        fs_extra_1.writeFileSync(targetIndexHtml, content);
    }
}
// tslint:disable-next-line:semicolon
;
(async function () {
    replaceVersion('EXPLORER_VERSION');
})().catch(e => {
    // tslint:disable-next-line:no-console
    console.error(e);
    process.exit(1);
});
