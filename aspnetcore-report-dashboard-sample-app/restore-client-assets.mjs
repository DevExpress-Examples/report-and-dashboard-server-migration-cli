import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const projectDir = path.dirname(fileURLToPath(import.meta.url));
const modulesDir = path.join(projectDir, 'node_modules');

function resolveSource(relativePath) {
    const fullPath = path.join(modulesDir, relativePath);
    if(!fs.existsSync(fullPath))
        throw new Error(`Client asset 'node_modules/${relativePath}' was not found. Run 'npm install' and try again.`);
    return fullPath;
}

function ensureDir(relativePath) {
    const fullPath = path.join(projectDir, relativePath);
    fs.mkdirSync(fullPath, { recursive: true });
    return fullPath;
}

function copyFile(relativeSource, destinationDir, newName) {
    const from = resolveSource(relativeSource);
    fs.copyFileSync(from, path.join(ensureDir(destinationDir), newName ?? path.basename(from)));
}

function copyDirectory(relativeSource, destinationDir) {
    fs.cpSync(resolveSource(relativeSource), ensureDir(destinationDir), { recursive: true, force: true });
}

function copyFiles(relativeSource, destinationDir, namePrefix = '') {
    const from = resolveSource(relativeSource);
    const to = ensureDir(destinationDir);
    for(const entry of fs.readdirSync(from, { withFileTypes: true })) {
        if(entry.isFile() && entry.name.startsWith(namePrefix))
            fs.copyFileSync(path.join(from, entry.name), path.join(to, entry.name));
    }
}

if(!process.argv.includes('--no-install')) {
    console.log('Running npm install...');
    execFileSync('npm', ['install'], { cwd: projectDir, stdio: 'inherit', shell: process.platform === 'win32' });
}

console.log('Copying CSS...');
copyFile('devextreme-dist/css/dx.light.css', 'wwwroot/css');
copyDirectory('devexpress-richedit/dist/icons', 'wwwroot/css/icons');
copyFiles('devextreme-dist/css/icons', 'wwwroot/css/icons', 'dxicons.');
copyDirectory('devextreme-dist/css/fonts', 'wwwroot/css/fonts');
copyFile('devexpress-richedit/dist/dx.richedit.css', 'wwwroot/css');

console.log('Copying JS...');
copyFile('devextreme-dist/js/dx.all.js', 'wwwroot/js');
copyFile('devexpress-richedit/dist/dx.richedit.min.js', 'wwwroot/js');

console.log('Copying Reporting...');
copyDirectory('devexpress-reporting/dist/css', 'wwwroot/xtrareportsjs/css');
copyDirectory('@devexpress/analytics-core/dist/js', 'wwwroot/xtrareportsjs/js');
copyFiles('devexpress-reporting/dist/js', 'wwwroot/xtrareportsjs/js');
copyFiles('@devexpress/analytics-core/dist/css', 'wwwroot/xtrareportsjs/css');

console.log('Copying Dashboard...');
copyDirectory('devexpress-dashboard/dist/css', 'wwwroot/css/dashboard');
copyFile('devexpress-dashboard/dist/js/dx-dashboard.min.js', 'wwwroot/js');

console.log('Copying Thirdparty...');
copyFile('knockout/build/output/knockout-latest.js', 'wwwroot/js', 'knockout.js');
copyFile('jquery/dist/jquery.min.js', 'wwwroot/js');
copyFile('jszip/dist/jszip.min.js', 'wwwroot/js');
copyFile('bootstrap/dist/js/bootstrap.bundle.min.js', 'wwwroot/js');
copyFile('bootstrap/dist/css/bootstrap.min.css', 'wwwroot/css');

console.log('Copying Ace...');
copyFile('ace-builds/src-min-noconflict/ace.js', 'wwwroot/js/ace');
copyFile('ace-builds/src-min-noconflict/ext-language_tools.js', 'wwwroot/js/ace');
copyDirectory('ace-builds/src-min-noconflict/snippets', 'wwwroot/js/ace/snippets');
copyFile('ace-builds/css/ace.css', 'wwwroot/css');
copyFile('ace-builds/css/theme/ambiance.css', 'wwwroot/css/theme');
copyFile('ace-builds/css/theme/dreamweaver.css', 'wwwroot/css/theme');

console.log('Copying D3...');
copyFile('d3/dist/d3.min.js', 'wwwroot/Content/d3-funnel');
copyFile('d3-funnel/dist/d3-funnel.min.js', 'wwwroot/Content/d3-funnel');

console.log('Finished copying client dependencies to wwwroot.');
