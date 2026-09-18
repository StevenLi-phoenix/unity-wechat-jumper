import {readFile} from 'node:fs/promises';
const [tag,settings='Jumper/ProjectSettings/ProjectSettings.asset']=process.argv.slice(2);
try {
  if(!/^v\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/.test(tag||''))throw new Error('Expected a version tag such as v1.1.0');
  const version=(await readFile(settings,'utf8')).match(/^\s*bundleVersion:\s*(\S+)\s*$/m)?.[1];
  if(version!==tag.slice(1))throw new Error('Tag '+tag+' does not match PlayerSettings.bundleVersion '+version);
  console.log('RELEASE_VERSION_OK: '+version);
} catch(error){console.error(error.message);process.exitCode=1;}
