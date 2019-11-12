#!/usr/bin/env node
"use strict";
// tslint:disable:no-console
Object.defineProperty(exports, "__esModule", { value: true });
const fs = require("fs-extra");
const path = require("path");
const fs_extra_1 = require("fs-extra");
const child_process_1 = require("child_process");
const _utils_1 = require("./_utils");
const root = path.resolve(__dirname, '..');
const commitHash = child_process_1.execSync('git rev-parse HEAD')
    .toString()
    .trim();
const md5File = require('md5-file/promise');
async function copyIndex(filename) {
    let md5 = '';
    let newFileName = `${filename}.js`;
    console.log(`> copy ${filename}.js to ${filename}.<hash>.js`);
    {
        const src = path.resolve(root, `static/dist/${filename}.js`);
        if (!fs.existsSync(src)) {
            throw new Error(`${src} does not exist`);
        }
        md5 = await md5File(src);
        newFileName = `${filename}.${md5}.js`;
        const dst = path.resolve(root, `static/dist/${newFileName}`);
        await fs.copy(src, dst);
        if (!fs.existsSync(dst)) {
            throw new Error(`${dst} does not exist`);
        }
    }
    const targetIndexHtml = path.resolve(root, 'static/index.html');
    if (!fs.existsSync(targetIndexHtml)) {
        throw new Error(`${targetIndexHtml} does not exist`);
    }
    console.log(`> replace ${filename}.js -> ${newFileName} in html`);
    {
        let content = fs_extra_1.readFileSync(targetIndexHtml).toString();
        if (!content.includes(`${filename}.js`)) {
            throw new Error(`index.html is dirty and does\'t contain the text "${filename}.js"`);
        }
        content = content.replace(new RegExp(filename + '.(S+.)?js'), newFileName);
        content = content.replace(/\s*<!--(.+)-->/, '');
        content = content + `\n\n<!-- ${new Date().toISOString()} commit: ${commitHash} -->`;
        fs_extra_1.writeFileSync(targetIndexHtml, content);
    }
}
async function injectDependencies(folder, dependencies, devDependency = false) {
    console.log(`> update ${folder}/package.json (injecting dependencies)`);
    {
        const file = path.resolve(root, `${folder}/package.json`);
        const packageJson = JSON.parse(fs_extra_1.readFileSync(file).toString());
        const localPackageJson = JSON.parse(fs_extra_1.readFileSync(path.resolve(root, `package.json`)).toString());
        const deps = new Set(dependencies);
        const target = devDependency ? 'devDependencies' : 'dependencies';
        packageJson[target] = packageJson[target] || {};
        deps.forEach(dep => {
            if (localPackageJson.dependencies[dep]) {
                packageJson[target][dep] = localPackageJson.dependencies[dep];
                deps.delete(dep);
                console.log(`  using dependency: ${dep}@${packageJson[target][dep]}`);
            }
        });
        deps.forEach(dep => {
            if (localPackageJson.devDependencies[dep]) {
                packageJson[target][dep] = localPackageJson.devDependencies[dep];
                deps.delete(dep);
                console.log(`  using devDependency: ${dep}@${packageJson[target][dep]}`);
            }
        });
        if (deps.size) {
            throw new Error(`Missing dependencies "${Array.from(deps).join('", "')}"`);
        }
        fs_extra_1.writeFileSync(file, JSON.stringify(packageJson, null, 2));
    }
}
async function prepareDecentralandECS(folder) {
    await validatePackage(folder);
    _utils_1.copyFile(require.resolve('dcl-amd'), path.resolve(root, `${folder}/artifacts/amd.js`));
    _utils_1.copyFile(path.resolve(root, `packages/build-ecs/index.js`), path.resolve(root, `${folder}/artifacts/build-ecs.js`));
    await injectDependencies(folder, ['typescript', 'uglify-js'], false);
}
async function validatePackage(folder) {
    console.log(`> update ${folder}/package.json commit`);
    {
        const file = path.resolve(root, `${folder}/package.json`);
        const packageJson = JSON.parse(fs_extra_1.readFileSync(file).toString());
        packageJson.commit = commitHash;
        console.log(`  commit: ${commitHash}`);
        fs_extra_1.writeFileSync(file, JSON.stringify(packageJson, null, 2));
    }
    _utils_1.copyFile(path.resolve(root, `static/dist/preview.js`), path.resolve(root, `${folder}/artifacts/preview.js`));
    _utils_1.copyFile(path.resolve(root, `static/preview.html`), path.resolve(root, `${folder}/artifacts/preview.html`));
    _utils_1.copyFile(path.resolve(root, `static/export.html`), path.resolve(root, `${folder}/artifacts/export.html`));
    // static resources
    _utils_1.copyFile(path.resolve(root, `static/fonts`), path.resolve(root, `${folder}/artifacts/fonts`));
    _utils_1.copyFile(path.resolve(root, `static/images`), path.resolve(root, `${folder}/artifacts/images`));
    _utils_1.copyFile(path.resolve(root, `static/models`), path.resolve(root, `${folder}/artifacts/models`));
    // unity
    _utils_1.copyFile(path.resolve(root, `static/unity`), path.resolve(root, `${folder}/artifacts/unity`));
    console.log(`> ensure ${folder}/lib exists`);
    {
        if (!fs.pathExists(path.resolve(root, `${folder}/lib`))) {
            throw new Error(`${folder}/lib folder does not exist`);
        }
    }
    console.log(`> ensure ${folder}/artifacts/preview.js exists`);
    {
        if (!fs.existsSync(path.resolve(root, `${folder}/artifacts/preview.js`))) {
            throw new Error(`${folder}/artifacts/preview.js does not exist`);
        }
    }
    console.log(`> ensure ${folder}/artifacts/preview.html exists`);
    {
        if (!fs.existsSync(path.resolve(root, `${folder}/artifacts/preview.html`))) {
            throw new Error(`${folder}/artifacts/preview.html does not exist`);
        }
    }
}
// tslint:disable-next-line:semicolon
;
(async function () {
    await copyIndex('unity');
    await prepareDecentralandECS('packages/decentraland-ecs');
    _utils_1.copyFile(path.resolve(root, `static/dist/editor.js`), path.resolve(root, `packages/decentraland-ecs/artifacts/editor.js`));
    await injectDependencies('packages/build-ecs', ['typescript', 'uglify-js'], false);
})().catch(e => {
    // tslint:disable-next-line:no-console
    console.error(e);
    process.exit(1);
});
