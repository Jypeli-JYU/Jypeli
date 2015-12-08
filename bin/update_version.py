from __future__ import with_statement

import sys
import os
import re
from tempfile import mkstemp


def file_findfirst(file_path, pattern):
    with open(file_path, 'r') as f:
        for line in f:
            print line
            if pattern.search(line):
                return line
    
    return None

def file_replace(file_path, pattern, replacement):
    tmph, tmpname = mkstemp()
    cpat = re.compile(pattern)
    f_src = open(file_path, 'r')
    f_tmp = os.fdopen(tmph, 'w')

    for line in f_src:
        f_tmp.write(re.sub(cpat, replacement, line))

    f_src.close()
    f_tmp.close()
    os.remove(file_path)
    os.rename(tmpname, file_path)

def main():
    vlinepattern = re.compile('^(\\s)*\\[assembly: AssemblyVersion\\(.*\\)\\]')
    verline = file_findfirst('../Jypeli/Properties/AssemblyInfo.cs', vlinepattern)
    if verline == None:
        print "Version number line not found!"
        return 1

    vpattern = re.compile('(\\d)+(\\.)(\\d)+(\\.)(\\d)+')
    vernum = re.search(vpattern, verline)
    if vernum == None:
        print "Invalid version number syntax!"
        return 1
    vernum = vernum.group(0)
    
    print "Current version number is " + vernum
    print "Enter new version number, or enter to keep the current one."
    newver = "!"
    
    while not re.match(vpattern, newver):
        newver = raw_input(">")
        if newver == '':
            return 0
    
    print "Setting new version to " + newver
    newverline = '[assembly: AssemblyVersion("%s.*")]' % (newver)
    file_replace('../Jypeli/Properties/AssemblyInfo.cs', vlinepattern, newverline)
    file_replace('../Jypeli/Properties/AssemblyInfo-Xamarin.cs', vlinepattern, newverline)
    file_replace('../Projektimallit/Xamarin/Properties/AssemblyInfo.cs', vlinepattern, newverline)
    
    addinpattern = re.compile('category="Jypeli" version=".*"')
    newaddinmatch = 'category="Jypeli" version="%s"' % newver
    file_replace('../Projektimallit/Xamarin/Properties/MonoDevelop.Jypeli.Windows.addin.xml', addinpattern, newaddinmatch)
    file_replace('../Projektimallit/Xamarin/Properties/MonoDevelop.Jypeli.Linux.addin.xml', addinpattern, newaddinmatch)
    file_replace('../Projektimallit/Xamarin/Properties/MonoDevelop.Jypeli.Mac.addin.xml', addinpattern, newaddinmatch)
    
    file_replace('../installer/jypeli.nsi', 'Name "MonoJypeli (\\d)+\\.(\\d)+\\.(\\d)+"', 'Name "MonoJypeli %s"' % newver)
    
    return 0
    
if __name__ == '__main__':
    sys.exit(main())

