from subprocess import *
from os.path import join
import sys
import re

def find_nsis():
    register_query = "reg query HKLM\\SOFTWARE\\NSIS /ve"
    return_value = call(register_query, stdout=PIPE)
    if return_value != 0:
        raise Exception("Could not find NSIS location from the register. Please install NSIS installer system first.")
    reg_process = Popen(register_query, stdout=PIPE)
    output = reg_process.communicate()[0]
    match = re.search("[A-Z]:\\\\.*", output.strip())
    if not match:
        raise Exception("Could not parse NSIS location from register value: " + output)
    return match.group(0)

def create_installer(nsis_file):
    path = find_nsis()
    return_code = call([join(path, "makensis.exe"), nsis_file])
    if return_code != 0:
        raise Exception("Running makensis failed")

def main():
    try:
        nsis_file = "..\\installer\\jypeli.nsi"
        create_installer(nsis_file)
    except Exception as e:
        print "CREATING INSTALLER NOT SUCCESSFUL:", e
        return 1
    return 0

if __name__ == '__main__':
	sys.exit(main())

