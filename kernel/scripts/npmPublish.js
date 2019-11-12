#!/usr/bin/env node
"use strict";
// tslint:disable:no-console
Object.defineProperty(exports, "__esModule", { value: true });
const child_process_1 = require("child_process");
const node_fetch_1 = require("node-fetch");
const semver = require("semver");
const git = require("git-rev-sync");
const fs = require("fs");
const time = new Date()
    .toISOString()
    .replace(/(\..*$)/g, '')
    .replace(/([^\dT])/g, '')
    .replace('T', '');
console.log(`> oddish`);
/**
 * Use cases
 *
 *  If no version is published, pick the version from the package.json and publish that version
 *
 *  If a version is published and the minor and major matches the package.json, publish a patch
 *
 *  If the packaje.json version minor and major differs from the published version, pick the latest published patch for the version of the package.json and increment the patch number
 *
 */
async function execute(command) {
    return new Promise((onSuccess, onError) => {
        console.log(`> ${command}`);
        child_process_1.exec(command, (error, stdout, stderr) => {
            stdout.trim().length && console.log('  ' + stdout.replace(/\n/g, '\n  '));
            stderr.trim().length && console.error('! ' + stderr.replace(/\n/g, '\n  '));
            if (error) {
                onError(stderr);
            }
            else {
                onSuccess(stdout);
            }
        });
    });
}
async function getBranch() {
    return git.branch();
}
async function setVersion(newVersion) {
    return execute(`npm version "${newVersion}" --force --no-git-tag-version --allow-same-version`);
}
async function publish(npmTag = []) {
    return execute(`npm publish` + npmTag.map($ => ' "--tag=' + $ + '"').join(''));
}
async function getVersion() {
    const json = JSON.parse(fs.readFileSync('package.json', 'utf8'));
    const pkgJsonVersion = json.version;
    const version = semver.parse(pkgJsonVersion.trim());
    if (!version) {
        throw new Error('Unable to parse semver from ' + pkgJsonVersion);
    }
    return `${version.major}.${version.minor}.${version.patch}`;
}
function snapshotize(value) {
    const commit = git.short();
    if (!commit) {
        throw new Error('Unable to get git commit');
    }
    return value + '-' + time + '.commit-' + commit;
}
async function getSnapshotVersion() {
    let nextVersion = snapshotize(await getVersion());
    const versions = await getReleaseTags();
    console.log('  published versions: ' + JSON.stringify(versions));
    if (versions.latest && semver.lt(nextVersion, versions.latest)) {
        console.log(`! @latest(${versions.latest}) > ${nextVersion}. Incrementing patch.`);
        nextVersion = snapshotize(semver.inc(versions.latest, 'patch'));
    }
    if (versions.next && semver.lt(nextVersion, versions.next)) {
        console.log(`! @next(${versions.latest}) > ${nextVersion}. Incrementing patch.`);
        nextVersion = snapshotize(semver.inc(versions.next, 'patch'));
    }
    return nextVersion;
}
async function getReleaseTags() {
    try {
        const json = JSON.parse(fs.readFileSync('package.json', 'utf8'));
        const versions = await node_fetch_1.default(`https://registry.npmjs.org/-/package/${json.name}/dist-tags`);
        if (versions.ok) {
            return await versions.json();
        }
        else {
            return {};
        }
    }
    catch (_a) {
        return {};
    }
}
console.log(`  pwd: ${process.cwd()}`);
const run = async () => {
    let branch = process.env.CIRCLE_BRANCH || process.env.BRANCH_NAME || process.env.TRAVIS_BRANCH || (await getBranch());
    let npmTag = null;
    let gitTag = process.env.CIRCLE_TAG || process.env.TRAVIS_TAG || null;
    let newVersion = null;
    let linkLatest = false;
    console.log(`  branch: ${branch}`);
    console.log(`  gitTag: ${gitTag}`);
    const prerelease = semver.prerelease(gitTag);
    // Travis keeps the branch name in the tags' builds
    if (gitTag) {
        if (semver.valid(gitTag)) {
            if (semver.coerce(gitTag).version === gitTag) {
                // Contains no prerelease data and should go to latest
                npmTag = 'latest';
                linkLatest = true;
                newVersion = gitTag;
            }
            else if (prerelease && prerelease.includes('rc')) {
                // Release candidate
                npmTag = 'rc';
                newVersion = gitTag;
            }
            else if (prerelease && prerelease.includes('er')) {
                // Explorer release
                npmTag = 'er';
                newVersion = gitTag;
            }
            else {
                npmTag = 'tag-' + gitTag;
                newVersion = await getSnapshotVersion();
            }
        }
        else {
            console.log(`invalid semver version: ${gitTag}`);
            npmTag = 'tag-' + gitTag;
            newVersion = await getSnapshotVersion();
        }
    }
    else {
        newVersion = await getSnapshotVersion();
    }
    console.log(`  package.json#version: ${await getVersion()}`);
    console.log(`  publishing:\n    version: ${newVersion}`);
    console.log(`    tag: ${npmTag || 'ci'}\n`);
    if (!gitTag) {
        if (branch === 'master') {
            npmTag = 'next';
        }
        else {
            console.log(`! canceling automatic npm publish. It can only be made in master branches or tags`);
            process.exit(0);
        }
    }
    const tags = await getReleaseTags();
    if (npmTag && npmTag in tags) {
        if (semver.gte(tags[npmTag], newVersion)) {
            console.log(`! This version will be not published as "${npmTag}" because a newer version is set. Publishing as "ci"\n`);
            npmTag = null;
        }
    }
    await setVersion(newVersion);
    if (npmTag) {
        await publish([npmTag]);
    }
    else {
        await publish(['ci']);
    }
    if (linkLatest) {
        try {
            if (!tags.latest || semver.gte(newVersion, tags.latest)) {
                const pkgName = (await execute(`npm info . name`)).trim();
                await execute(`npm dist-tag add ${pkgName}@${newVersion} latest`);
            }
        }
        catch (e) {
            console.error(e);
        }
    }
    await execute(`npm info . dist-tags --json`);
};
run().catch(e => {
    console.error('Error:');
    console.error(e);
    process.exit(1);
});
