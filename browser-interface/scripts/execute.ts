import { exec } from 'child_process';

export async function execute(command: string, workingDirectory: string): Promise<string> {
  return new Promise<string>((onSuccess, onError) => {
    exec(command, { cwd: workingDirectory }, (error, stdout, stderr) => {
      stdout.trim().length && console.log('stdout:\n' + stdout.replace(/\n/g, '\n  '));
      stderr.trim().length && console.error('stderr:\n' + stderr.replace(/\n/g, '\n  '));

      if (error) {
        onError(stderr);
      } else {
        onSuccess(stdout);
      }
    });
  });
}
